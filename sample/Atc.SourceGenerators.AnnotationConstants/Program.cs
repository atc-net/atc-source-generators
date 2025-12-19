Console.WriteLine("=== AnnotationConstantsGenerator Demo ===");
Console.WriteLine();

// Access Product annotation constants
Console.WriteLine("--- Product Annotations ---");
Console.WriteLine();

Console.WriteLine("Product.Name:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Name.DisplayName}");
Console.WriteLine($"  Description: {AnnotationConstants.Product.Name.Description}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Product.Name.IsRequired}");
Console.WriteLine($"  MinimumLength: {AnnotationConstants.Product.Name.MinimumLength}");
Console.WriteLine($"  MaximumLength: {AnnotationConstants.Product.Name.MaximumLength}");
Console.WriteLine($"  RequiredErrorMessage: {AnnotationConstants.Product.Name.RequiredErrorMessage}");
Console.WriteLine($"  StringLengthErrorMessage: {AnnotationConstants.Product.Name.StringLengthErrorMessage}");
Console.WriteLine();

Console.WriteLine("Product.Price:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Price.DisplayName}");
Console.WriteLine($"  Description: {AnnotationConstants.Product.Price.Description}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Product.Price.IsRequired}");
Console.WriteLine($"  Minimum: {AnnotationConstants.Product.Price.Minimum}");
Console.WriteLine($"  Maximum: {AnnotationConstants.Product.Price.Maximum}");
Console.WriteLine($"  OperandType: {AnnotationConstants.Product.Price.OperandType}");
Console.WriteLine($"  RangeErrorMessage: {AnnotationConstants.Product.Price.RangeErrorMessage}");
Console.WriteLine();

Console.WriteLine("Product.Sku:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Sku.DisplayName}");
Console.WriteLine($"  ShortName: {AnnotationConstants.Product.Sku.ShortName}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Product.Sku.IsRequired}");
Console.WriteLine($"  Pattern: {AnnotationConstants.Product.Sku.Pattern}");
Console.WriteLine($"  RegularExpressionErrorMessage: {AnnotationConstants.Product.Sku.RegularExpressionErrorMessage}");
Console.WriteLine();

// Access Customer annotation constants
Console.WriteLine("--- Customer Annotations ---");
Console.WriteLine();

Console.WriteLine("Customer.Id:");
Console.WriteLine($"  IsKey: {AnnotationConstants.Customer.Id.IsKey}");
Console.WriteLine($"  IsEditable: {AnnotationConstants.Customer.Id.IsEditable}");
Console.WriteLine();

Console.WriteLine("Customer.Email:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.Email.DisplayName}");
Console.WriteLine($"  Order: {AnnotationConstants.Customer.Email.Order}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Customer.Email.IsRequired}");
Console.WriteLine($"  IsEmailAddress: {AnnotationConstants.Customer.Email.IsEmailAddress}");
Console.WriteLine();

Console.WriteLine("Customer.PhoneNumber:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.PhoneNumber.DisplayName}");
Console.WriteLine($"  IsPhone: {AnnotationConstants.Customer.PhoneNumber.IsPhone}");
Console.WriteLine();

Console.WriteLine("Customer.Website:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.Website.DisplayName}");
Console.WriteLine($"  IsUrl: {AnnotationConstants.Customer.Website.IsUrl}");
Console.WriteLine();

Console.WriteLine("Customer.CreditCardNumber:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.CreditCardNumber.DisplayName}");
Console.WriteLine($"  IsCreditCard: {AnnotationConstants.Customer.CreditCardNumber.IsCreditCard}");
Console.WriteLine();

Console.WriteLine("Customer.RowVersion:");
Console.WriteLine($"  IsTimestamp: {AnnotationConstants.Customer.RowVersion.IsTimestamp}");
Console.WriteLine($"  IsScaffoldColumn: {AnnotationConstants.Customer.RowVersion.IsScaffoldColumn}");
Console.WriteLine();

// Access PasswordReset annotation constants
Console.WriteLine("--- PasswordReset Annotations ---");
Console.WriteLine();

Console.WriteLine("PasswordReset.ConfirmPassword:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.PasswordReset.ConfirmPassword.DisplayName}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.PasswordReset.ConfirmPassword.IsRequired}");
Console.WriteLine($"  CompareProperty: {AnnotationConstants.PasswordReset.ConfirmPassword.CompareProperty}");
Console.WriteLine();

// Access NetworkConfig annotation constants (Atc attributes)
Console.WriteLine("--- NetworkConfig Annotations (Atc Package) ---");
Console.WriteLine();

Console.WriteLine("NetworkConfig.ServerAddress:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.ServerAddress.DisplayName}");
Console.WriteLine($"  IsIPAddress: {AnnotationConstants.NetworkConfig.ServerAddress.IsIPAddress}");
Console.WriteLine($"  IPAddressRequired: {AnnotationConstants.NetworkConfig.ServerAddress.IPAddressRequired}");
Console.WriteLine();

Console.WriteLine("NetworkConfig.ApiEndpoint:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.ApiEndpoint.DisplayName}");
Console.WriteLine($"  IsAtcUri: {AnnotationConstants.NetworkConfig.ApiEndpoint.IsAtcUri}");
Console.WriteLine($"  AtcUriRequired: {AnnotationConstants.NetworkConfig.ApiEndpoint.AtcUriRequired}");
Console.WriteLine($"  AtcUriAllowHttp: {AnnotationConstants.NetworkConfig.ApiEndpoint.AtcUriAllowHttp}");
Console.WriteLine($"  AtcUriAllowHttps: {AnnotationConstants.NetworkConfig.ApiEndpoint.AtcUriAllowHttps}");
Console.WriteLine();

Console.WriteLine("NetworkConfig.CurrencyCode:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.CurrencyCode.DisplayName}");
Console.WriteLine($"  IsIsoCurrencySymbol: {AnnotationConstants.NetworkConfig.CurrencyCode.IsIsoCurrencySymbol}");
Console.WriteLine($"  IsoCurrencySymbolRequired: {AnnotationConstants.NetworkConfig.CurrencyCode.IsoCurrencySymbolRequired}");
Console.WriteLine();

Console.WriteLine("NetworkConfig.AllowedCurrency:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.AllowedCurrency.DisplayName}");
Console.WriteLine($"  IsIsoCurrencySymbol: {AnnotationConstants.NetworkConfig.AllowedCurrency.IsIsoCurrencySymbol}");
Console.WriteLine($"  AllowedIsoCurrencySymbols: [{string.Join(", ", AnnotationConstants.NetworkConfig.AllowedCurrency.AllowedIsoCurrencySymbols)}]");
Console.WriteLine();

Console.WriteLine("NetworkConfig.ServiceKey:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.ServiceKey.DisplayName}");
Console.WriteLine($"  IsAtcString: {AnnotationConstants.NetworkConfig.ServiceKey.IsAtcString}");
Console.WriteLine($"  IsKeyString: {AnnotationConstants.NetworkConfig.ServiceKey.IsKeyString}");
Console.WriteLine();

Console.WriteLine("NetworkConfig.Identifier:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.NetworkConfig.Identifier.DisplayName}");
Console.WriteLine($"  IsAtcString: {AnnotationConstants.NetworkConfig.Identifier.IsAtcString}");
Console.WriteLine($"  AtcStringRequired: {AnnotationConstants.NetworkConfig.Identifier.AtcStringRequired}");
Console.WriteLine($"  AtcStringMinLength: {AnnotationConstants.NetworkConfig.Identifier.AtcStringMinLength}");
Console.WriteLine($"  AtcStringMaxLength: {AnnotationConstants.NetworkConfig.Identifier.AtcStringMaxLength}");
Console.WriteLine($"  AtcStringInvalidCharacters: [{string.Join(", ", AnnotationConstants.NetworkConfig.Identifier.AtcStringInvalidCharacters.Select(c => $"'{c}'"))}]");
Console.WriteLine($"  AtcStringInvalidPrefixStrings: [{string.Join(", ", AnnotationConstants.NetworkConfig.Identifier.AtcStringInvalidPrefixStrings.Select(s => $"\"{s}\""))}]");
Console.WriteLine();

Console.WriteLine("=== Demo Complete ===");
Console.WriteLine();
Console.WriteLine("Benefits of AnnotationConstantsGenerator:");
Console.WriteLine("  - Zero reflection at runtime");
Console.WriteLine("  - Compile-time access to annotation metadata");
Console.WriteLine("  - IntelliSense support for all constants");
Console.WriteLine("  - Native AOT compatible");
Console.WriteLine("  - Supports both Microsoft DataAnnotations and Atc attributes");