# Guia de Frontends Web

Este guia cobre os quatro frameworks disponíveis em `src/UI/Web/` e como cada um se integra com a API backend.

---

## Visão Geral

| Framework | Pasta | Stack | Orquestrado pelo Aspire |
| --------- | ----- | ----- | ---------------------- |
| Angular | `src/UI/Web/Angular/` | Angular 21, TypeScript, Tailwind | Sim |
| Blazor | `src/UI/Web/Blazor/` | .NET 10, C#, três variantes | Sim (WebApp) |
| React | `src/UI/Web/React/` | React 18, Vite, Zustand, React Query | Sim |
| Vue | `src/UI/Web/Vue/` | Vue 3, Vite, Pinia, TypeScript | Sim |

---

## Executar os Frontends

### Via Aspire (recomendado)

O Aspire orquestra a API e todos os frontends Web juntos. Basta executar:

```bash
dotnet run --project src/Aspire/AppHost
# ou
./run.sh    # Linux/Mac
.\run.ps1   # Windows
```

Acesse o **dashboard Aspire** em `http://localhost:18888` para ver os recursos e URLs de cada serviço.

### Individualmente

```bash
# Angular
cd src/UI/Web/Angular
npm install
npm start                 # http://localhost:4200

# React
cd src/UI/Web/React
npm install
npm run dev               # http://localhost:5173

# Vue
cd src/UI/Web/Vue
npm install
npm run dev               # http://localhost:5173

# Blazor WebApp
dotnet run --project src/UI/Web/Blazor/WebApp/App/App.csproj
```

### Build para produção (todos)

```bash
./build-web-all.sh    # Linux/Mac
.\build-web-all.ps1   # Windows
```

---

## Configuração da URL da API

Cada framework tem sua própria forma de configurar o endpoint da API.

### Angular

Arquivo: [src/UI/Web/Angular/src/environments/environment.ts](../src/UI/Web/Angular/src/environments/environment.ts)

A URL é lida em runtime via `window["env"]`, o que permite configurar sem rebuild:

```typescript
export const environment = {
  production: false,
  apiUrl: (window as any)["env"]?.["apiUrl"] || 'https://localhost:3060'
};
```

Para desenvolvimento local, crie `src/assets/env.js` com:

```javascript
(window as any)["env"] = { apiUrl: "https://localhost:3060" };
```

### React

Arquivo: [src/UI/Web/React/.env](../src/UI/Web/React/.env)

```env
VITE_API_BASE_URL=https://localhost:3060
```

### Vue

Arquivo: [src/UI/Web/Vue/.env](../src/UI/Web/Vue/.env)

```env
VITE_API_BASE_URL=https://localhost:3060
```

### Blazor

O Blazor é um projeto C# e se comunica com a API via `HttpClient` registrado no `MauiProgram.cs` ou `Program.cs`. A URL é configurada em `appsettings.json` do próprio projeto Blazor ou via variável de ambiente no Aspire.

---

## Portas padrão da API

| Modo | URL |
| ---- | --- |
| `dotnet run` direto | `https://localhost:3060` / `http://localhost:3062` |
| Via Aspire | URL dinâmica — consulte o dashboard em `http://localhost:18888` |
| Swagger UI | `https://localhost:3060/swagger` |

---

## src/Shared — Modelos Compartilhados

A pasta `src/Shared/` contém DTOs usados tanto pelo backend quanto pelos frontends C# (Blazor e MAUI):

| Arquivo | Conteúdo |
| ------- | -------- |
| `AuthDtos.cs` | LoginRequest, TokenResponse, UserInfo |
| `ProductDtos.cs` | CreateProductRequest, ProductResponse |
| `OrderDtos.cs` | CreateOrderRequest, OrderResponse |
| `CustomerReviewDtos.cs` | ReviewRequest, ReviewResponse |
| `AuditDtos.cs` | AuditEntry, AuditFilter |
| `DocumentDtos.cs` | DocumentUploadRequest, DocumentResponse |
| `PagedResponse.cs` | Wrapper genérico de paginação |

**Frontends TypeScript (Angular, React, Vue)** devem replicar esses tipos manualmente ou usar uma ferramenta de geração como NSwag/OpenAPI CLI a partir do Swagger da API (`/swagger/v1/swagger.json`).

---

## CORS

A API já está configurada para aceitar requisições de frontends locais. Se você adicionar uma porta diferente, edite a política CORS em `src/Server/Api/Extensions/` ou `appsettings.json`:

```json
"AllowedOrigins": ["http://localhost:4200", "http://localhost:5173", "https://localhost:3060"]
```

---

## Autenticação nos Frontends

A API usa JWT. O fluxo padrão:

1. `POST /api/auth/login` → recebe `accessToken` e `refreshToken`
2. Armazene o token (localStorage ou memória)
3. Inclua `Authorization: Bearer <token>` em todas as requisições autenticadas
4. Use `POST /api/auth/refresh` quando o token expirar

Todos os quatro frontends já implementam esse fluxo em seus serviços de autenticação.

---

## Estrutura de Pastas (padrão adotado)

Todos os frameworks seguem a mesma organização por feature:

```text
src/
├── api/           # Chamadas HTTP para a API
├── auth/          # Login, logout, guard de rota
├── components/    # Componentes reutilizáveis
├── features/      # Módulos por domínio (products, orders, etc.)
├── pages/         # Páginas/rotas principais
└── store/         # Estado global (Zustand, Pinia, NgRx)
```

---

## Referências

- [QUICK-START.md](../QUICK-START.md) — Como rodar o projeto completo
- [AUTHENTICATION.md](AUTHENTICATION.md) — JWT e OAuth2 na API
- [SECURITY.md](SECURITY.md) — CORS e headers de segurança
- [MOBILE-GUIDE.md](MOBILE-GUIDE.md) — Apps mobile (Flutter, MAUI, React Native)
