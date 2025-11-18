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

        return context.Response.WriteAsync("PetStore API is running!");
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

await app.RunAsync();