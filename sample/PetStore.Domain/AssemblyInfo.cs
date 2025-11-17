// Example: Exclude internal services from automatic registration
// [assembly: RegistrationFilter(ExcludeNamespaces = new[] { "PetStore.Domain.Internal" })]

// Example: Exclude test/mock services using pattern matching
// [assembly: RegistrationFilter(ExcludePatterns = new[] { "*Mock*", "*Test*", "*Fake*" })]