# Guia de Apps Mobile

Este guia cobre os três frameworks mobile disponíveis em `src/UI/Mobile/` e como configurá-los para se conectar à API backend.

---

## Visão Geral

| Framework | Pasta | Stack | Plataformas |
| --------- | ----- | ----- | ----------- |
| MAUI | `src/UI/Mobile/MauiApp/` | .NET 10, C#, XAML | Android, iOS, Windows, macOS |
| Flutter | `src/UI/Mobile/FlutterApp/` | Flutter SDK 3.3+, Dart, Provider | Android, iOS |
| React Native | `src/UI/Mobile/ReactNativeApp/` | React Native 0.73, Expo, TypeScript | Android, iOS |

> **Nota:** Apps mobile **não são orquestrados pelo Aspire**. Eles precisam de runtime nativo (emulador, dispositivo físico ou simulador) e são iniciados separadamente via `run-mobile.sh` / `run-mobile.ps1`.

---

## Executar os Apps

### Script launcher (todos os frameworks)

```bash
./run-mobile.sh    # Linux/Mac
.\run-mobile.ps1   # Windows
```

O script apresenta um menu para escolher qual app iniciar e em qual plataforma.

### Individualmente

```bash
# MAUI (Android)
cd src/UI/Mobile/MauiApp
dotnet build -t:Run -f net10.0-android

# Flutter
cd src/UI/Mobile/FlutterApp
flutter pub get
flutter run

# React Native (Expo)
cd src/UI/Mobile/ReactNativeApp
npm install
npx expo start
```

### Build para produção

```bash
./build-mobile-all.sh    # Linux/Mac
.\build-mobile-all.ps1   # Windows
```

---

## Pré-requisitos por Framework

### MAUI

- .NET 10 SDK com workload MAUI: `dotnet workload install maui`
- Android SDK (detectado em `~/Android/Sdk`)
- Para iOS: Xcode (macOS apenas)

### Flutter

- Flutter SDK 3.3+: [flutter.dev/install](https://flutter.dev/docs/get-started/install)
- Android Studio ou Xcode para emuladores
- Verificar setup: `flutter doctor`

### React Native / Expo

- Node.js 18+
- Expo CLI: `npm install -g expo-cli`
- Android Studio ou Xcode para emuladores
- Para dispositivo físico: instalar o app Expo Go

---

## Configuração da URL da API

Cada framework tem um arquivo de configuração central para o endpoint da API.

### MAUI

Arquivo: [src/UI/Mobile/MauiApp/MauiProgram.cs](../src/UI/Mobile/MauiApp/MauiProgram.cs)

```csharp
private const string ApiBaseUrl = "https://localhost:7196";
```

Altere para o endereço da API em execução. Para emuladores Android, use o IP da máquina host:

```csharp
// Emulador Android → use o IP da máquina (não localhost)
private const string ApiBaseUrl = "http://10.0.2.2:3062";

// Dispositivo físico → use o IP local da máquina
private const string ApiBaseUrl = "http://192.168.1.x:3062";
```

### Flutter

Arquivo: [src/UI/Mobile/FlutterApp/lib/core/network/api_client.dart](../src/UI/Mobile/FlutterApp/lib/core/network/api_client.dart)

```dart
static String get baseUrl {
  // Emulador Android → 10.0.2.2 mapeia para localhost da máquina host
  return 'http://10.0.2.2:3062';
}
```

### React Native

Arquivo: [src/UI/Mobile/ReactNativeApp/src/api/apiClient.ts](../src/UI/Mobile/ReactNativeApp/src/api/apiClient.ts)

```typescript
const API_URL = Platform.OS === 'android'
  ? 'http://10.0.2.2:3062'   // emulador Android
  : 'http://localhost:3062';   // simulador iOS
```

---

## Portas padrão da API

| Modo | URL HTTP | URL HTTPS |
| ---- | -------- | --------- |
| `dotnet run` direto | `http://localhost:3062` | `https://localhost:3060` |
| Emulador Android | `http://10.0.2.2:3062` | — (evite HTTPS com cert dev) |
| Dispositivo físico | `http://<IP-da-maquina>:3062` | — |

> Para emulador Android, `10.0.2.2` é o alias especial que aponta para `localhost` da máquina host. Para simulador iOS, `localhost` funciona diretamente.

---

## src/Shared — Modelos Compartilhados (MAUI e Blazor)

O projeto `src/Shared/` é referenciado diretamente pelo MAUI (e opcionalmente pelo Blazor), pois ambos são projetos C#:

```xml
<!-- MauiApp.csproj -->
<ProjectReference Include="..\..\..\..\Shared\Shared.csproj" />
```

DTOs disponíveis:

| Tipo | Namespace |
| ---- | --------- |
| `LoginRequest`, `TokenResponse` | `Shared.Auth` |
| `ProductResponse`, `CreateProductRequest` | `Shared.Products` |
| `OrderResponse`, `CreateOrderRequest` | `Shared.Orders` |
| `PagedResponse<T>` | `Shared` |

**Flutter e React Native** replicam os tipos manualmente em seus modelos, mantendo compatibilidade com a API via contrato JSON.

---

## Autenticação

A API usa JWT. O fluxo nos apps mobile:

1. `POST /api/auth/login` → recebe `accessToken` e `refreshToken`
2. Armazene com segurança:
   - **MAUI**: `SecureStorage.Default`
   - **Flutter**: `flutter_secure_storage`
   - **React Native**: `expo-secure-store`
3. Inclua `Authorization: Bearer <token>` em todas as requisições
4. Renove via `POST /api/auth/refresh` quando expirar

---

## Estrutura de Pastas (padrão adotado)

Todos os frameworks seguem organização por feature:

```text
lib/ (Flutter) / src/ (React Native) / Pages+Services/ (MAUI)
├── auth/           # Login, tokens, guard de navegação
├── features/
│   ├── products/   # Lista, detalhe, criação
│   ├── orders/     # Pedidos
│   ├── audit/      # Logs de auditoria
│   └── dashboard/  # Visão geral
├── shared/         # Widgets/componentes reutilizáveis
└── core/
    ├── api/        # HTTP client configurado
    └── services/   # Serviços base
```

---

## Troubleshooting

### MAUI: erro de certificado HTTPS

Em desenvolvimento com emulador, prefira HTTP na porta 3062. Para usar HTTPS, adicione o certificado de dev ao emulador Android ou desabilite a validação em dev:

```csharp
// Apenas em desenvolvimento
handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
```

### Flutter: conexão recusada no emulador

Use `10.0.2.2` no lugar de `localhost`. Confirme que a API está rodando e aceitando conexões em todas as interfaces (`http://+:3062`).

### React Native / Expo: Network Request Failed

Verifique se o IP está correto (`Platform.OS === 'android'`). Confirme que o firewall permite conexões na porta da API.

---

## Referências

- [QUICK-START.md](../QUICK-START.md) — Como rodar a API e os launchers
- [AUTHENTICATION.md](AUTHENTICATION.md) — JWT e OAuth2 na API
- [UI-GUIDE.md](UI-GUIDE.md) — Frontends Web (Angular, Blazor, React, Vue)
