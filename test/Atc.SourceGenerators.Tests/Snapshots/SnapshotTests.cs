namespace Atc.SourceGenerators.Tests.Snapshots;

/// <summary>
/// Snapshots of the complete generated output (all files and diagnostics) for one representative input per
/// generator. A change to any generated line shows up as a snapshot diff in review; accept an intended change by
/// replacing the .verified file with the .received file.
/// </summary>
public class SnapshotTests
{
    private static readonly HashSet<string> AttributeHintNames = new(StringComparer.Ordinal)
    {
        "Microsoft.CodeAnalysis.EmbeddedAttribute.cs",
        "RegistrationAttribute.g.cs",
        "OptionsBindingAttribute.g.cs",
        "MapToAttribute.g.cs",
        "MapIgnoreAttribute.g.cs",
        "MapPropertyAttribute.g.cs",
        "MapDerivedTypeAttribute.g.cs",
        "MappingConfigurationAttribute.g.cs",
        "MapConfigIgnoreAttribute.g.cs",
        "MapConfigPropertyAttribute.g.cs",
        "MapConfigOptionsAttribute.g.cs",
        "MapTypesAttribute.g.cs",
        "MappingBuilder.g.cs",
    };

    [Fact]
    public void Attribute_Definitions()
        => VerifyGenerated(
            "namespace MyApp;",
            includeAttributes: true,
            [
                new DependencyRegistrationGenerator(),
                new OptionsBindingGenerator(),
                new ObjectMappingGenerator(),
                new MappingConfigurationGenerator(),
            ]);

    [Fact]
    public void DependencyRegistration()
        => VerifyGenerated(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Atc.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            namespace MyApp;

            public interface IUserService
            {
            }

            [Registration(Lifetime.Scoped)]
            public class UserService : IUserService
            {
            }

            public interface IRepository<T>
            {
            }

            [Registration(Lifetime.Scoped)]
            public class Repository<T> : IRepository<T>
            {
            }

            public interface IPaymentProcessor
            {
            }

            [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
            public class StripePaymentProcessor : IPaymentProcessor
            {
            }

            public interface IEmailSender
            {
            }

            [Registration(Lifetime.Transient, As = typeof(IEmailSender), Factory = nameof(Create))]
            public class EmailSender : IEmailSender
            {
                public static IEmailSender Create(IServiceProvider serviceProvider) => new EmailSender();
            }

            public interface IOrderService
            {
            }

            [Registration(Lifetime.Scoped, As = typeof(IOrderService))]
            public class OrderService : IOrderService
            {
            }

            [Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
            public class LoggingOrderService : IOrderService
            {
                public LoggingOrderService(IOrderService inner)
                {
                }
            }

            public interface ICache
            {
            }

            [Registration(As = typeof(ICache), Condition = "Features:UseRedis")]
            public class RedisCache : ICache
            {
            }

            [Registration(As = typeof(ICache), TryAdd = true)]
            public class MemoryCache : ICache
            {
            }

            [Registration]
            public class MaintenanceService : BackgroundService
            {
                protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
            }
            """,
            includeAttributes: false,
            [
                new DependencyRegistrationGenerator(),
            ]);

    [Fact]
    public void OptionsBinding()
        => VerifyGenerated(
            """
            using System.ComponentModel.DataAnnotations;
            using Atc.SourceGenerators.Annotations;

            namespace MyApp;

            [OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
            public partial class DatabaseOptions
            {
                [Required]
                public string ConnectionString { get; set; } = string.Empty;
            }

            [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
            public partial class FeaturesOptions
            {
                public bool EnableNewUi { get; set; }

                internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
                {
                }
            }

            [OptionsBinding("Email", ChildSections = new[] { "Primary", "Secondary" }, ConfigureAll = nameof(SetDefaults))]
            public partial class EmailOptions
            {
                public int Port { get; set; }

                internal static void SetDefaults(EmailOptions options) => options.Port = 587;
            }

            [OptionsBinding("Storage", PostConfigure = nameof(Normalize), ErrorOnMissingKeys = true)]
            public partial class StorageOptions
            {
                public string BasePath { get; set; } = string.Empty;

                internal static void Normalize(StorageOptions options) => options.BasePath = options.BasePath.TrimEnd('/');
            }
            """,
            includeAttributes: false,
            [
                new OptionsBindingGenerator(),
            ]);

    [Fact]
    public void ObjectMapping()
        => VerifyGenerated(
            """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using Atc.SourceGenerators.Annotations;

            namespace MyApp;

            public enum StatusDto
            {
                Unknown,
                Active,
            }

            public class AddressDto
            {
                public string City { get; set; } = string.Empty;
            }

            public class OrderLineDto
            {
                public int Quantity { get; set; }
            }

            public sealed record CustomerDto(Guid Id, string Name)
            {
                public StatusDto Status { get; set; }

                public AddressDto? Address { get; set; }

                public List<OrderLineDto> Lines { get; set; } = [];

                public Dictionary<string, OrderLineDto> LinesByKey { get; set; } = [];

                public string CreatedAt { get; set; } = string.Empty;

                public string? AddressCity { get; set; }
            }

            [MapTo(typeof(StatusDto), Bidirectional = true)]
            public enum StatusSource
            {
                None,
                Active,
            }

            [MapTo(typeof(AddressDto), Bidirectional = true)]
            public partial class Address
            {
                public string City { get; set; } = string.Empty;
            }

            [MapTo(typeof(OrderLineDto))]
            public partial class OrderLine
            {
                public int Quantity { get; set; }
            }

            public abstract class EntityBase
            {
                public Guid Id { get; set; }
            }

            [MapTo(typeof(CustomerDto), EnableFlattening = true)]
            public partial class Customer : EntityBase
            {
                public string Name { get; set; } = string.Empty;

                public StatusSource Status { get; set; }

                public Address? Address { get; set; }

                public List<OrderLine> Lines { get; set; } = [];

                public Dictionary<string, OrderLine> LinesByKey { get; set; } = [];

                public DateTime CreatedAt { get; set; }

                [MapIgnore]
                public string Secret { get; set; } = string.Empty;
            }
            """,
            includeAttributes: false,
            [
                new ObjectMappingGenerator(),
                new EnumMappingGenerator(),
            ]);

    [Fact]
    public void EnumMapping()
        => VerifyGenerated(
            """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp;

            public enum PetStatusDto
            {
                Unknown,
                Available,
                Pending,
                Adopted,
            }

            [MapTo(typeof(PetStatusDto), Bidirectional = true)]
            public enum PetStatus
            {
                None,
                Pending,
                Available,
                Adopted,
            }
            """,
            includeAttributes: false,
            [
                new ObjectMappingGenerator(),
                new EnumMappingGenerator(),
            ]);

    [Fact]
    public void MappingConfiguration()
        => VerifyGenerated(
            """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using Atc.SourceGenerators.Annotations;

            [assembly: MapTypes(typeof(MyApp.Contact), typeof(MyApp.ContactDto), Bidirectional = true)]

            namespace MyApp
            {
                public enum ContactType
                {
                    None,
                    Active,
                }

                public enum ContactCategory
                {
                    Unknown,
                    Active,
                }

                public class Address
                {
                    public string City { get; set; } = string.Empty;
                }

                public class AddressDto
                {
                    public string City { get; set; } = string.Empty;
                }

                public class Contact
                {
                    public int Id { get; set; }

                    public string FullName { get; set; } = string.Empty;

                    public ContactType Type { get; set; }

                    public Address? Address { get; set; }

                    public List<Address> Addresses { get; set; } = [];

                    public IReadOnlyDictionary<string, Address> AddressesByKey { get; set; } = new Dictionary<string, Address>();

                    public decimal Amount { get; set; }

                    public string InternalId { get; set; } = string.Empty;
                }

                public class ContactDto
                {
                    public int Id { get; set; }

                    public string FullName { get; set; } = string.Empty;
                }

                public sealed record CustomerDto(int Id, string DisplayName)
                {
                    public ContactCategory Type { get; set; }

                    public AddressDto? Address { get; set; }

                    public List<AddressDto> Addresses { get; set; } = [];

                    public IReadOnlyDictionary<string, AddressDto> AddressesByKey { get; set; } = new Dictionary<string, AddressDto>();

                    public string? City { get; set; }

                    public Money Amount { get; set; } = new(0m);
                }

                public sealed record Money(decimal Value);

                [MappingConfiguration]
                public static partial class CrmMappings
                {
                    [MapConfigProperty("FullName", "DisplayName")]
                    [MapConfigProperty("Address.City", "City")]
                    [MapConfigIgnore("InternalId")]
                    public static partial CustomerDto MapToCustomerDto(this Contact source);

                    private static Money ToMoney(decimal amount) => new(amount);
                }
            }
            """,
            includeAttributes: false,
            [
                new MappingConfigurationGenerator(),
                new ObjectMappingGenerator(),
                new EnumMappingGenerator(),
            ]);

    [Fact]
    public void AnnotationConstants()
        => VerifyGenerated(
            """
            using System.ComponentModel.DataAnnotations;

            namespace MyApp;

            public class Product
            {
                [Display(Name = "Product Name", Description = "The display name of the product")]
                [Required]
                [StringLength(100, MinimumLength = 2)]
                public string Name { get; set; } = string.Empty;

                [Display(Name = "Price")]
                [Range(typeof(decimal), "0.01", "999999.99")]
                public decimal Price { get; set; }

                [EmailAddress]
                [RegularExpression("^[a-z]+@example\\.com$")]
                public string? ContactEmail { get; set; }
            }
            """,
            includeAttributes: false,
            [
                new AnnotationConstantsGenerator(),
            ]);

    private static void VerifyGenerated(
        string source,
        bool includeAttributes,
        IIncrementalGenerator[] generators,
        [CallerMemberName] string snapshotName = "",
        [CallerFilePath] string testFilePath = "")
    {
        var compilation = GeneratorTestHarness.CreateCompilation(source, "MyApp");
        var driver = CSharpGeneratorDriver
            .Create(generators)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _, TestContext.Current.CancellationToken);

        GeneratorTestHarness.AssertGeneratedCodeCompiles(compilation, outputCompilation);

        SnapshotAssert.Matches(
            Path.Combine(Path.GetDirectoryName(testFilePath), "Verified", snapshotName),
            driver.GetRunResult(),
            hintName => AttributeHintNames.Contains(hintName) == includeAttributes);
    }
}