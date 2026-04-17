# Sistema de Emissão de Notas Fiscais 🧾

Sistema full stack desenvolvido como desafio técnico, com foco em arquitetura de microsserviços, regras de negócio consistentes e integração resiliente entre serviços. 🚀

## Visão Geral

O projeto simula um cenário real de emissão de notas fiscais com controle de estoque e faturamento desacoplados.

O backend foi dividido em dois microsserviços independentes:
- `estoque-service`: responsável por produtos, saldo em estoque e validações de disponibilidade
- `faturamento-service`: responsável por notas fiscais, itens, impressão/emissão e integração com o estoque

O frontend em Angular centraliza a operação do sistema em uma interface única, permitindo cadastrar produtos, acompanhar notas fiscais e executar o fluxo completo de emissão com feedback visual amigável.

## Screenshots

### Tela de Produtos

![Tela de Produtos](<docs/Screenshot 2026-04-17 130752.png>)

### Tela de Notas Fiscais

![Tela de Notas Fiscais](<docs/Screenshot 2026-04-17 131000.png>)

## Arquitetura

### Microsserviços

**estoque-service**
- Gerencia o cadastro de produtos
- Atualiza descrição de produtos
- Executa atualização administrativa de estoque
- Valida disponibilidade
- Realiza baixa de estoque durante a emissão da nota

**faturamento-service**
- Cria notas fiscais com numeração sequencial
- Permite inclusão de itens apenas em notas abertas
- Consulta notas por ID e por número sequencial
- Executa a emissão da nota fiscal
- Orquestra validação e baixa de estoque via `estoque-service`

### Frontend

O `frontend-angular` atua como camada de apresentação da solução:
- consome os dois microsserviços
- organiza os fluxos de produtos e notas fiscais
- exibe estados de carregamento, mensagens de sucesso e tratamento amigável de erros
- impede ações inválidas na interface, como editar ou emitir notas já fechadas

### Banco de Dados

O PostgreSQL é utilizado como banco relacional da aplicação. Cada microsserviço possui seu próprio banco lógico:
- `estoque_db`
- `faturamento_db`

Essa separação reforça o isolamento entre domínios e evita acoplamento indevido entre os serviços.

## Tecnologias Utilizadas

### Backend
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- PostgreSQL
- FluentValidation
- Serilog
- Polly

### Frontend
- Angular 19
- Angular Material
- RxJS
- Reactive Forms

### Infraestrutura
- Docker
- Docker Compose
- Nginx

## Funcionalidades

- Cadastro de produtos
- Atualização de descrição de produto
- Atualização administrativa de estoque
- Sugestão assistida de descrição de produto com IA
- Consulta de produtos
- Criação de nota fiscal com numeração sequencial
- Inclusão de itens apenas quando a nota está **Aberta**
- Consulta de notas fiscais
- Emissão de nota fiscal
- Validação e baixa de estoque durante a emissão
- Fechamento automático da nota em caso de sucesso
- Manutenção da nota como **Aberta** em caso de falha
- Idempotência na emissão com `X-Idempotency-Key`
- Simulação de falha com o código `ERRO500`
- Feedback amigável de erros no frontend

## Fluxo Principal do Sistema

1. O usuário cadastra um produto no `estoque-service`.
2. O usuário cria uma nova nota fiscal no `faturamento-service`.
3. Enquanto a nota estiver **Aberta**, itens podem ser adicionados.
4. Ao emitir a nota, o `faturamento-service` consulta o `estoque-service`.
5. O estoque é validado item a item.
6. Se houver saldo suficiente, a baixa é realizada.
7. Após a baixa com sucesso, a nota é fechada.
8. Se ocorrer erro de estoque ou falha de integração, a nota permanece **Aberta**.
9. O frontend exibe o resultado da operação com mensagem de sucesso ou erro.

## Tratamento de Falhas

### Cenário `ERRO500`

Quando um item possui o código `ERRO500`, o sistema simula uma falha no fluxo de estoque. Esse cenário foi incluído para demonstrar comportamento resiliente da aplicação.

Nessa situação:
- a emissão falha
- a nota fiscal não é fechada
- o estoque não é baixado
- o erro é registrado
- o frontend apresenta uma mensagem amigável ao usuário

Esse cenário demonstra a capacidade do sistema de lidar com falhas sem comprometer a consistência do negócio.

### Retry com Polly

O `faturamento-service` utiliza Polly para retry apenas em falhas transitórias na comunicação com o `estoque-service`.

Objetivos:
- aumentar resiliência em falhas momentâneas
- evitar retry em erros funcionais, como produto inexistente ou estoque insuficiente
- manter o comportamento previsível do fluxo de negócio

## Idempotência na Emissão

Como melhoria opcional, o endpoint `POST /api/invoices/{id}/print` aceita o header `X-Idempotency-Key`.

Quando a mesma nota é emitida novamente com a mesma chave:
- o fluxo de emissão não é processado uma segunda vez
- o estoque não sofre nova baixa
- a nota não é fechada novamente
- a resposta anterior é reutilizada com segurança

Essa abordagem protege o cenário clássico de retry do cliente ou reenvio acidental da mesma operação crítica.

### Como demonstrar

1. Crie uma nota fiscal com item válido.
2. Envie `POST /api/invoices/{id}/print` com o header `X-Idempotency-Key: print-key-001`.
3. Repita a mesma chamada com a mesma chave.
4. A segunda resposta será reaproveitada sem novo débito de estoque.

Essa proteção também está coberta por teste de integração no `faturamento-service`.

## Tratamento de Concorrência

Como melhoria opcional, o `estoque-service` implementa proteção para o cenário em que duas requisições tentam consumir a última unidade de um produto ao mesmo tempo.

Estratégia adotada:
- transação no fluxo de débito de estoque
- lock em nível de linha no PostgreSQL (`FOR UPDATE`)
- revalidação do saldo dentro da transação protegida

Resultado:
- apenas uma requisição consegue debitar a última unidade
- a outra recebe erro de conflito de negócio
- o estoque nunca fica negativo

### Como demonstrar

Com o PostgreSQL em execução, rode o teste de concorrência:

```bash
docker-compose up -d postgres
env ENABLE_POSTGRES_CONCURRENCY_TESTS=true dotnet test estoque-service/tests/EstoqueService.IntegrationTests/EstoqueService.IntegrationTests.csproj --filter ProductConcurrencyTests -v minimal
```

## Sugestão de Descrição com IA

Como melhoria opcional, o `estoque-service` expõe um endpoint para sugerir descrições de produto a partir do código e de uma descrição parcial.

Fluxo:
- o usuário informa o código do produto
- opcionalmente escreve uma descrição inicial
- clica em `Sugerir descrição com IA`
- o sistema retorna uma sugestão pronta para uso no formulário

A implementação foi mantida simples e segura para o desafio:
- interface dedicada para geração de descrição
- estratégia determinística atual, fácil de demonstrar
- estrutura pronta para futura substituição por um provider real de LLM

### Como demonstrar

1. Acesse a tela de produtos.
2. Preencha um código como `NOTE-001`.
3. Informe uma descrição parcial, como `Notebook`, ou deixe o campo vazio.
4. Clique em `Sugerir descrição com IA`.
5. Mostre a sugestão retornada e use o botão para aplicar no formulário.

## Estrutura do Projeto

```text
.
├── docs/
├── docker/
│   └── postgres/
├── estoque-service/
│   ├── src/
│   └── tests/
├── faturamento-service/
│   ├── src/
│   └── tests/
├── frontend-angular/
│   ├── public/
│   └── src/
├── scripts/
├── docker-compose.yml
└── README.md
```

## Como Executar com Docker

### Subir a stack completa

```bash
docker-compose up --build
```

### URLs da aplicação

- Frontend: http://localhost:4200
- Estoque Swagger UI: http://localhost:5001/swagger/index.html
- Faturamento Swagger UI: http://localhost:5002/swagger/index.html

Observação:
- para validar o Swagger via terminal, prefira `GET` com `curl -sS` ou `curl -i -sS`
- `HEAD` com `curl -I` pode retornar `404` mesmo com a interface funcionando normalmente

### Observação sobre testes do frontend

Para executar `npm run test` no `frontend-angular`, é necessário ter **Chrome ou Chromium** instalado no ambiente local, já que a suíte utiliza `ChromeHeadless` via Karma.

Se necessário, também é possível definir manualmente:

```bash
export CHROME_BIN=/caminho/do/chrome-ou-chromium
```

### Executar todos os testes

Para rodar toda a suíte em sequência:

```bash
bash scripts/test-all.sh
```

O script executa:
- testes de integração do `estoque-service`
- testes de integração do `faturamento-service`
- testes do `frontend-angular`

Se não houver navegador compatível disponível, o script falha com mensagem explícita e código diferente de zero, no mesmo padrão esperado em CI.

## Principais Endpoints da API

### Estoque Service
- `POST /api/products`
- `POST /api/products/description-suggestions`
- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/products/code/{code}`
- `PUT /api/products/{id}`
- `PATCH /api/products/{id}/stock`

### Faturamento Service
- `POST /api/invoices`
- `GET /api/invoices`
- `GET /api/invoices/{id}`
- `GET /api/invoices/number/{number}`
- `POST /api/invoices/{id}/items`
- `POST /api/invoices/{id}/print`
  Header opcional: `X-Idempotency-Key`

## Destaques do Frontend

- Interface construída com Angular 19 e Angular Material
- Estrutura organizada por páginas, serviços, modelos e camada compartilhada
- Reactive Forms para entradas e validações
- Uso de RxJS com `switchMap`, `catchError`, `finalize` e fluxo reativo de atualização
- Indicadores de carregamento para ações críticas
- Snackbar para feedback de sucesso e erro
- Regras visuais para bloquear ações em notas fiscais fechadas
- Frontend containerizado e servido por Nginx

## Decisões Técnicas

- Separação por microsserviços para isolar responsabilidades de estoque e faturamento
- Arquitetura em camadas no backend para manter controllers enxutos e regras de negócio centralizadas em services
- PostgreSQL como banco relacional consistente para os dois domínios
- Polly para resiliência em integrações externas
- Serilog para observabilidade e troubleshooting
- Docker Compose para facilitar execução local e demonstração do ambiente completo
- Idempotência persistida na emissão de nota para evitar efeitos colaterais em retries
- Serviço de sugestão de descrição desacoplado, pronto para troca por IA real no futuro
- Angular Material para acelerar entrega com boa base visual e consistência de componentes

## Limitações e Melhorias Futuras

- Autenticação e autorização
- Observabilidade centralizada com tracing distribuído
- Pipeline CI/CD
- Testes frontend mais amplos além da suíte unitária mínima
- Monitoramento de saúde entre serviços
- Configuração de ambiente mais avançada para produção
- Estratégia de versionamento e documentação de API com contrato mais formal

## Autor

**Alan Souza**  
