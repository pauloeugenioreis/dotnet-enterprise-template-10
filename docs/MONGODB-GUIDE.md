# 🗄️ Guia MongoDB

Este guia documenta a implementação MongoDB do template: como habilitar, como o seed automático funciona e como diagnosticar os erros mais comuns.

---

## Índice

1. [Visão Geral](#visão-geral)
2. [Quando Usar](#quando-usar)
3. [Como Habilitar](#como-habilitar)
4. [Credenciais e Container](#credenciais-e-container)
5. [Seed Automático](#seed-automático)
6. [Estrutura de Código](#estrutura-de-código)
7. [Exemplo Prático](#exemplo-prático)
8. [Troubleshooting](#troubleshooting)

---

## Visão Geral

MongoDB é o recurso NoSQL opcional do template para cenários orientados a documentos. Ele é útil quando você quer persistir dados com estrutura flexível, sem depender do modelo relacional do EF Core.

O template já entrega:

- configuração de conexão via `AppSettings.Infrastructure.MongoDB`;
- registro automático via `MongoExtension`;
- repositório genérico `IMongoRepository<T>`;
- seed inicial com `MongoDbSeeder`;
- script de criação do projeto com autenticação do container.

---

## Quando Usar

Use MongoDB quando o seu domínio tiver:

- documentos com campos variáveis;
- histórico de eventos ou auditoria;
- catálogos com atributos opcionais;
- dados que não precisam de joins complexos.

---

## Como Habilitar

Se você estiver criando um projeto novo, o caminho recomendado é usar os scripts do template.

### Windows

```powershell
cd scripts
.\new-project.ps1
```

### Linux/macOS

```bash
cd scripts
chmod +x new-project.sh
./new-project.sh
```

Ao selecionar MongoDB, o script:

- adiciona a connection string em `appsettings.json`;
- cria o container `mongo` com autenticação;
- habilita o init script para criar o usuário de aplicação;
- deixa o `MongoDbSeeder` pronto para uso no ambiente de desenvolvimento.

---

## Credenciais e Container

O template usa credenciais simples para desenvolvimento:

```text
Usuário root: admin
Senha root:   admin
Usuário app:   admin
Senha app:     admin
```

Connection string padrão:

```text
mongodb://admin:admin@localhost:27017/<NomeDoProjeto>
```

O container sobe com healthcheck autenticado e executa o init script em `scripts/mongo-init`.

> Em produção, troque as credenciais e use secrets/variáveis de ambiente.

---

## Seed Automático

O `MongoDbSeeder` existe para garantir dados iniciais sem travar a API.

Comportamento:

- tenta conectar com retry e backoff exponencial;
- valida se a coleção já contém dados antes de inserir novamente;
- insere exemplos de `CustomerReview` quando a coleção está vazia;
- falha de forma graciosa se o Mongo ainda não estiver disponível.

Isso evita o problema clássico de startup em que a API sobe antes do banco estar pronto.

---

## Estrutura de Código

Os principais arquivos da implementação são:

- `src/Domain/Entities/MongoEntityBase.cs` — base para documentos Mongo;
- `src/Domain/Entities/CustomerReview.cs` — entidade de exemplo;
- `src/Domain/Interfaces/IMongoRepository.cs` — contrato genérico;
- `src/Data/Repository/Mongo/MongoRepository.cs` — implementação genérica;
- `src/Data/Repository/Mongo/CustomerReviewRepository.cs` — repositório concreto;
- `src/Data/Seeders/MongoDbSeeder.cs` — seed de desenvolvimento;
- `src/Infrastructure/Extensions/MongoExtension.cs` — registro no DI.

---

## Exemplo Prático

### 1. Criar a entidade

```csharp
public class AuditLogEntry : MongoEntityBase
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
```

### 2. Criar o repositório

```csharp
public interface IAuditLogRepository : IMongoRepository<AuditLogEntry>
{
}

public class AuditLogRepository : MongoRepository<AuditLogEntry>, IAuditLogRepository
{
    public AuditLogRepository(IMongoDatabase database) : base(database, "auditlogs")
    {
    }
}
```

### 3. Criar o serviço

```csharp
public class AuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }
}
```

---

## Troubleshooting

### Authentication failed

Esse erro geralmente significa que o volume do Mongo foi reaproveitado com credenciais antigas.

Soluções:

1. pare os containers;
2. remova o volume do Mongo;
3. suba tudo novamente para o init script recriar o usuário.

### Timeout na primeira chamada

Se a API iniciar antes do Mongo ficar pronto, o `MongoDbSeeder` tenta novamente algumas vezes antes de prosseguir.

### Banco vazio após recriar o projeto

Verifique se o init script `scripts/mongo-init/01-create-app-user.js` está montado no container e se o `docker-compose.yml` foi regenerado pelo script do template.

### Connection string incorreta

Confira se o formato segue este padrão:

```text
mongodb://usuario:senha@host:27017/banco
```

---

## Próximos Passos

Se você quiser evoluir a implementação:

- criar novas coleções com base em `MongoEntityBase`;
- adicionar testes de integração específicos para MongoDB;
- ajustar credenciais para produção via variáveis de ambiente;
- expandir o seed inicial com mais documentos de exemplo.
