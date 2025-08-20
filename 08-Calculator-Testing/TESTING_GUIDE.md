# Guía Completa de Testing en .NET Core 8

Esta guía completa explica las estrategias de testing implementadas en el proyecto Calculator API, demostrando las mejores prácticas para testing en .NET Core 8.

## Tabla de Contenidos

1. [Introducción a Testing en .NET](#introducción-a-testing-en-net)
2. [Tipos de Tests](#tipos-de-tests)
3. [Herramientas y Librerías](#herramientas-y-librerías)
4. [Unit Tests](#unit-tests)
5. [Integration Tests](#integration-tests)
6. [Container Tests](#container-tests)
7. [Mocking y Test Doubles](#mocking-y-test-doubles)
8. [Test Data Builders](#test-data-builders)
9. [FluentAssertions](#fluentassertions)
10. [Test Fixtures y Shared Context](#test-fixtures-y-shared-context)
11. [Mejores Prácticas](#mejores-prácticas)
12. [Patrones Avanzados](#patrones-avanzados)

## Introducción a Testing en .NET

El testing es fundamental para garantizar la calidad, mantenibilidad y confiabilidad del software. En .NET Core 8, tenemos un ecosistema robusto de herramientas que nos permiten implementar diferentes tipos de tests de manera efectiva.

### ¿Por qué Testing?

- **Confianza en el código**: Los tests nos dan seguridad al hacer cambios
- **Documentación viva**: Los tests documentan cómo debe comportarse el código
- **Detección temprana de bugs**: Encontrar errores antes de producción
- **Refactoring seguro**: Cambiar código sin romper funcionalidad
- **Mejor diseño**: El código testeable tiende a ser mejor diseñado

## Tipos de Tests

### Pirámide de Testing

```
    /\
   /  \
  / E2E \     ← Pocos, lentos, costosos
 /______\
/        \
| Integration |  ← Algunos, moderados
|____________|
|            |
|    Unit     |  ← Muchos, rápidos, baratos
|____________|
```

### 1. Unit Tests
- **Propósito**: Probar unidades individuales de código (métodos, clases)
- **Características**: Rápidos, aislados, determinísticos
- **Scope**: Una sola clase o método
- **Dependencies**: Mockeadas o stubbed

### 2. Integration Tests
- **Propósito**: Probar la integración entre componentes
- **Características**: Más lentos, usan infraestructura real o simulada
- **Scope**: Múltiples componentes trabajando juntos
- **Dependencies**: Algunas reales, otras mockeadas

### 3. End-to-End (E2E) Tests
- **Propósito**: Probar el sistema completo desde la perspectiva del usuario
- **Características**: Lentos, frágiles, costosos de mantener
- **Scope**: Sistema completo
- **Dependencies**: Todas reales

## Herramientas y Librerías

### Framework de Testing
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
```

### Assertions
```xml
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

### Mocking
```xml
<PackageReference Include="Moq" Version="4.20.69" />
```

### Integration Testing
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

### Container Testing
```xml
<PackageReference Include="Testcontainers.Oracle" Version="3.6.0" />
```

## Unit Tests

Los unit tests prueban unidades individuales de código de manera aislada.

### Estructura Básica

```csharp
public class CalculatorServiceTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorServiceTests()
    {
        // Arrange - Setup
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        // Arrange
        double a = 5, b = 3;

        // Act
        var result = _calculatorService.Add(a, b);

        // Assert
        result.Should().Be(8);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Características de Buenos Unit Tests

#### 1. **AAA Pattern (Arrange, Act, Assert)**
```csharp
[Fact]
public async Task CalculateAsync_ValidOperation_ShouldReturnCorrectResult()
{
    // Arrange
    var request = new CalculationRequest
    {
        FirstOperand = 10,
        SecondOperand = 5,
        Operation = "+"
    };

    // Act
    var result = await _calculatorService.CalculateAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Result.Should().Be(15);
    result.Expression.Should().Be("10 + 5 = 15");
}
```

#### 2. **Theory Tests para Múltiples Casos**
```csharp
[Theory]
[InlineData(5, 3, 8)]
[InlineData(-5, 3, -2)]
[InlineData(0, 0, 0)]
[InlineData(10.5, 2.3, 12.8)]
public void Add_ShouldReturnCorrectSum(double a, double b, double expected)
{
    // Act
    var result = _calculatorService.Add(a, b);

    // Assert
    result.Should().Be(expected);
}
```

#### 3. **Testing de Excepciones**
```csharp
[Fact]
public void Divide_ByZero_ShouldThrowDivideByZeroException()
{
    // Act & Assert
    _calculatorService.Invoking(s => s.Divide(10, 0))
        .Should().Throw<DivideByZeroException>()
        .WithMessage("Cannot divide by zero");
}
```

#### 4. **Testing Asíncrono**
```csharp
[Fact]
public async Task CalculateAsync_DivisionByZero_ShouldThrowException()
{
    // Arrange
    var request = new CalculationRequest
    {
        FirstOperand = 10,
        SecondOperand = 0,
        Operation = "/"
    };

    // Act & Assert
    await _calculatorService.Invoking(s => s.CalculateAsync(request))
        .Should().ThrowAsync<DivideByZeroException>()
        .WithMessage("Cannot divide by zero");
}
```

## Integration Tests

Los integration tests prueban cómo múltiples componentes trabajan juntos.

### TestServer para API Testing

```csharp
public class CalculatorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CalculatorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real database with InMemory for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CalculatorDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<CalculatorDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_ValidRequest_ShouldReturnCorrectResult()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CalculationResult>();
        result.Should().NotBeNull();
        result!.Result.Should().Be(15);
    }
}
```

### Características de Integration Tests

#### 1. **Testing de Endpoints Completos**
```csharp
[Theory]
[InlineData(10, 5, "+", 15)]
[InlineData(10, 5, "-", 5)]
[InlineData(10, 5, "*", 50)]
[InlineData(10, 5, "/", 2)]
public async Task Calculate_ValidRequest_ShouldReturnCorrectResult(
    double firstOperand, double secondOperand, string operation, double expectedResult)
{
    // Arrange
    var request = new CalculationRequest
    {
        FirstOperand = firstOperand,
        SecondOperand = secondOperand,
        Operation = operation
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/calculator/calculate", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content.ReadFromJsonAsync<CalculationResult>();
    result!.Result.Should().Be(expectedResult);
}
```

#### 2. **Testing de Flujos End-to-End**
```csharp
[Fact]
public async Task CompleteWorkflow_CalculateAndRetrieveHistory_ShouldWorkCorrectly()
{
    // Arrange
    var calculations = new[]
    {
        new CalculationRequest { FirstOperand = 10, SecondOperand = 5, Operation = "+" },
        new CalculationRequest { FirstOperand = 20, SecondOperand = 4, Operation = "*" }
    };

    // Act - Perform calculations
    foreach (var calc in calculations)
    {
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", calc);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Verify history
    var historyResponse = await _client.GetAsync("/api/calculator/history");
    var history = await historyResponse.Content.ReadFromJsonAsync<CalculationHistory[]>();

    // Assert
    history!.Should().HaveCount(2);
    history.Should().Contain(h => h.Result == 15);
    history.Should().Contain(h => h.Result == 80);
}
```

## Container Tests

Los container tests usan contenedores Docker para probar con infraestructura real.

### Testcontainers con Oracle

```csharp
public class CalculatorOracleIntegrationTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorOracleIntegrationTests()
    {
        // En un escenario real, usarías Testcontainers:
        // var container = new OracleBuilder()
        //     .WithDatabase("testdb")
        //     .WithUsername("testuser")
        //     .WithPassword("testpass")
        //     .Build();
        // 
        // await container.StartAsync();
        // var connectionString = container.GetConnectionString();

        // Para este ejemplo, usamos InMemory
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"OracleSimulationDb_{Guid.NewGuid()}")
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    [Fact]
    public async Task CalculateAsync_ShouldPersistToOracleDatabase()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 25,
            SecondOperand = 5,
            Operation = "*"
        };

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(125);

        // Verify persistence in database
        var persistedEntry = await _context.CalculationHistory
            .FirstOrDefaultAsync(h => h.FirstOperand == 25 && h.SecondOperand == 5);

        persistedEntry.Should().NotBeNull();
        persistedEntry!.Result.Should().Be(125);
    }
}
```

### Configuración Real de Testcontainers

```csharp
// Ejemplo de configuración real con Testcontainers
public class RealOracleContainerTests : IAsyncLifetime
{
    private readonly OracleContainer _oracleContainer;
    private CalculatorDbContext _context;
    private CalculatorService _service;

    public RealOracleContainerTests()
    {
        _oracleContainer = new OracleBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _oracleContainer.StartAsync();
        
        var connectionString = _oracleContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseOracle(connectionString)
            .Options;

        _context = new CalculatorDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        
        _service = new CalculatorService(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _oracleContainer.DisposeAsync();
    }

    [Fact]
    public async Task RealOracle_ShouldPersistData()
    {
        // Test implementation with real Oracle database
    }
}
```

## Mocking y Test Doubles

### Tipos de Test Doubles

1. **Dummy**: Objetos que se pasan pero nunca se usan
2. **Fake**: Implementaciones funcionales pero simplificadas
3. **Stub**: Proveen respuestas predefinidas a llamadas
4. **Spy**: Stubs que también registran información sobre cómo fueron llamados
5. **Mock**: Objetos pre-programados con expectativas

### Usando Moq

```csharp
public class CalculatorServiceWithMockTests
{
    [Fact]
    public async Task CalculateAsync_ShouldCallRepositoryCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<ICalculationRepository>();
        var service = new CalculatorService(mockRepository.Object);
        
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+"
        };

        mockRepository
            .Setup(r => r.SaveCalculationAsync(It.IsAny<CalculationHistory>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.CalculateAsync(request);

        // Assert
        mockRepository.Verify(
            r => r.SaveCalculationAsync(It.Is<CalculationHistory>(
                h => h.FirstOperand == 10 && 
                     h.SecondOperand == 5 && 
                     h.Operation == "+" && 
                     h.Result == 15)),
            Times.Once);
    }
}
```

### Mock Setup Patterns

```csharp
// Setup return values
mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new CalculationHistory { Id = 1, Result = 15 });

// Setup exceptions
mockRepository
    .Setup(r => r.SaveAsync(It.IsAny<CalculationHistory>()))
    .ThrowsAsync(new InvalidOperationException("Database error"));

// Setup with conditions
mockRepository
    .Setup(r => r.GetByIdAsync(It.Is<int>(id => id > 0)))
    .ReturnsAsync((int id) => new CalculationHistory { Id = id });

// Verify calls
mockRepository.Verify(r => r.SaveAsync(It.IsAny<CalculationHistory>()), Times.Once);
mockRepository.Verify(r => r.GetByIdAsync(1), Times.Never);
```

## Test Data Builders

Los Test Data Builders proporcionan una forma fluida de crear objetos de test.

### Implementación del Builder Pattern

```csharp
public class TestDataBuilder
{
    public static CalculationRequestBuilder CalculationRequest() => new();
    public static CalculationHistoryBuilder CalculationHistory() => new();
}

public class CalculationRequestBuilder
{
    private double _firstOperand = 10;
    private double _secondOperand = 5;
    private string _operation = "+";

    public CalculationRequestBuilder WithFirstOperand(double value)
    {
        _firstOperand = value;
        return this;
    }

    public CalculationRequestBuilder WithSecondOperand(double value)
    {
        _secondOperand = value;
        return this;
    }

    public CalculationRequestBuilder WithOperation(string operation)
    {
        _operation = operation;
        return this;
    }

    public CalculationRequestBuilder WithAddition() => WithOperation("+");
    public CalculationRequestBuilder WithSubtraction() => WithOperation("-");
    public CalculationRequestBuilder WithMultiplication() => WithOperation("*");
    public CalculationRequestBuilder WithDivision() => WithOperation("/");

    public CalculationRequest Build() => new()
    {
        FirstOperand = _firstOperand,
        SecondOperand = _secondOperand,
        Operation = _operation
    };
}
```

### Uso de Test Data Builders

```csharp
[Fact]
public async Task CalculateAsync_WithBuilder_ShouldWork()
{
    // Arrange
    var request = TestDataBuilder.CalculationRequest()
        .WithFirstOperand(100)
        .WithSecondOperand(25)
        .WithDivision()
        .Build();

    // Act
    var result = await _calculatorService.CalculateAsync(request);

    // Assert
    result.Result.Should().Be(4);
}

[Fact]
public async Task MultipleCalculations_WithBuilders_ShouldWork()
{
    // Arrange
    var operations = new[]
    {
        TestDataBuilder.CalculationRequest().WithAddition().Build(),
        TestDataBuilder.CalculationRequest().WithSubtraction().Build(),
        TestDataBuilder.CalculationRequest().WithMultiplication().Build()
    };

    // Act & Assert
    foreach (var operation in operations)
    {
        var result = await _calculatorService.CalculateAsync(operation);
        result.Should().NotBeNull();
    }
}
```

### Ventajas de Test Data Builders

1. **Legibilidad**: El código de test es más expresivo
2. **Mantenibilidad**: Cambios en objetos se centralizan
3. **Flexibilidad**: Fácil crear variaciones de objetos
4. **Reutilización**: Builders se pueden usar en múltiples tests

## FluentAssertions

FluentAssertions proporciona una sintaxis más natural y expresiva para assertions.

### Assertions Básicas

```csharp
// En lugar de:
Assert.Equal(15, result.Result);
Assert.True(result.Timestamp <= DateTime.UtcNow);

// Usa:
result.Result.Should().Be(15);
result.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow);
```

### Assertions de Colecciones

```csharp
[Fact]
public async Task GetHistoryAsync_ShouldHaveExpectedCollectionProperties()
{
    // Arrange & Act
    var history = await _calculatorService.GetHistoryAsync();

    // Assert
    history.Should()
        .NotBeNull()
        .And.HaveCount(3)
        .And.BeInDescendingOrder(h => h.CreatedAt)
        .And.OnlyContain(h => h.Id > 0)
        .And.OnlyContain(h => !string.IsNullOrEmpty(h.Operation));

    history.Should().ContainSingle(h => h.Operation == "/" && h.Result == 10);
    history.Should().Contain(h => h.Result > 0);
}
```

### Assertions de Excepciones

```csharp
[Fact]
public async Task CalculateAsync_InvalidOperation_ShouldThrowWithMessage()
{
    // Arrange
    var request = new CalculationRequest
    {
        FirstOperand = 10,
        SecondOperand = 5,
        Operation = "^"
    };

    // Act & Assert
    await _calculatorService.Invoking(s => s.CalculateAsync(request))
        .Should().ThrowAsync<ArgumentException>()
        .WithMessage("Unsupported operation: ^")
        .Where(ex => ex.ParamName == "operation");
}
```

### Assertions de Tiempo

```csharp
[Fact]
public async Task CalculateAsync_ShouldSetCorrectTimestamp()
{
    // Arrange
    var request = TestDataBuilder.CalculationRequest().Build();
    var beforeTime = DateTime.UtcNow;

    // Act
    var result = await _calculatorService.CalculateAsync(request);

    // Assert
    result.Timestamp.Should()
        .BeAfter(beforeTime)
        .And.BeOnOrBefore(DateTime.UtcNow)
        .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}
```

### Assertions Personalizadas

```csharp
public static class TestExtensions
{
    public static void ShouldBeEquivalentToCalculationRequest(
        this CalculationResult result, 
        CalculationRequest request, 
        double expectedResult)
    {
        result.Should().NotBeNull();
        result.FirstOperand.Should().Be(request.FirstOperand);
        result.SecondOperand.Should().Be(request.SecondOperand);
        result.Operation.Should().Be(request.Operation);
        result.Result.Should().Be(expectedResult);
        result.Expression.Should().Be($"{request.FirstOperand} {request.Operation} {request.SecondOperand} = {expectedResult}");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

// Uso:
[Fact]
public async Task CalculateAsync_WithCustomAssertion_ShouldWork()
{
    // Arrange
    var request = TestDataBuilder.CalculationRequest().WithAddition().Build();

    // Act
    var result = await _calculatorService.CalculateAsync(request);

    // Assert
    result.ShouldBeEquivalentToCalculationRequest(request, 15);
}
```

## Test Fixtures y Shared Context

### IClassFixture para Compartir Setup

```csharp
public class CalculatorTestFixture : IDisposable
{
    public CalculatorDbContext Context { get; }
    public CalculatorService Service { get; }

    public CalculatorTestFixture()
    {
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        Context = new CalculatorDbContext(options);
        Service = new CalculatorService(Context);
    }

    public async Task SeedTestDataAsync()
    {
        var testData = new[]
        {
            new CalculationHistory
            {
                FirstOperand = 10,
                SecondOperand = 5,
                Operation = "+",
                Result = 15,
                Expression = "10 + 5 = 15",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        };

        Context.CalculationHistory.AddRange(testData);
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

[Collection("Calculator Collection")]
public class CalculatorServiceFixtureTests : IClassFixture<CalculatorTestFixture>
{
    private readonly CalculatorTestFixture _fixture;

    public CalculatorServiceFixtureTests(CalculatorTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHistoryAsync_WithFixture_ShouldWork()
    {
        // Arrange
        await _fixture.SeedTestDataAsync();

        // Act
        var history = await _fixture.Service.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(1);
    }
}
```

### Collection Fixtures para Compartir Entre Clases

```csharp
[CollectionDefinition("Calculator Collection")]
public class CalculatorCollection : ICollectionFixture<CalculatorTestFixture>
{
    // Esta clase no tiene código, solo sirve para aplicar [CollectionDefinition]
}

[Collection("Calculator Collection")]
public class FirstTestClass : IClassFixture<CalculatorTestFixture>
{
    // Tests que usan el fixture compartido
}

[Collection("Calculator Collection")]
public class SecondTestClass : IClassFixture<CalculatorTestFixture>
{
    // Tests que usan el mismo fixture compartido
}
```

## Mejores Prácticas

### 1. Naming Conventions

```csharp
// Patrón: MethodName_Scenario_ExpectedBehavior
[Fact]
public void Add_WithPositiveNumbers_ShouldReturnSum()

[Fact]
public void Add_WithNegativeNumbers_ShouldReturnCorrectResult()

[Fact]
public async Task CalculateAsync_WithDivisionByZero_ShouldThrowException()
```

### 2. Test Organization

```csharp
public class CalculatorServiceTests
{
    #region Basic Math Operations Tests
    
    [Fact]
    public void Add_ShouldReturnCorrectSum() { }
    
    [Fact]
    public void Subtract_ShouldReturnCorrectDifference() { }
    
    #endregion

    #region CalculateAsync Tests
    
    [Fact]
    public async Task CalculateAsync_ValidOperation_ShouldReturnResult() { }
    
    #endregion

    #region Error Handling Tests
    
    [Fact]
    public void Divide_ByZero_ShouldThrowException() { }
    
    #endregion
}
```

### 3. Test Independence

```csharp
// ❌ Malo - Tests dependientes
[Fact]
public async Task Test1_CreateCalculation() 
{
    var result = await _service.CalculateAsync(request);
    // Guarda ID en variable de clase
}

[Fact]
public async Task Test2_GetCalculation() 
{
    // Usa ID de Test1
}

// ✅ Bueno - Tests independientes
[Fact]
public async Task CreateCalculation_ShouldPersist()
{
    // Arrange, Act, Assert completo
}

[Fact]
public async Task GetCalculation_ShouldRetrieve()
{
    // Arrange (crea sus propios datos), Act, Assert
}
```

### 4. Test Data Management

```csharp
// ✅ Bueno - Cada test limpia después de sí mismo
public class CalculatorServiceTests : IDisposable
{
    private readonly CalculatorDbContext _context;

    public CalculatorServiceTests()
    {
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class
            .Options;

        _context = new CalculatorDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### 5. Async Testing

```csharp
// ✅ Correcto
[Fact]
public async Task CalculateAsync_ShouldReturnResult()
{
    // Arrange
    var request = new CalculationRequest { /* ... */ };

    // Act
    var result = await _calculatorService.CalculateAsync(request);

    // Assert
    result.Should().NotBeNull();
}

// ❌ Incorrecto - No usar .Result o .Wait()
[Fact]
public void CalculateAsync_BadExample()
{
    var result = _calculatorService.CalculateAsync(request).Result; // ❌
}
```

## Patrones Avanzados

### 1. Testing de Concurrencia

```csharp
[Fact]
public async Task CalculateAsync_ConcurrentOperations_ShouldMaintainDataIntegrity()
{
    // Arrange
    var concurrentOperations = Enumerable.Range(1, 50)
        .Select(i => TestDataBuilder.CalculationRequest()
            .WithFirstOperand(i)
            .WithSecondOperand(1)
            .WithAddition()
            .Build())
        .ToList();

    // Act - Execute operations concurrently
    var tasks = concurrentOperations.Select(op => _calculatorService.CalculateAsync(op));
    var results = await Task.WhenAll(tasks);

    // Assert
    results.Should().HaveCount(50);
    results.Should().OnlyContain(r => r != null);
    
    // Verify all entries were persisted
    var historyCount = await _context.CalculationHistory.CountAsync();
    historyCount.Should().Be(50);
}
```

### 2. Performance Testing

```csharp
[Fact]
public async Task GetHistoryAsync_WithLargeDataset_ShouldPerformWell()
{
    // Arrange - Create large dataset
    var largeDataset = Enumerable.Range(1, 1000)
        .Select(i => new CalculationHistory
        {
            FirstOperand = i,
            SecondOperand = 1,
            Operation = "+",
            Result = i + 1,
            Expression = $"{i} + 1 = {i + 1}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        });

    _context.CalculationHistory.AddRange(largeDataset);
    await _context.SaveChangesAsync();

    // Act
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var history = await _calculatorService.GetHistoryAsync(100);
    stopwatch.Stop();

    // Assert
    history.Should().HaveCount(100);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
}
```

### 3. Testing de Edge Cases

```csharp
[Theory]
[InlineData(double.MaxValue, double.MaxValue)]
[InlineData(double.MinValue, double.MinValue)]
[InlineData(double.PositiveInfinity, 1)]
[InlineData(double.NegativeInfinity, 1)]
[InlineData(double.NaN, 5)]
public async Task CalculateAsync_ExtremeValues_ShouldHandleGracefully(double a, double b)
{
    // Arrange
    var request = TestDataBuilder.CalculationRequest()
        .WithFirstOperand(a)
        .WithSecondOperand(b)
        .WithAddition()
        .Build();

    // Act
    var action = async () => await _calculatorService.CalculateAsync(request);

    // Assert
    await action.Should().NotThrowAsync();
}
```

### 4. Custom Test Attributes

```csharp
public class IntegrationTestAttribute : FactAttribute
{
    public IntegrationTestAttribute()
    {
        if (!IsIntegrationTestEnvironment())
        {
            Skip = "Integration tests are disabled in this environment";
        }
    }

    private static bool IsIntegrationTestEnvironment()
    {
        return Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS") == "true";
    }
}

[IntegrationTest]
public async Task DatabaseIntegration_ShouldWork()
{
    // Test que solo se ejecuta en ciertos ambientes
}
```

### 5. Test Categories

```csharp
public static class TestCategories
{
    public const string Unit = "Unit";
    public const string Integration = "Integration";
    public const string Performance = "Performance";
}

[Fact]
[Trait("Category", TestCategories.Unit)]
public void Add_UnitTest_ShouldWork() { }

[Fact]
[Trait("Category", TestCategories.Integration)]
public async Task Api_IntegrationTest_ShouldWork() { }

[Fact]
[Trait("Category", TestCategories.Performance)]
public async Task LargeDataset_PerformanceTest_ShouldBefast() { }
```

## Comandos de Testing

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests de una categoría específica
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Tests por nombre
dotnet test --filter "Name~Calculator"

# Con coverage
dotnet test --collect:"XPlat Code Coverage"

# Con logger detallado
dotnet test --logger "console;verbosity=detailed"
```

### Configuración de Coverage

```xml
<!-- En el archivo .csproj -->
<PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
</PropertyGroup>
```

## Conclusión

Esta guía cubre las estrategias de testing más importantes en .NET Core 8:

1. **Unit Tests**: Rápidos, aislados, para lógica de negocio
2. **Integration Tests**: Para probar componentes trabajando juntos
3. **Container Tests**: Para probar con infraestructura real
4. **Mocking**: Para aislar dependencias
5. **Test Data Builders**: Para crear datos de test de manera fluida
6. **FluentAssertions**: Para assertions más expresivas
7. **Test Fixtures**: Para compartir setup entre tests

### Recomendaciones Finales

- **Comienza con Unit Tests**: Son la base de una buena suite de tests
- **Usa Integration Tests para flujos críticos**: Especialmente APIs y persistencia
- **Reserva Container Tests para casos específicos**: Cuando necesites infraestructura real
- **Mantén tests simples y enfocados**: Un test, un concepto
- **Invierte en herramientas**: FluentAssertions, Test Data Builders, etc.
- **Automatiza todo**: CI/CD debe ejecutar todos los tests

El testing es una inversión que paga dividendos a largo plazo. Un código bien testeado es más confiable, mantenible y permite refactoring seguro.