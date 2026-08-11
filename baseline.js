import http from 'k6/http';
import { check, fail } from 'k6';
import { Trend } from 'k6/metrics';

// Dois baldes separados: cada tipo de página tem sua própria medição.
const pageShallow = new Trend('dur_pagina_1', true);      // caso fácil
const pageDeep    = new Trend('dur_pagina_profunda', true); // caso que dói (OFFSET grande)
const pageKeyset  = new Trend('dur_pagina_keyset', true);   // caso com keyset
const email = __ENV.API_EMAIL;
const senha = __ENV.API_SENHA;

export function setup() {
  const res = http.post('http://localhost:8080/api/Auth', JSON.stringify({ email: email, senha: senha }), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, { 'login -> 200': (r) => r.status === 200 });

  if(res.status !== 200) {
    fail(`Falha no login: ${res.status} ${res.body}`);
  }

  const token = res.json().token;

  if(!token) {
    fail(`Falha no login: token não retornado. ${res.status} ${res.body}`);
  }

  return  {token: token};
}


export const options = {
  // Rampa: sobe devagar (warmup), segura, desce. O warmup evita medir
  // conexões frias e JIT na largada, que sujariam o p95.
  stages: [
    { duration: '5s',  target: 10 },  // warmup: sobe pra 10 VUs
    { duration: '20s', target: 10 },  // medição estável
    { duration: '5s',  target: 0 },   // desaquece
  ],
  thresholds: {
    // Corretude, não velocidade: se a API começar a dar erro sob carga, o teste FALHA.
    // Não depende de baseline nenhum, por isso já entra agora.
    http_req_failed: ['rate<0.01'],   // menos de 1% de erro
  },
};

const BASE = 'http://localhost:8080';
const SIZE = 20;

export default function (data) {
  const params = { headers: { 'Authorization': `Bearer ${data.token}` } };

  if (!params.headers.Authorization) {
    fail('Token de autorização não encontrado. Verifique se o setup() retornou corretamente o token.');
  }

  check(params, { 'token presente': (p) => p.headers.Authorization !== undefined });

  // Página 1 — o SQL entrega as 20 primeiras linhas, sem descartar nada.
  const r1 = http.get(`${BASE}/api/products/pagination?pageNumber=1&pageSize=${SIZE}`, params);

  if(!r1) {
    fail(`Falha na requisição da página 1: resposta nula ou indefinida. ${r1.status} ${r1.body}`);
  }

  check(r1, { 'pagina 1 -> 200': (r) => r.status === 200 });
  pageShallow.add(r1.timings.duration);

  // Página 4000 — vira OFFSET 79980: o SQL lê e joga fora ~80k linhas
  // antes de entregar as 20. É o custo que o keyset vai eliminar depois.
  const r2 = http.get(`${BASE}/api/products/pagination?pageNumber=4000&pageSize=${SIZE}`, params );
  if(!r2) {
    fail(`Falha na requisição da página profunda: resposta nula ou indefinida. ${r2.status} ${r2.body}`);
  }

  check(r2, { 'pagina profunda -> 200': (r) => r.status === 200 });
  pageDeep.add(r2.timings.duration);

  const r3 = http.get(`${BASE}/api/products/paginationKeyset?cursor=79980&pageSize=${SIZE}`, params );
  
  if(!r3) {
    fail(`Falha na requisição da página keyset: resposta nula ou indefinida. ${r3.status} ${r3.body}`);
  }

  check(r3, { 'pagina keyset -> 200': (r) => r.status === 200 });
  pageKeyset.add(r3.timings.duration);
}
