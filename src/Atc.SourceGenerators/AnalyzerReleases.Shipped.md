; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ATCDIR001 | DependencyInjection | Error | Service 'As' type must be an interface
ATCDIR002 | DependencyInjection | Error | Class does not implement specified interface
ATCDIR003 | DependencyInjection | Warning | Duplicate service registration with different lifetime
ATCDIR004 | DependencyInjection | Error | Hosted services must use Singleton lifetime
ATCDIR005 | DependencyInjection | Error | Factory method not found
ATCDIR006 | DependencyInjection | Error | Factory method has invalid signature
ATCDIR007 | DependencyInjection | Error | Instance member not found
ATCDIR008 | DependencyInjection | Error | Instance member must be static
ATCDIR009 | DependencyInjection | Error | Instance and Factory are mutually exclusive
ATCDIR010 | DependencyInjection | Error | Instance registration requires Singleton lifetime
ATCOPT001 | OptionsBinding | Error | Options class must be partial
ATCOPT002 | OptionsBinding | Error | Section name cannot be null or empty
ATCOPT003 | OptionsBinding | Error | Const section name cannot be null or empty
ATCOPT004 | OptionsBinding | Error | OnChange requires Monitor lifetime
ATCOPT005 | OptionsBinding | Error | OnChange not supported with named options
ATCOPT006 | OptionsBinding | Error | OnChange callback method not found
ATCOPT007 | OptionsBinding | Error | OnChange callback has invalid signature
ATCMAP001 | ObjectMapping | Error | Mapping class must be partial
ATCMAP002 | ObjectMapping | Error | Target type must be a class or struct
ATCMAP003 | ObjectMapping | Error | MapProperty target property not found
ATCMAP004 | ObjectMapping | Warning | Required property on target type has no mapping
ATCENUM001 | EnumMapping | Error | Target type must be an enum
ATCENUM002 | EnumMapping | Warning | Source enum value has no matching target value