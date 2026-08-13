# Performance — ProdutosAPI

Este documento registra a metodologia e os resultados dos testes de performance do catálogo de produtos. O objetivo principal foi observar o custo da paginação por **offset** em posições profundas e comparar o comportamento com uma implementação por **keyset pagination**.

## Objetivo

O experimento responde a duas perguntas:

1. O custo da paginação por offset aumenta quando a consulta precisa alcançar posições profundas?
2. Uma paginação por keyset, usando `Id` como cursor, mantém latência menor na mesma região da tabela?

Os benchmarks foram executados localmente para reduzir interferências de infraestrutura serverless, cold start e latência de rede externa.

## Ambiente

Condições utilizadas nos testes:

- API executada localmente;
- SQL Server local;
- 100 mil produtos;
- cache desabilitado;
- k6;
- até 10 VUs;
- 30 segundos de execução em três estágios;
- `pageSize = 20`;
- autenticação executada uma vez no `setup()`;
- `Trend` separada para cada cenário;
- threshold de erro HTTP abaixo de 1%.

Os resultados brutos estão preservados em:

- [`baseline-offset.txt`](baseline-offset.txt)
- [`baseline-keyset.txt`](baseline-keyset.txt)

## Cenários

### Offset — página inicial

Consulta a primeira página, sem descarte relevante de registros anteriores.

```http
GET /api/products/pagination?pageNumber=1&pageSize=20
```

### Offset — página profunda

A página 4000 com 20 registros por página representa um deslocamento de:

```text
(4000 - 1) × 20 = 79.980
```

Conceitualmente, a consulta precisa alcançar a posição depois de aproximadamente 79.980 registros antes de devolver os próximos 20.

```http
GET /api/products/pagination?pageNumber=4000&pageSize=20
```

### Keyset — mesma região da tabela

Para comparar com a região profunda do offset, o cursor foi posicionado em torno do mesmo ponto:

```http
GET /api/products/paginationKeyset?cursor=79980&pageSize=20
```

A consulta no Repository segue a ideia:

```csharp
.Where(p => p.Id > cursor.Value)
.OrderBy(p => p.Id)
.Take(pageSize + 1)
```

O registro adicional permite descobrir se existe uma próxima página sem executar um `COUNT(*)`.

## Baseline original — offset

O primeiro benchmark foi executado antes da implementação do keyset.

| Cenário | Média | Mediana | p90 | p95 | Máximo |
| --- | ---: | ---: | ---: | ---: | ---: |
| Offset — página 1 | 62,76 ms | 55,90 ms | 111,46 ms | **129,83 ms** | 347,47 ms |
| Offset — página profunda | 136,48 ms | 120,67 ms | 249,08 ms | **289,76 ms** | 552,99 ms |

Erros HTTP: **0%**.

Esse baseline mostrou que, nas condições da execução, a página profunda apresentou p95 mais de duas vezes maior que a página inicial.

## Comparação após keyset

Depois da implementação do keyset, os três cenários foram executados na mesma rodada.

| Cenário | Média | Mediana | p90 | p95 | Máximo |
| --- | ---: | ---: | ---: | ---: | ---: |
| Offset — página 1 | 41,88 ms | 45,96 ms | 62,75 ms | **68,41 ms** | 171,98 ms |
| Offset — página profunda | 87,44 ms | 98,11 ms | 126,38 ms | **136,65 ms** | 248,41 ms |
| Keyset — página profunda | 6,38 ms | 6,31 ms | 10,49 ms | **12,12 ms** | 73,17 ms |

Erros HTTP: **0%**.

Na mesma execução:

```text
Offset profundo p95: 136,65 ms
Keyset profundo p95:  12,12 ms
```

O p95 do keyset profundo foi aproximadamente **11,3x menor** que o offset profundo nessa rodada.

## Por que o comportamento muda

### Offset

A paginação atual por offset usa:

```csharp
.Skip((pageNumber - 1) * pageSize)
.Take(pageSize)
```

Em posições profundas, a consulta precisa avançar pela ordenação e descartar os registros anteriores antes de devolver a página solicitada.

Além disso, o contrato do endpoint offset retorna totais:

```text
PageNumber
TotalItems
TotalPages
```

Por isso o Repository também executa `CountAsync`.

### Keyset

O keyset não recebe número de página. O cliente informa o último ponto conhecido da ordenação:

```csharp
.Where(p => p.Id > cursor.Value)
.OrderBy(p => p.Id)
.Take(pageSize + 1)
```

O `Id` funciona como cursor. O endpoint retorna:

```text
Items
NextCursor
HasNextPage
```

Como não há necessidade de `TotalItems` ou `TotalPages`, o keyset também evita o `CountAsync`.

## Trade-offs

| Offset | Keyset |
| --- | --- |
| Permite saltar diretamente para uma página específica | Navega a partir de um cursor conhecido |
| Natural para interfaces com páginas numeradas | Natural para “carregar mais” e rolagem infinita |
| Pode degradar em páginas profundas | Mantém custo mais estável em posições profundas |
| Retorna facilmente `TotalItems` e `TotalPages` | Não fornece totais naturalmente |
| Pode sofrer deslocamentos com inserções e exclusões | Tende a ser mais estável durante mudanças nos dados |

Por isso, o projeto mantém **as duas estratégias**. Keyset não substitui o offset em todos os casos; ele é uma alternativa para cenários em que navegação sequencial e performance em posições profundas são mais importantes que salto arbitrário por número de página.

## Validação funcional

A paginação keyset também possui testes de integração do Repository usando:

- `ProductRepository` real;
- `AppDbContext` real;
- EF Core;
- SQLite in-memory.

Os testes cobrem pelo menos dois comportamentos centrais:

1. quando existem mais produtos, `HasNextPage` é `true` e `NextCursor` aponta para o último item entregue;
2. na última página, `HasNextPage` é `false` e `NextCursor` é `null`.

## Limitações do benchmark

Os resultados devem ser interpretados dentro do escopo do experimento.

### O endpoint offset faz mais trabalho

O offset executa `CountAsync` para calcular `TotalItems` e `TotalPages`. O keyset não precisa dessa consulta.

Assim, a comparação registrada aqui mede os **contratos reais dos endpoints**, e não exclusivamente o custo isolado de:

```text
OFFSET / Skip
versus
WHERE Id > cursor
```

Um microbenchmark específico exigiria isolar o `CountAsync`.

### Warmup e janela de medição

O teste utiliza uma rampa inicial de VUs que ajuda a aquecer aplicação, JIT, conexões e banco. Entretanto, as `Trend` atuais também recebem amostras durante a subida e a descida de carga.

Uma versão mais rigorosa poderia separar cenários de warmup e medição, registrando métricas customizadas apenas durante a janela estável.

Essa sofisticação foi deliberadamente deixada fora do escopo atual: o objetivo deste projeto é manter um benchmark simples, reproduzível e suficiente para demonstrar o comportamento dos endpoints reais.

### Execuções diferentes não devem ser comparadas diretamente

O baseline original e a execução posterior ocorreram em momentos diferentes. Estado de JIT, cache de páginas do banco, sistema operacional e outros fatores podem variar.

Por isso, o ganho de **11,3x** foi calculado usando apenas o offset profundo e o keyset profundo da **mesma execução** registrada em `baseline-keyset.txt`.

## Conclusão

O experimento confirmou o comportamento esperado para o cenário testado:

- o offset apresentou maior custo em página profunda;
- o keyset alcançou a mesma região da tabela com latência significativamente menor;
- nenhum dos cenários apresentou erros HTTP;
- offset e keyset permanecem no projeto porque atendem a necessidades diferentes.

O resultado não transforma keyset em uma substituição universal do offset. Ele demonstra, com números do próprio projeto, por que paginação orientada a cursor é uma alternativa adequada quando a navegação é sequencial e páginas profundas são frequentes.
