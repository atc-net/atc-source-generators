// ReSharper disable InvertIf
// ReSharper disable RedundantArgumentDefaultValue

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services
builder.Services.AddSingleton<IUserRepository, UserRepository>();

// Register domain services
builder.Services.AddSingleton<UserService>();

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Redirect root to Scalar UI in development
app
    .MapGet("/", context =>
    {
        if (context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment())
        {
            context.Response.Redirect("/scalar/v1");
            return Task.CompletedTask;
        }

        return context.Response.WriteAsync("PetStore API is running!", CancellationToken.None);
    });

app
    .MapGet("/users/{id:guid}", (Guid id, UserService userService) =>
    {
        var user = userService.GetById(id);
        if (user is null)
        {
            return Results.NotFound(new { message = $"User with ID {id} not found" });
        }

        // ✨ Use generated mapping: Domain → DTO
        var data = user.MapToUserDto();
        return Results.Ok(data);
    })
    .WithName("GetUserById")
    .Produces<UserDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app
    .MapGet("/users", (UserService userService) =>
    {
        // ✨ Use generated mapping: Domain → DTO
        var data = userService
            .GetAll()
            .Select(u => u.MapToUserDto())
            .ToList();
        return Results.Ok(data);
    })
    .WithName("GetAllUsers")
    .Produces<List<UserDto>>(StatusCodes.Status200OK);

app
    .MapGet("/users/flat", (UserService userService) =>
    {
        // ✨ Use generated flattening mapping: Domain → Flattened DTO
        // Demonstrates property flattening where nested Address properties are flattened
        // to AddressCity, AddressStreet, etc. in the target DTO
        var data = userService
            .GetAll()
            .Select(u => u.MapToUserFlatDto())
            .ToList();
        return Results.Ok(data);
    })
    .WithName("GetAllUsersFlat")
    .WithSummary("Get all users with flattened address properties")
    .WithDescription("Demonstrates property flattening feature where nested Address.City becomes AddressCity, Address.Street becomes AddressStreet, etc.")
    .Produces<List<UserFlatDto>>(StatusCodes.Status200OK);

app
    .MapGet("/events", () =>
    {
        // ✨ Demonstrate built-in type conversion: Strong types → String DTOs
        // Shows automatic conversion of Guid, DateTimeOffset, int, and bool to string
        var events = new List<UserEvent>
        {
            new()
            {
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = "Login",
                Timestamp = DateTimeOffset.UtcNow,
                DurationSeconds = 5,
                Success = true,
            },
            new()
            {
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = "Logout",
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
                DurationSeconds = 2,
                Success = true,
            },
        };

        var data = events
            .Select(e => e.MapToUserEventDto())
            .ToList();
        return Results.Ok(data);
    })
    .WithName("GetAllEvents")
    .WithSummary("Get user events with type conversion")
    .WithDescription("Demonstrates built-in type conversion where Guid → string, DateTimeOffset → string (ISO 8601), int → string, bool → string")
    .Produces<List<UserEventDto>>(StatusCodes.Status200OK);

app
    .MapGet("/result", () =>
    {
        // ✨ Demonstrate generic type mapping: Result<T> → ResultDto<T>
        // Shows generic mappers preserving type parameters
        var result = new Result<string>
        {
            Data = "Success!",
            Success = true,
            Message = "Operation completed successfully",
            ErrorCode = null,
        };

        // The generated MapToResultDto<T>() preserves the type parameter
        var data = result.MapToResultDto();
        return Results.Ok(data);
    })
    .WithName("GetResult")
    .WithSummary("Get a result with generic type mapping")
    .WithDescription("Demonstrates generic type mapping where Result<T> maps to ResultDto<T> preserving the type parameter.")
    .Produces<ResultDto<string>>(StatusCodes.Status200OK);

app
    .MapGet("/users/paged", (UserService userService) =>
    {
        // ✨ Demonstrate generic type mapping with constraints: PagedResult<T> → PagedResultDto<T>
        // Shows generic mappers with 'where T : class' constraint
        var users = userService.GetAll();
        var userList = users.ToList();
        var firstPage = userList.Take(10);
        var pagedResult = new PagedResult<User>
        {
            Items = firstPage.ToList(),
            TotalCount = userList.Count,
            PageNumber = 1,
            PageSize = 10,
        };

        // The generated MapToPagedResultDto<T>() preserves type parameter and constraints
        var data = pagedResult.MapToPagedResultDto();
        return Results.Ok(data);
    })
    .WithName("GetPagedUsers")
    .WithSummary("Get paged users with generic type mapping")
    .WithDescription("Demonstrates generic type mapping with constraints where PagedResult<T> maps to PagedResultDto<T> with 'where T : class' constraint.")
    .Produces<PagedResultDto<UserDto>>(StatusCodes.Status200OK);

app
    .MapPost("/register", (UserRegistration registration) =>
    {
        // ✨ Use generated mapping with required property validation: Domain → DTO
        // UserRegistrationDto has required properties (Email, FullName)
        // The generator validated at compile-time that all required properties are mapped
        // If UserRegistration was missing Email or FullName, you would get ATCMAP004 warning
        var data = registration.MapToUserRegistrationDto();
        return Results.Ok(new
        {
            Message = "Registration successful!",
            Data = data,
        });
    })
    .WithName("RegisterUser")
    .WithSummary("Register a new user with required property validation")
    .WithDescription("Demonstrates required property validation where target DTO has 'required' properties (Email, FullName). The generator ensures at compile-time that all required properties are mapped.")
    .Produces<object>(StatusCodes.Status200OK);

app
    .MapGet("/animals", () =>
    {
        // ✨ Demonstrate polymorphic mapping: Domain → DTO
        // Shows automatic type pattern matching where base class maps to derived types
        var animals = new List<Animal>
        {
            new Dog
            {
                Id = 1,
                Name = "Buddy",
                Breed = "Golden Retriever",
            },
            new Cat
            {
                Id = 2,
                Name = "Whiskers",
                Lives = 9,
            },
            new Dog
            {
                Id = 3,
                Name = "Max",
                Breed = "German Shepherd",
            },
        };

        // The generated MapToAnimalDto() uses a switch expression to map each derived type
        var data = animals
            .Select(a => a.MapToAnimalDto())
            .ToList();
        return Results.Ok(data);
    })
    .WithName("GetAllAnimals")
    .WithSummary("Get all animals with polymorphic mapping")
    .WithDescription("Demonstrates polymorphic mapping where abstract Animal base class maps to derived Dog/Cat types using type pattern matching.")
    .Produces<List<AnimalDto>>(StatusCodes.Status200OK);

app
    .MapGet("/config/api", () =>
    {
        // ✨ Demonstrate PropertyNameStrategy.CamelCase: PascalCase → camelCase
        // Shows automatic property name conversion for JavaScript/JSON APIs
        var config = new ApiConfiguration
        {
            ApiEndpoint = "https://api.example.com/v1",
            ApiKey = "sk_test_1234567890",
            TimeoutSeconds = 30,
            EnableRetry = true,
            MaxRetryAttempts = 3,
        };

        // Generated mapping converts PascalCase properties to camelCase
        // ApiEndpoint → apiEndpoint, ApiKey → apiKey, etc.
        var data = config.MapToApiConfigurationDto();
        return Results.Ok(data);
    })
    .WithName("GetApiConfiguration")
    .WithSummary("Get API configuration with camelCase property names")
    .WithDescription("Demonstrates PropertyNameStrategy.CamelCase where PascalCase domain properties (ApiEndpoint, ApiKey) are automatically mapped to camelCase DTO properties (apiEndpoint, apiKey) for JavaScript/JSON compatibility.")
    .Produces<ApiConfigurationDto>(StatusCodes.Status200OK);

app
    .MapGet("/config/database", () =>
    {
        // ✨ Demonstrate PropertyNameStrategy.SnakeCase: PascalCase → snake_case
        // Shows automatic property name conversion for PostgreSQL/Python APIs
        var settings = new DatabaseSettings
        {
            DatabaseHost = "localhost",
            DatabasePort = 5432,
            DatabaseName = "myapp_production",
            ConnectionTimeout = 60,
            EnableSsl = true,
        };

        // Generated mapping converts PascalCase properties to snake_case
        // DatabaseHost → database_host, DatabasePort → database_port, etc.
        var data = settings.MapToDatabaseSettingsDto();
        return Results.Ok(data);
    })
    .WithName("GetDatabaseConfiguration")
    .WithSummary("Get database configuration with snake_case property names")
    .WithDescription("Demonstrates PropertyNameStrategy.SnakeCase where PascalCase domain properties (DatabaseHost, DatabasePort) are automatically mapped to snake_case DTO properties (database_host, database_port) for PostgreSQL/Python compatibility.")
    .Produces<DatabaseSettingsDto>(StatusCodes.Status200OK);

await app
    .RunAsync()
    .ConfigureAwait(false);