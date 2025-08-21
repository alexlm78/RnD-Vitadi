# 📚 Digital Library API - Advanced Documentation Example

This project demonstrates **professional-grade API documentation** using Swagger/OpenAPI with ASP.NET Core 8. It showcases comprehensive documentation features, custom UI theming, interactive examples, and simulated authentication for a digital library management system.

## 🎯 Learning Objectives

This example teaches developers how to create **world-class API documentation** that goes beyond basic Swagger generation:

1. **Advanced Swagger/OpenAPI Configuration** - Custom schemas, examples, and metadata
2. **Professional UI Customization** - Branded themes, enhanced UX, and interactive features
3. **Multiple Authentication Documentation** - JWT, API Keys, Basic Auth, and OAuth2 simulation
4. **API Versioning Strategies** - Multiple versions with separate documentation
5. **Developer Experience Enhancement** - Interactive helpers, keyboard shortcuts, and performance metrics
6. **Documentation Best Practices** - Comprehensive examples, clear descriptions, and user-friendly design

## ✨ Key Features

### 🎨 Custom Swagger UI Theme
- **Library-inspired Design**: Custom CSS with book/library themed elements
- **Dark/Light Mode Support**: Automatic theme detection and manual toggle
- **Enhanced Navigation**: Improved filtering, search, and operation organization
- **Interactive Elements**: Custom buttons, modals, and enhanced functionality
- **Responsive Design**: Mobile-friendly documentation interface

### 🔐 Authentication Simulation
- **Multiple Auth Schemes**: JWT Bearer, API Key, Basic Auth, and OAuth2
- **Demo Credentials**: Pre-filled test credentials for different user roles
- **Interactive Auth Helper**: Step-by-step authentication guidance
- **Role-based Examples**: Different access levels (User, Librarian, Admin)

### 📖 Rich Documentation Features
- **Comprehensive Examples**: Request/response samples for all endpoints
- **Interactive Code Snippets**: cURL, PowerShell, and other language examples
- **Detailed Descriptions**: Clear explanations for every parameter and response
- **Error Documentation**: Complete error codes and troubleshooting guides
- **Performance Metrics**: Real-time API performance monitoring

### 🚀 Enhanced Developer Experience
- **Quick Actions**: Download specs, health checks, and API testing
- **Keyboard Shortcuts**: Power-user navigation and functionality
- **API Explorer**: Visual navigation through different endpoint categories
- **Version Comparison**: Side-by-side documentation for different API versions
- **Custom Notifications**: User-friendly feedback and status messages

## 🏗️ Architecture Overview

```
DigitalLibrary.Api/
├── Controllers/           # API endpoints with rich documentation
│   ├── BooksController.cs      # Book management operations
│   ├── AuthorsController.cs    # Author management operations
│   ├── LoansController.cs      # Loan tracking operations
│   └── LibraryController.cs    # System information endpoints
├── Models/               # Data models with detailed annotations
├── DTOs/                 # Data transfer objects with examples
├── Swagger/              # Custom Swagger configuration
│   ├── ExampleSchemaFilter.cs  # Automatic example generation
│   ├── SwaggerDefaultValues.cs # Default value handling
│   └── EnumSchemaFilter.cs     # Enhanced enum documentation
├── wwwroot/swagger-ui/   # Custom UI assets
│   ├── custom.css             # Professional theme styling
│   └── custom.js              # Enhanced functionality
└── Program.cs            # Advanced Swagger configuration
```

## 🚀 Getting Started

### Prerequisites
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension

### Quick Start
1. **Clone and Navigate**:
   ```bash
   git clone <repository-url>
   cd 09-DigitalLibrary-Documentation/DigitalLibrary.Api
   ```

2. **Run the Application**:
   ```bash
   dotnet run
   ```

3. **Open Documentation**:
   Navigate to `https://localhost:5001` in your browser

4. **Explore Features**:
   - Try the authentication helper (🔐 Auth Helper button)
   - Download the OpenAPI spec (📥 Download Spec button)
   - Test the API endpoints (🧪 Quick Test button)
   - Use keyboard shortcuts (Ctrl/Cmd + H for help)

## 📚 API Endpoints Overview

### 📖 Books Management
- `GET /api/v1/books` - List all books with pagination and filtering
- `GET /api/v1/books/{id}` - Get detailed book information
- `POST /api/v1/books` - Add new books to the catalog
- `PUT /api/v1/books/{id}` - Update existing book information
- `DELETE /api/v1/books/{id}` - Remove books from catalog

### 👤 Authors Management
- `GET /api/v1/authors` - Browse author directory
- `GET /api/v1/authors/{id}` - Get author details and bibliography
- `POST /api/v1/authors` - Register new authors
- `PUT /api/v1/authors/{id}` - Update author information
- `DELETE /api/v1/authors/{id}` - Remove authors

### 📋 Loan Tracking
- `GET /api/v1/loans` - View all loans with status filtering
- `GET /api/v1/loans/{id}` - Get specific loan details
- `POST /api/v1/loans` - Create new book loans
- `PUT /api/v1/loans/{id}` - Update loan status and information
- `DELETE /api/v1/loans/{id}` - Cancel or remove loans

### ℹ️ Library Information
- `GET /api/v1/library/info` - Get library statistics and system info
- `GET /api/v1/library/health` - System health check endpoint

## 🔐 Authentication Examples

The API demonstrates **four different authentication methods** with interactive examples:

### 1. JWT Bearer Token
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
**Demo Tokens Available**:
- **User**: `demo-user-token` (Read access)
- **Librarian**: `demo-librarian-token` (Read/Write access)
- **Admin**: `demo-admin-token` (Full access)

### 2. API Key Authentication
```http
X-API-Key: your-api-key-here
```
**Demo Keys Available**:
- **Development**: `dev-api-key-12345`
- **Testing**: `test-api-key-67890`
- **Production**: `prod-api-key-abcdef`

### 3. Basic Authentication
```http
Authorization: Basic dXNlcm5hbWU6cGFzc3dvcmQ=
```

### 4. OAuth2 Simulation
Complete OAuth2 authorization code flow simulation for demonstration purposes.

## 🎨 Customization Features

### CSS Theming
The custom CSS includes:
- **Library-inspired color scheme** with book and reading themes
- **Responsive design** that works on all device sizes
- **Accessibility features** including high contrast and keyboard navigation
- **Print-friendly styles** for documentation printing
- **Dark mode support** with automatic detection

### JavaScript Enhancements
The custom JavaScript adds:
- **Interactive authentication helpers** with one-click credential filling
- **API exploration tools** with guided navigation
- **Performance monitoring** with real-time metrics
- **Keyboard shortcuts** for power users
- **Custom modals and notifications** for better UX

## 📊 Advanced Features

### API Versioning
- **Version 1.0**: Current stable API with full functionality
- **Version 2.0**: Future API version (placeholder for demonstration)
- **Side-by-side documentation** for version comparison
- **Backward compatibility** strategies and migration guides

### Interactive Examples
- **Auto-generated code samples** in multiple languages
- **Try-it-out functionality** with real API calls
- **Response validation** and error handling examples
- **Performance metrics** for each API call

### Developer Tools
- **OpenAPI spec download** in JSON and YAML formats
- **Health check integration** with system status monitoring
- **Quick API testing** with pre-configured test scenarios
- **Documentation search** with advanced filtering

## 🛠️ Configuration Deep Dive

### Swagger Configuration (Program.cs)
```csharp
// Multiple security schemes
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme { ... });
options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme { ... });
options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme { ... });

// Enhanced UI configuration
options.DocExpansion(DocExpansion.List);
options.EnableDeepLinking();
options.EnableFilter();
options.ShowExtensions();
options.OAuthUsePkce();
```

### Custom Filters
- **ExampleSchemaFilter**: Automatically generates realistic examples
- **SwaggerDefaultValues**: Handles default values and optional parameters
- **EnumSchemaFilter**: Enhanced enum documentation with descriptions

## 📈 Best Practices Demonstrated

### 1. Documentation Quality
- **Comprehensive descriptions** for every endpoint, parameter, and response
- **Realistic examples** that developers can copy and use immediately
- **Error scenarios** with clear explanations and solutions
- **Business context** explaining when and why to use each endpoint

### 2. User Experience
- **Intuitive navigation** with logical grouping and filtering
- **Visual hierarchy** that guides users through the documentation
- **Interactive elements** that encourage exploration and testing
- **Responsive design** that works on all devices

### 3. Developer Productivity
- **One-click authentication** with demo credentials
- **Code generation** for multiple programming languages
- **Performance insights** to help optimize API usage
- **Keyboard shortcuts** for efficient navigation

### 4. Maintainability
- **Modular CSS and JavaScript** for easy customization
- **Configuration-driven** setup that's easy to modify
- **Extensible architecture** for adding new features
- **Clear separation** between content and presentation

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to `https://localhost:5001`
2. Click the **🔐 Auth Helper** button to see authentication options
3. Use **Fill Demo Credentials** buttons to test different user roles
4. Try the **🧪 Quick Test** button for immediate API validation
5. Use **Try it out** on any endpoint to make real API calls

### Using cURL Examples
```bash
# Get library information
curl https://localhost:5001/api/v1/library/info

# List books with authentication
curl -H "Authorization: Bearer demo-user-token" \
     https://localhost:5001/api/v1/books

# Create a new loan with API key
curl -X POST \
     -H "X-API-Key: dev-api-key-12345" \
     -H "Content-Type: application/json" \
     -d '{"borrowerName":"John Doe","borrowerEmail":"john@example.com","bookId":1}' \
     https://localhost:5001/api/v1/loans
```

### Keyboard Shortcuts
- **Ctrl/Cmd + K**: Focus search filter
- **Ctrl/Cmd + D**: Download OpenAPI specification
- **Ctrl/Cmd + H**: Show keyboard shortcuts help
- **Esc**: Close modals and dialogs

## 🔧 Extending the Example

### Adding New Features
1. **Custom Authentication**: Replace demo auth with real implementation
2. **Additional Endpoints**: Add more business logic and endpoints
3. **Database Integration**: Connect to real data storage
4. **Advanced Filtering**: Add complex query capabilities
5. **File Upload**: Add document and image upload functionality

### Customizing the UI
1. **Branding**: Update colors, logos, and styling to match your organization
2. **Additional Languages**: Add more code sample languages
3. **Custom Widgets**: Create domain-specific UI components
4. **Integration**: Connect with other developer tools and services

### Production Considerations
1. **Security**: Implement real authentication and authorization
2. **Performance**: Add caching, rate limiting, and optimization
3. **Monitoring**: Integrate with APM and logging solutions
4. **Testing**: Add comprehensive API testing and validation

## 📚 Learning Resources

### Key Concepts Covered
- **OpenAPI 3.0 Specification** - Complete schema definition
- **Swagger UI Customization** - Theming and functionality enhancement
- **API Documentation Best Practices** - Industry standards and guidelines
- **Authentication Documentation** - Security scheme implementation
- **Developer Experience Design** - UX principles for API documentation

### Related Examples in This Training Series
- **01-TaskManager-ConfigDI**: Basic API setup and configuration
- **06-ProductCatalog-Validation**: Input validation and error handling
- **08-Calculator-Testing**: API testing strategies and implementation

## 🎓 What You'll Learn

By studying this example, you'll understand:

1. **How to create professional API documentation** that developers actually want to use
2. **Advanced Swagger configuration techniques** beyond basic setup
3. **UI/UX principles** for developer-facing documentation
4. **Authentication documentation strategies** for different security schemes
5. **Performance optimization** for documentation sites
6. **Accessibility considerations** for inclusive design
7. **Maintenance strategies** for keeping documentation current

## 🤝 Contributing

This example is part of a comprehensive .NET Core 8 training series. To contribute:

1. **Follow the established patterns** in other examples
2. **Maintain documentation quality** with clear explanations
3. **Test all features** across different browsers and devices
4. **Update this README** when adding new functionality

## 📄 License

This project is part of the .NET Core 8 Training Platform and is provided for educational purposes. See the main repository license for details.

---

**💡 Pro Tip**: Use this example as a template for your own API documentation projects. The patterns and techniques demonstrated here can be adapted to any ASP.NET Core API to create professional, user-friendly documentation that enhances the developer experience.