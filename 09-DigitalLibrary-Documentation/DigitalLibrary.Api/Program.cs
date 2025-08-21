using DigitalLibrary.Api.Data;
using DigitalLibrary.Api.Models;
using DigitalLibrary.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Configure Entity Framework with In-Memory Database for demonstration
builder.Services.AddDbContext<DigitalLibraryDbContext>(options =>
    options.UseInMemoryDatabase("DigitalLibraryDb"));

// Configure Swagger/OpenAPI with advanced settings and versioning
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // API version 1.0
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Digital Library API",
        Description = "A comprehensive API for managing a digital library system with books, authors, and loans. " +
                     "This version includes full CRUD operations for books, authors, and loan management with rich documentation.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Digital Library Support Team",
            Email = "support@digitallibrary.com",
            Url = new Uri("https://digitallibrary.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Future API version 2.0 (placeholder for demonstration)
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "Digital Library API",
        Description = "Enhanced version of the Digital Library API with additional features like advanced search, " +
                     "user management, and reservation system. This version includes breaking changes from v1.0.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Digital Library Support Team",
            Email = "support@digitallibrary.com",
            Url = new Uri("https://digitallibrary.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Enable XML comments for detailed documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);

    // Enable annotations for additional metadata
    options.EnableAnnotations();

    // Configure multiple security schemes for demonstration
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Add your API key to the X-API-Key header.",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Description = "Basic HTTP authentication. Enter username and password.",
        Type = SecuritySchemeType.Http,
        Scheme = "basic"
    });

    // Add OAuth2 simulation for demonstration
    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Description = "OAuth2 authorization code flow (simulated for demonstration)",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://example.com/oauth/authorize"),
                TokenUrl = new Uri("https://example.com/oauth/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "read", "Read access to library resources" },
                    { "write", "Write access to library resources" },
                    { "admin", "Administrative access to all resources" }
                }
            }
        }
    });

    // Add global security requirement with multiple options
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Custom operation filters for better documentation
    options.OperationFilter<SwaggerDefaultValues>();
    
    // Configure enum descriptions
    options.SchemaFilter<EnumSchemaFilter>();
    
    // Add examples for request/response models
    options.SchemaFilter<ExampleSchemaFilter>();
});

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DigitalLibraryDbContext>();
    context.Database.EnsureCreated();
    
    // Seed sample data if database is empty
    if (!context.Authors.Any())
    {
        SeedSampleData(context);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Configure endpoints for different API versions
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Library API v1.0");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Digital Library API v2.0 (Future)");
        
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
        options.DocumentTitle = "📚 Digital Library API - Interactive Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableDeepLinking();
        options.DisplayOperationId();
        options.DisplayRequestDuration();
        options.EnableValidator();
        options.EnableFilter();
        options.ShowExtensions();
        options.ShowCommonExtensions();
        options.SupportedSubmitMethods(Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
                                      Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, 
                                      Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, 
                                      Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete);
        
        // OAuth2 configuration for demonstration
        options.OAuthClientId("digital-library-swagger-ui");
        options.OAuthAppName("Digital Library API Documentation");
        options.OAuthScopeSeparator(" ");
        options.OAuthUsePkce();
        
        // Custom CSS for better appearance
        options.InjectStylesheet("/swagger-ui/custom.css");
        
        // Custom JavaScript for enhanced functionality
        options.InjectJavascript("/swagger-ui/custom.js");
        
        // Add custom configuration
        options.ConfigObject.AdditionalItems.Add("syntaxHighlight", new Dictionary<string, object>
        {
            ["activated"] = true,
            ["theme"] = "agate"
        });
        
        options.ConfigObject.AdditionalItems.Add("tryItOutEnabled", true);
        options.ConfigObject.AdditionalItems.Add("requestSnippetsEnabled", true);
        options.ConfigObject.AdditionalItems.Add("requestSnippets", new Dictionary<string, object>
        {
            ["generators"] = new Dictionary<string, object>
            {
                ["curl_bash"] = new { title = "cURL (bash)", syntax = "bash" },
                ["curl_powershell"] = new { title = "cURL (PowerShell)", syntax = "powershell" },
                ["curl_cmd"] = new { title = "cURL (CMD)", syntax = "bash" }
            },
            ["defaultExpanded"] = true,
            ["languages"] = new[] { "curl_bash", "curl_powershell", "curl_cmd" }
        });
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // For serving custom CSS

app.UseAuthorization();

app.MapControllers();

app.Run();

// Sample data seeding method
static void SeedSampleData(DigitalLibraryDbContext context)
{
    // Create sample authors
    var authors = new[]
    {
        new Author
        {
            FirstName = "Jane",
            LastName = "Austen",
            Biography = "English novelist known for her wit, social commentary and insight into women's lives in the early 19th century.",
            BirthDate = new DateTime(1775, 12, 16),
            Nationality = "British",
            Email = "jane.austen@classic.com"
        },
        new Author
        {
            FirstName = "George",
            LastName = "Orwell",
            Biography = "English novelist and essayist, journalist and critic, best known for his dystopian novels.",
            BirthDate = new DateTime(1903, 6, 25),
            Nationality = "British",
            Email = "george.orwell@dystopian.com"
        },
        new Author
        {
            FirstName = "Agatha",
            LastName = "Christie",
            Biography = "English writer known for her detective novels, especially those featuring Hercule Poirot and Miss Marple.",
            BirthDate = new DateTime(1890, 9, 15),
            Nationality = "British",
            Email = "agatha.christie@mystery.com"
        }
    };

    context.Authors.AddRange(authors);
    context.SaveChanges();

    // Create sample books
    var books = new[]
    {
        new Book
        {
            Title = "Pride and Prejudice",
            Description = "A romantic novel that critiques the British landed gentry at the end of the 18th century.",
            ISBN = "978-0-14-143951-8",
            PublicationDate = new DateTime(1813, 1, 28),
            Publisher = "T. Egerton",
            PageCount = 432,
            Genre = "Romance",
            Language = "English",
            TotalCopies = 5,
            AvailableCopies = 4,
            AverageRating = 4.5m,
            RatingCount = 150,
            AuthorId = authors[0].Id
        },
        new Book
        {
            Title = "1984",
            Description = "A dystopian social science fiction novel about totalitarian control and surveillance.",
            ISBN = "978-0-452-28423-4",
            PublicationDate = new DateTime(1949, 6, 8),
            Publisher = "Secker & Warburg",
            PageCount = 328,
            Genre = "Dystopian Fiction",
            Language = "English",
            TotalCopies = 8,
            AvailableCopies = 6,
            AverageRating = 4.7m,
            RatingCount = 200,
            AuthorId = authors[1].Id
        },
        new Book
        {
            Title = "Murder on the Orient Express",
            Description = "A detective novel featuring the Belgian detective Hercule Poirot.",
            ISBN = "978-0-00-711693-6",
            PublicationDate = new DateTime(1934, 1, 1),
            Publisher = "Collins Crime Club",
            PageCount = 256,
            Genre = "Mystery",
            Language = "English",
            TotalCopies = 3,
            AvailableCopies = 2,
            AverageRating = 4.3m,
            RatingCount = 89,
            AuthorId = authors[2].Id
        },
        new Book
        {
            Title = "Animal Farm",
            Description = "An allegorical novella about farm animals who rebel against their human farmer.",
            ISBN = "978-0-452-28424-1",
            PublicationDate = new DateTime(1945, 8, 17),
            Publisher = "Secker & Warburg",
            PageCount = 112,
            Genre = "Political Satire",
            Language = "English",
            TotalCopies = 6,
            AvailableCopies = 5,
            AverageRating = 4.2m,
            RatingCount = 175,
            AuthorId = authors[1].Id
        }
    };

    context.Books.AddRange(books);
    context.SaveChanges();

    // Create sample loans
    var loans = new[]
    {
        new Loan
        {
            BorrowerName = "John Smith",
            BorrowerEmail = "john.smith@email.com",
            BorrowerPhone = "+1-555-123-4567",
            LoanDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(4),
            Status = LoanStatus.Active,
            Notes = "Regular loan",
            BookId = books[0].Id
        },
        new Loan
        {
            BorrowerName = "Sarah Johnson",
            BorrowerEmail = "sarah.johnson@email.com",
            BorrowerPhone = "+1-555-987-6543",
            LoanDate = DateTime.UtcNow.AddDays(-25),
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = LoanStatus.Active,
            Notes = "Overdue loan",
            BookId = books[1].Id
        },
        new Loan
        {
            BorrowerName = "Mike Wilson",
            BorrowerEmail = "mike.wilson@email.com",
            LoanDate = DateTime.UtcNow.AddDays(-30),
            DueDate = DateTime.UtcNow.AddDays(-16),
            ReturnDate = DateTime.UtcNow.AddDays(-14),
            Status = LoanStatus.ReturnedLate,
            FineAmount = 2.00m,
            FinePaid = true,
            BookId = books[2].Id
        }
    };

    context.Loans.AddRange(loans);
    context.SaveChanges();
}
