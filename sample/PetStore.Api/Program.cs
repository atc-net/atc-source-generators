// ReSharper disable InvertIf
// ReSharper disable RedundantArgumentDefaultValue

var builder = WebApplication.CreateBuilder(args);

// ✨ Scenario B: Register all services transitively (Domain + DataAccess)
// This single call registers:
//   - PetService from PetStore.Domain
//   - PetRepository from PetStore.DataAccess (auto-detected as referenced assembly)
builder.Services.AddDependencyRegistrationsFromDomain(
    includeReferencedAssemblies: true);

// ✨ Register configuration options automatically via [OptionsBinding] attribute
// This single call registers options from PetStore.Domain (PetStoreOptions)
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
        if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
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
    .MapPost("/pets", ([FromBody] CreatePetRequest request, IPetService petService) =>
    {
        var pet = petService.CreatePet(request);

        // Use generated mapping: Pet → PetResponse
        var response = pet.MapToPetResponse();

        return Results.Created($"/pets/{pet.Id}", response);
    })
    .WithName("CreatePet")
    .Produces<PetResponse>(StatusCodes.Status201Created);

await app.RunAsync();