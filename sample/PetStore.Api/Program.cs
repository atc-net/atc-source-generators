// ReSharper disable InvertIf
// ReSharper disable RedundantArgumentDefaultValue

var builder = WebApplication.CreateBuilder(args);

// ✨ Register all services transitively (Domain + DataAccess)
// This single call registers:
//   - PetService from PetStore.Domain
//   - PetRepository from PetStore.DataAccess (auto-detected as referenced assembly)
builder.Services.AddDependencyRegistrationsFromDomain(
    includeReferencedAssemblies: true);

// ✨ Register configuration options automatically via [OptionsBinding] attribute
// This single call registers options from PetStore.Domain (PetStoreOptions + PetMaintenanceServiceOptions)
builder.Services.AddOptionsFromDomain(
    builder.Configuration,
    includeReferencedAssemblies: true);

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

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
    .MapGet("/pets", (IPetService petService) =>
    {
        var pets = petService.GetAll();

        // Use generated mapping: Pet → PetResponse
        var response = pets.Select(p => p.MapToPetResponse());

        return Results.Ok(response);
    })
    .WithName("GetAllPets")
    .Produces<IEnumerable<PetResponse>>(StatusCodes.Status200OK);

app
    .MapGet("/pets/{id:guid}", (Guid id, IPetService petService) =>
    {
        var pet = petService.GetById(id);

        if (pet is null)
        {
            return Results.NotFound(new { Message = $"Pet with ID {id} not found." });
        }

        // Use generated mapping: Pet → PetResponse
        var response = pet.MapToPetResponse();

        return Results.Ok(response);
    })
    .WithName("GetPetById")
    .Produces<PetResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app
    .MapGet("/pets/status/{status}", ([FromRoute] PetStatus status, IPetService petService) =>
    {
        var pets = petService.GetByStatus((PetStore.Domain.Models.PetStatus)status);

        // Use generated mapping: Pet → PetResponse
        var response = pets.Select(p => p.MapToPetResponse());

        return Results.Ok(response);
    })
    .WithName("GetPetsByStatus")
    .Produces<IEnumerable<PetResponse>>(StatusCodes.Status200OK);

app
    .MapGet("/pets/summary", (IPetService petService) =>
    {
        var pets = petService.GetAll();

        // ✨ Use generated flattening mapping: Pet → PetSummaryResponse
        // Demonstrates property flattening where nested Owner properties are flattened
        // to OwnerName, OwnerEmail, OwnerPhone in the target DTO
        var response = pets.Select(p => p.MapToPetSummaryResponse());

        return Results.Ok(response);
    })
    .WithName("GetAllPetsSummary")
    .WithSummary("Get all pets with flattened owner properties")
    .WithDescription("Demonstrates property flattening feature where nested Owner.Name becomes OwnerName, Owner.Email becomes OwnerEmail, etc.")
    .Produces<IEnumerable<PetSummaryResponse>>(StatusCodes.Status200OK);

app
    .MapGet("/pets/details", (IPetService petService) =>
    {
        var pets = petService.GetAll();

        // ✨ Demonstrate built-in type conversion: Strong types → String DTOs
        // Shows automatic conversion of Guid → string, int → string, DateTimeOffset → string (ISO 8601)
        var response = pets.Select(p => p.MapToPetDetailsDto());

        return Results.Ok(response);
    })
    .WithName("GetAllPetsDetails")
    .WithSummary("Get all pets with type conversion to strings")
    .WithDescription("Demonstrates built-in type conversion where Guid → string, int → string, DateTimeOffset → string (ISO 8601)")
    .Produces<IEnumerable<PetDetailsDto>>(StatusCodes.Status200OK);

app
    .MapPost("/pets", ([FromBody] CreatePetRequest request, IPetService petService) =>
    {
        var pet = petService.CreatePet(request);

        // Use generated mapping: Pet → PetResponse
        var response = pet.MapToPetResponse();

        return Results.Created($"/pets/{pet.Id}", response);
    })
    .WithName("CreatePet")
    .Produces<PetResponse>(StatusCodes.Status201Created);

app
    .MapPut("/pets/{id:guid}", (Guid id, [FromBody] UpdatePetRequest request, IPetService petService) =>
    {
        var existingPet = petService.GetById(id);
        if (existingPet is null)
        {
            return Results.NotFound(new { Message = $"Pet with ID {id} not found." });
        }

        // ✨ Demonstrate required property validation with UpdatePetRequest
        // UpdatePetRequest has required properties (Name, Species)
        // The generator validated at compile-time that Pet domain model has all required properties
        // If Pet was missing Name or Species, you would get ATCMAP004 warning at build time

        // Update pet properties from request
        existingPet.Name = request.Name;
        existingPet.Species = request.Species;
        if (request.Age.HasValue)
        {
            existingPet.Age = request.Age.Value;
        }

        // Return updated pet
        var response = existingPet.MapToPetResponse();
        return Results.Ok(response);
    })
    .WithName("UpdatePet")
    .WithSummary("Update a pet with required property validation")
    .WithDescription("Demonstrates required property validation where UpdatePetRequest has 'required' properties (Name, Species). The generator ensures at compile-time that all required properties can be mapped from the Pet domain model.")
    .Produces<PetResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// Demonstrate instance registration
app
    .MapGet("/config", (IApiConfiguration config) =>
    {
        // The IApiConfiguration is registered using instance registration
        // It uses ApiConfiguration.DefaultInstance property to provide a pre-created singleton
        var configInfo = new
        {
            config.ApiVersion,
            config.MaxPageSize,
            config.EnableApiDocumentation,
            config.BaseUrl,
            RateLimitPerMinute = config.GetConfigValue("RateLimitPerMinute"),
            CacheDurationSeconds = config.GetConfigValue("CacheDurationSeconds"),
            EnableLogging = config.GetConfigValue("EnableLogging"),
        };

        return Results.Ok(configInfo);
    })
    .WithName("GetApiConfiguration")
    .Produces<object>(StatusCodes.Status200OK);

app
    .MapGet("/notifications", () =>
    {
        // ✨ Demonstrate polymorphic mapping: Domain → DTO
        // Shows automatic type pattern matching where base class maps to derived types
        var notifications = new List<PetStore.Domain.Models.Notification>
        {
            new PetStore.Domain.Models.EmailNotification
            {
                Id = Guid.NewGuid(),
                Message = "Your pet vaccination is due next week",
                CreatedAt = DateTimeOffset.UtcNow,
                To = "owner@example.com",
                Subject = "Pet Vaccination Reminder",
            },
            new PetStore.Domain.Models.SmsNotification
            {
                Id = Guid.NewGuid(),
                Message = "Your pet grooming appointment is confirmed",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                PhoneNumber = "+1-555-0123",
            },
            new PetStore.Domain.Models.EmailNotification
            {
                Id = Guid.NewGuid(),
                Message = "Your pet adoption application has been approved!",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                To = "newowner@example.com",
                Subject = "Pet Adoption Approved",
            },
        };

        // The generated MapToNotificationDto() uses a switch expression to map each derived type
        var response = notifications
            .Select(n => n.MapToNotificationDto())
            .ToList();

        return Results.Ok(response);
    })
    .WithName("GetAllNotifications")
    .WithSummary("Get all notifications with polymorphic mapping")
    .WithDescription("Demonstrates polymorphic mapping where abstract Notification base class maps to derived EmailNotification/SmsNotification types using type pattern matching.")
    .Produces<List<NotificationDto>>(StatusCodes.Status200OK);

app
    .MapGet("/analytics/pets/{id:guid}", (Guid id) =>
    {
        // ✨ Demonstrate PropertyNameStrategy.CamelCase: PascalCase → camelCase
        // Shows automatic property name conversion for JavaScript analytics dashboards
        var analytics = new PetStore.Domain.Models.PetAnalytics
        {
            PetId = id,
            TotalVisits = 142,
            TotalAdoptions = 3,
            AverageVisitDuration = 8.5,
            MostPopularTimeSlot = "14:00-16:00",
            LastUpdated = DateTimeOffset.UtcNow,
        };

        // Generated mapping converts PascalCase properties to camelCase
        // PetId → petId, TotalVisits → totalVisits, AverageVisitDuration → averageVisitDuration, etc.
        var response = analytics.MapToPetAnalyticsDto();

        return Results.Ok(response);
    })
    .WithName("GetPetAnalytics")
    .WithSummary("Get pet analytics with camelCase property names")
    .WithDescription("Demonstrates PropertyNameStrategy.CamelCase where PascalCase domain properties (PetId, TotalVisits, AverageVisitDuration) are automatically mapped to camelCase DTO properties (petId, totalVisits, averageVisitDuration) for JavaScript/JSON dashboard compatibility.")
    .Produces<PetAnalyticsDto>(StatusCodes.Status200OK);

await app.RunAsync();