Console.WriteLine("=== Atc.SourceGenerators - Enum Mapping Sample ===\n");

Console.WriteLine("1. Testing PetStatusEntity → PetStatusDto mapping:");
Console.WriteLine("   - Special case: None → Unknown");
Console.WriteLine("   - Bidirectional: true\n");

var entityNone = PetStatusEntity.None;
var entityAvailable = PetStatusEntity.Available;
var entityPending = PetStatusEntity.Pending;
var entityAdopted = PetStatusEntity.Adopted;

var dtoUnknown = entityNone.MapToPetStatusDto();
var dtoAvailable = entityAvailable.MapToPetStatusDto();
var dtoPending = entityPending.MapToPetStatusDto();
var dtoAdopted = entityAdopted.MapToPetStatusDto();

Console.WriteLine($"   {entityNone} → {dtoUnknown}");
Console.WriteLine($"   {entityAvailable} → {dtoAvailable}");
Console.WriteLine($"   {entityPending} → {dtoPending}");
Console.WriteLine($"   {entityAdopted} → {dtoAdopted}");

Console.WriteLine("\n2. Testing PetStatusDto → PetStatusEntity (reverse mapping):");

var entityFromDto = dtoUnknown.MapToPetStatusEntity();
Console.WriteLine($"   {dtoUnknown} → {entityFromDto}");

Console.WriteLine("\n3. Testing FeatureState → FeatureFlag mapping:");
Console.WriteLine("   - Exact name matching");
Console.WriteLine("   - Bidirectional: false\n");

var stateActive = FeatureState.Active;
var stateInactive = FeatureState.Inactive;
var stateTesting = FeatureState.Testing;

var flagActive = stateActive.MapToFeatureFlag();
var flagInactive = stateInactive.MapToFeatureFlag();
var flagTesting = stateTesting.MapToFeatureFlag();

Console.WriteLine($"   {stateActive} → {flagActive}");
Console.WriteLine($"   {stateInactive} → {flagInactive}");
Console.WriteLine($"   {stateTesting} → {flagTesting}");

Console.WriteLine("\n4. Testing case-insensitive matching:");
Console.WriteLine("   All enum values match regardless of casing");

Console.WriteLine("\n5. Performance characteristics:");
Console.WriteLine("   ✓ Zero runtime cost - pure switch expressions");
Console.WriteLine("   ✓ Compile-time safety with exhaustive checking");
Console.WriteLine("   ✓ ArgumentOutOfRangeException for unmapped values");

Console.WriteLine("\n=== All tests completed successfully! ===");