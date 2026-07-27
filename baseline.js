import http from 'k6/http';
import { check } from 'k6';
import { Trend } from 'k6/metrics';

// Dois baldes separados: cada tipo de página tem sua própria medição.
const pageShallow = new Trend('dur_pagina_1', true);      // caso fácil
const pageDeep    = new Trend('dur_pagina_profunda', true); // caso que dói (OFFSET grande)

export function setup() {
  
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

export default function () {
  // Página 1 — o SQL entrega as 20 primeiras linhas, sem descartar nada.
  const r1 = http.get(`${BASE}/api/produtos/pagination?pageNumber=1&pageSize=${SIZE}`);
  check(r1, { 'pagina 1 -> 200': (r) => r.status === 200 });
  pageShallow.add(r1.timings.duration);

  // Página 4000 — vira OFFSET 79980: o SQL lê e joga fora ~80k linhas
  // antes de entregar as 20. É o custo que o keyset vai eliminar depois.
  const r2 = http.get(`${BASE}/api/produtos/pagination?pageNumber=4000&pageSize=${SIZE}`);
  check(r2, { 'pagina profunda -> 200': (r) => r.status === 200 });
  pageDeep.add(r2.timings.duration);
}
