// Exclude internal services from automatic registration
[assembly: RegistrationFilter(
    ExcludeNamespaces = ["Atc.SourceGenerators.DependencyRegistration.Services.Internal"])]

// Exclude mock and test services using pattern matching
[assembly: RegistrationFilter(
    ExcludePatterns = ["*Mock*", "*Test*"])]