# 🔄 ORM Implementations - Complete Examples

Este diretório contém **implementações completas e funcionais** de repositórios usando diferentes ORMs para as entidades `Product` e `Order`.

## 📁 Estrutura

Repository/
├── Repository.cs                              # Entity Framework Core (Padrão)
├── OrderRepository.cs                         # Entity Framework Core (Padrão)
├── Dapper/                                    # Dapper implementations
│   ├── ProductDapperRepository.cs            # ✅ Completo
│   └── OrderDapperRepository.cs              # ✅ Completo
├── NHibernate/                                # NHibernate implementations
│   ├── ProductNHibernateRepository.cs        # ✅ Completo
│   └── OrderNHibernateRepository.cs          # ✅ Completo
├── Linq2Db/                                   # Linq2Db implementations
│   ├── ProductLinq2DbRepository.cs           # ✅ Completo
│   └── OrderLinq2DbRepository.cs             # ✅ Completo
└── Ado/                                       # ADO.NET implementations
    ├── ProductAdoRepository.cs               # ✅ Completo
    └── OrderAdoRepository.cs                 # ✅ Completo
```bash

## 🎯 Como Testar Cada ORM

### Comparação Rápida

| ORM | Performance | Controle | Verbosidade | Curva Aprendizado | Uso Ideal |
|-----|------------|----------|-------------|-------------------|-----------|
| **Entity Framework** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | Fácil | CRUD geral, RAD |
| **Dapper** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | Fácil | Queries complexas |
| **ADO.NET** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐ | Médio | Performance crítica |
| **NHibernate** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | Difícil | Enterprise apps |
| **Linq2Db** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Médio | LINQ + Performance |

---

### 1️⃣ **Entity Framework Core** (Padrão - Já Funciona)

**Status**: ✅ Habilitado por padrão

Nada a fazer! Está funcionando out-of-the-box.

---

### 2️⃣ **Dapper** (High Performance)

**Status**: ✅ Implementação completa pronta

#### Como habilitar:

**Passo 1**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha 41:
// services.AddEntityFramework(connectionString, dbSettings);
**Passo 3**: Descomente a linha 47:
services.AddDapper(connectionString);
**Passo 4**: Execute o projeto!

dotnet run --project src/Api
#### O que foi implementado:
- ✅ `ProductDapperRepository` - CRUD completo com SQL raw
- ✅ `OrderDapperRepository` - Com transações e relacionamentos
- ✅ `IDbConnectionFactory` - Factory pattern para gerenciamento de conexões
- ✅ `SqlConnectionFactory` - Implementação registrada no DI
- ✅ Injeção de dependência adequada (não cria conexões internamente)
- ✅ Paginação com OFFSET/FETCH
- ✅ Multi-table queries com Dapper multi-mapping
- ✅ Transações para operações complexas

---

### 3️⃣ **NHibernate** (Enterprise Features)

**Status**: ✅ Implementação completa pronta (requer habilitação)

#### Pré-requisito - Instale os pacotes e habilite compilação:

**Passo 1**: Edite `src/Data/Data.csproj` e descomente (linhas ~31-32):
<PackageReference Include="NHibernate" Version="5.5.2" />
<PackageReference Include="FluentNHibernate" Version="3.4.0" />
**Passo 2**: No mesmo arquivo, **remova ou comente** as linhas que excluem NHibernate da compilação (~linha 46):
<!-- Comente ou remova estas linhas -->
<!--
<ItemGroup>
  <Compile Remove="Mappings\NHibernate\**" />
  <Compile Remove="Repository\NHibernate\**" />
</ItemGroup>
-->
**Passo 3**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs` e descomente o código do método `AddNHibernate` (linhas ~133-149)

**Passo 4**: Restaure os pacotes:
dotnet restore
#### Como habilitar no projeto:

**Passo 5**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 6**: Comente a linha 41:
// services.AddEntityFramework(connectionString, dbSettings);
**Passo 7**: Descomente a linha 52:
services.AddNHibernate(connectionString, dbSettings);
**Passo 8**: Execute o projeto!

dotnet build
dotnet run --project src/Api
#### O que foi implementado:
- ✅ `ProductNHibernateRepository` - Com LINQ support
- ✅ `OrderNHibernateRepository` - Com lazy loading
- ✅ `ProductMap` e `OrderMap` - FluentNHibernate mappings
- ✅ SessionFactory configurado
- ✅ Cascade operations para relacionamentos

---

### 4️⃣ **Linq2Db** (LINQ + Performance)

**Status**: ✅ Implementação completa pronta (requer habilitação)

#### Pré-requisito - Instale os pacotes e habilite compilação:

**Passo 1**: Edite `src/Data/Data.csproj` e descomente (linhas ~35-36):
<PackageReference Include="linq2db" Version="5.4.1" />
<PackageReference Include="linq2db.EntityFrameworkCore" Version="8.1.0" />
**Passo 2**: Edite `src/Infrastructure/Infrastructure.csproj` e descomente (linha ~44):
<PackageReference Include="linq2db.AspNet" Version="5.4.1" />
**Passo 3**: No arquivo `src/Data/Data.csproj`, **remova ou comente** as linhas que excluem Linq2Db da compilação (~linha 51):
<!-- Comente ou remova estas linhas -->
<!--
<ItemGroup>
  <Compile Remove="Context\ApplicationDataConnection.cs" />
  <Compile Remove="Repository\Linq2Db\**" />
</ItemGroup>
-->
**Passo 4**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`:
- Descomente o using na linha 8: `using LinqToDB.AspNet;`
- Descomente o código do método `AddLinq2Db` (linhas ~169-182)

**Passo 5**: Restaure os pacotes:
dotnet restore
#### Como habilitar no projeto:

**Passo 6**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 7**: Comente a linha 41:
// services.AddEntityFramework(connectionString, dbSettings);
**Passo 8**: Descomente a linha 57:
services.AddLinq2Db(connectionString, dbSettings);
**Passo 9**: Execute o projeto!

dotnet build
dotnet run --project src/Api
#### O que foi implementado:
- ✅ `ProductLinq2DbRepository` - Com LINQ completo
- ✅ `OrderLinq2DbRepository` - Com transações
- ✅ `ApplicationDataConnection` - DataConnection configurado
- ✅ Bulk operations support
- ✅ Transações explícitas para relacionamentos

---

## 🧪 Testando a API

Após habilitar qualquer ORM, teste os endpoints:

### Swagger UI
```
```bash
http://localhost:5000/swagger
### Testar Products
# GET - Listar produtos
curl http://localhost:5000/api/v1/product

# GET - Buscar produto por ID
curl http://localhost:5000/api/v1/product/1

# POST - Criar produto
curl -X POST http://localhost:5000/api/v1/product \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","price":99.99,"stock":10,"category":"Test"}'
### Testar Orders
# GET - Listar pedidos
curl http://localhost:5000/api/v1/order

# GET - Buscar pedido por ID (com items)
curl http://localhost:5000/api/v1/order/1
---

## 📊 Comparação de Performance

| ORM | Read Speed | Write Speed | LINQ Support | Complexity |
|-----|-----------|-------------|--------------|------------|
| **Dapper** | ⚡⚡⚡⚡⚡ | ⚡⚡⚡⚡⚡ | ❌ | Medium |
| **Linq2Db** | ⚡⚡⚡⚡⚡ | ⚡⚡⚡⚡ | ✅ | Medium |
| **EF Core** | ⚡⚡⚡ | ⚡⚡⚡ | ✅ | Low |
| **NHibernate** | ⚡⚡⚡ | ⚡⚡⚡ | ✅ | High |

---

## 💡 Dicas

### Para alternar entre ORMs rapidamente:

1. Apenas altere uma linha no `DatabaseExtension.cs`
2. Não precisa alterar controllers ou services
3. A interface `IRepository<T>` abstrai tudo
4. Perfeito para benchmarks e testes A/B

### Para usar múltiplos ORMs simultaneamente:

```
```csharp
// No seu service, injete repositórios específicos
public class ProductService
{
    private readonly IRepository<Product> _efRepo;
    private readonly ProductDapperRepository _dapperRepo;

    public ProductService(
        IRepository<Product> efRepo,
        ProductDapperRepository dapperRepo)
    {
        _efRepo = efRepo;      // EF Core para CRUD normal
        _dapperRepo = dapperRepo; // Dapper para queries complexas
    }
}
---

## 🆘 Troubleshooting

### Erro: "Type not found" ao usar NHibernate
**Solução**: Certifique-se de ter instalado os pacotes NuGet

### Erro: "Table doesn't exist" ao usar Dapper/Linq2Db
**Solução**: Execute as migrations do EF Core primeiro para criar as tabelas:
dotnet ef database update --project src/Data --startup-project src/Api
### Erro de compilação com Linq2Db
**Solução**: Instale o pacote `linq2db.AspNet` no projeto Infrastructure

---

## 📚 Recursos Adicionais

- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [NHibernate Documentation](https://nhibernate.info/)
- [Linq2Db Documentation](https://linq2db.github.io/)
- [ADO.NET Documentation](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/)
- [ORM Guide completo](../../docs/ORM-GUIDE.md)

---

## 4️⃣ **ADO.NET** (Maximum Control & Performance)

**Status**: ✅ Implementação completa pronta

#### Como habilitar:

**Passo 1**: Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha 67:
// services.AddEntityFramework(connectionString, dbSettings);
**Passo 3**: Descomente a linha 81:
services.AddAdo(connectionString);
**Passo 4**: Execute o projeto!

dotnet run --project src/Api
```

#### O que foi implementado:
- ✅ `ProductAdoRepository` - CRUD completo com SqlCommand e SqlDataReader
- ✅ `OrderAdoRepository` - Com transações explícitas e multi-table operations
- ✅ Usa `IDbConnectionFactory` para gerenciamento de conexões (DI)
- ✅ Controle total sobre comandos SQL e parâmetros
- ✅ Gerenciamento manual de transações para operações complexas
- ✅ Mapping manual de DataReader para objetos
- ✅ Performance máxima sem overhead de ORM
- ✅ Paginação com OFFSET/FETCH NEXT

#### Características:
- **Performance**: A melhor possível, sem qualquer overhead
- **Controle**: Total sobre SQL, parâmetros, transações
- **Verbosidade**: Requer mais código do que ORMs
- **Uso ideal**: Queries críticas de performance, ETL, bulk operations

---

**🎉 Todos os ORMs estão completamente implementados e prontos para uso!**
