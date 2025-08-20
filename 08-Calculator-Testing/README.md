# Calculator API - Testing Complete

Este proyecto demuestra una implementación completa de una API de calculadora con historial de operaciones, diseñada específicamente para enseñar conceptos de testing en .NET Core 8.

## Características Implementadas

### ✅ Funcionalidades Básicas
- **Operaciones matemáticas básicas**: suma, resta, multiplicación y división
- **Historial de operaciones**: almacenamiento persistente de todas las calculaciones
- **API REST completa**: endpoints para operaciones individuales y complejas
- **Manejo de errores**: validación de entrada y manejo de división por cero
- **Documentación automática**: Swagger UI integrado

### ✅ Tecnologías Utilizadas
- **ASP.NET Core 8**: Framework web
- **Entity Framework Core**: ORM para persistencia
- **Entity Framework InMemory**: Base de datos en memoria para desarrollo y testing
- **Swagger/OpenAPI**: Documentación automática de la API
- **Dependency Injection**: Inyección de dependencias nativa de .NET

## Estructura del Proyecto

```
Calculator.Api/
├── Controllers/
│   └── CalculatorController.cs      # Controlador principal de la API
├── Data/
│   └── CalculatorDbContext.cs       # Contexto de Entity Framework
├── Models/
│   ├── CalculationHistory.cs        # Entidad para historial
│   ├── CalculationRequest.cs        # DTO para requests
│   └── CalculationResult.cs         # DTO para responses
├── Services/
│   ├── ICalculatorService.cs        # Interfaz del servicio
│   └── CalculatorService.cs         # Implementación del servicio
└── Program.cs                       # Configuración de la aplicación
```

## Endpoints de la API

### Operaciones Básicas (GET)
- `GET /api/calculator/add?a={num1}&b={num2}` - Suma dos números
- `GET /api/calculator/subtract?a={num1}&b={num2}` - Resta dos números
- `GET /api/calculator/multiply?a={num1}&b={num2}` - Multiplica dos números
- `GET /api/calculator/divide?a={num1}&b={num2}` - Divide dos números

### Operaciones Complejas (POST)
- `POST /api/calculator/calculate` - Realiza una operación y la guarda en el historial

```json
{
  "firstOperand": 25.5,
  "secondOperand": 14.3,
  "operation": "+"
}
```

### Gestión de Historial
- `GET /api/calculator/history?limit={num}` - Obtiene el historial de operaciones
- `DELETE /api/calculator/history` - Limpia todo el historial

## Cómo Ejecutar

1. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

2. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```

3. **Acceder a Swagger UI**:
   - Abrir navegador en `http://localhost:5117`
   - La documentación interactiva estará disponible

## Ejemplos de Uso

### Usando cURL

```bash
# Suma simple
curl "http://localhost:5117/api/calculator/add?a=10&b=5"

# Operación compleja con historial
curl -X POST "http://localhost:5117/api/calculator/calculate" \
  -H "Content-Type: application/json" \
  -d '{"firstOperand": 25.5, "secondOperand": 14.3, "operation": "+"}'

# Ver historial
curl "http://localhost:5117/api/calculator/history"
```

### Usando el archivo HTTP
El proyecto incluye un archivo `Calculator.Api.http` con ejemplos de todas las operaciones que puedes ejecutar directamente desde VS Code con la extensión REST Client.

## Conceptos de Testing Demostrados

### 1. **Separación de Responsabilidades**
- **Controller**: Maneja HTTP requests/responses
- **Service**: Contiene lógica de negocio
- **Repository Pattern**: Abstracción de acceso a datos (via EF Core)

### 2. **Dependency Injection**
- Servicios registrados en el contenedor DI
- Interfaces para facilitar mocking en tests
- Configuración centralizada en `Program.cs`

### 3. **Entity Framework InMemory**
- Base de datos en memoria para desarrollo rápido
- Ideal para tests de integración
- No requiere configuración de base de datos externa

### 4. **Manejo de Errores**
- Validación de modelos con Data Annotations
- Excepciones personalizadas (DivideByZeroException)
- Respuestas HTTP apropiadas (400 Bad Request para errores)

### 5. **Documentación Automática**
- Comentarios XML para documentación rica
- Swagger UI para testing interactivo
- Ejemplos de request/response

## Preparación para Testing

Esta implementación está diseñada para ser fácilmente testeable:

### **Testeable por Diseño**
- ✅ Interfaces para todos los servicios
- ✅ Dependency Injection configurado
- ✅ Lógica de negocio separada de controllers
- ✅ Métodos públicos para operaciones individuales
- ✅ Base de datos en memoria para tests rápidos

### **Casos de Test Sugeridos**

**Unit Tests:**
- Operaciones matemáticas básicas
- Manejo de división por cero
- Validación de entrada
- Mapeo de DTOs

**Integration Tests:**
- Endpoints de la API
- Persistencia en base de datos
- Flujo completo de operaciones

**Container Tests:**
- Tests con base de datos real
- Performance bajo carga
- Migración de esquemas

## Testing Completo Implementado

Este proyecto incluye una implementación completa de testing en .NET Core 8:

### ✅ Tests Implementados

1. **Tests Unitarios** con xUnit, Moq y FluentAssertions
   - `CalculatorServiceTests.cs` - Tests básicos del servicio
   - `CalculatorServiceAdvancedTests.cs` - Tests avanzados con mocking
   - `CalculatorServiceFluentAssertionsTests.cs` - Demostraciones de FluentAssertions
   - `CalculatorServiceFixtureTests.cs` - Tests usando fixtures compartidos

2. **Tests de Integración** con TestServer
   - `CalculatorIntegrationTests.cs` - Tests completos de API
   - `CalculatorOracleIntegrationTests.cs` - Tests de persistencia

3. **Tests de Contenedor** con Testcontainers
   - `CalculatorRealOracleTests.cs` - Tests con base de datos real

4. **Herramientas de Testing**
   - `TestDataBuilder.cs` - Builder pattern para datos de test
   - `CalculatorTestFixture.cs` - Fixtures compartidos

### 📚 Documentación Completa

Para una guía completa sobre testing en .NET Core 8, consulta:

**[TESTING_GUIDE.md](./TESTING_GUIDE.md)** - Guía completa que incluye:

- Diferencias entre Unit, Integration y Container tests
- Ejemplos de mocking con Moq
- Patrones de Test Data Builders
- FluentAssertions avanzadas
- Test Fixtures y contexto compartido
- Mejores prácticas y patrones avanzados
- Comandos y configuración de testing

### 🏃‍♂️ Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo unit tests
dotnet test --filter "Category=Unit"

# Solo integration tests  
dotnet test --filter "Category=Integration"

# Con coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Notas Técnicas

- **Base de Datos**: Usa InMemory por defecto, fácil cambiar a SQL Server/Oracle para producción
- **Logging**: Configurado para desarrollo, listo para Serilog en producción
- **Validación**: Data Annotations básicas, extensible con FluentValidation
- **Documentación**: XML comments completos para Swagger
- **Error Handling**: Manejo básico, extensible con middleware global

Este proyecto sirve como base sólida para aprender y practicar todas las técnicas de testing en .NET Core 8.