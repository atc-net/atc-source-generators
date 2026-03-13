### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ATCMCF001 | MappingConfiguration | Error | Mapping configuration class must be declared as static
ATCMCF002 | MappingConfiguration | Error | Mapping configuration class must be declared as partial
ATCMCF004 | MappingConfiguration | Error | Mapping method must be an extension method
ATCMCF005 | MappingConfiguration | Warning | Ignored property not found on source type
ATCMCF006 | MappingConfiguration | Error | Renamed source property not found
ATCMCF007 | MappingConfiguration | Error | Renamed target property not found
ATCMCF008 | MappingConfiguration | Info | Configuration class has no partial extension methods
ATCMCF010 | MappingConfiguration | Error | Method return type must be a class, record, or struct
ATCMAP005 | ObjectMapping | Warning | Type has both attribute and configuration-based mapping
ATCMAP006 | ObjectMapping | Warning | Type pair is configured multiple times
ATCMAP010 | ObjectMapping | Error | Configuration target type must be a class or struct
ATCENUM004 | EnumMapping | Warning | Auto-detected enum mapping has partial match
ATCENUM005 | EnumMapping | Warning | Enum types have no matching values
ATCMCF011 | MappingConfiguration | Error | Map() requires exactly two type arguments
ATCMCF013 | MappingConfiguration | Error | AddMappings() requires a lambda expression argument
ATCENUM006 | EnumMapping | Warning | Flag enum mapping may produce incorrect results for combined values
ATCMAP011 | ObjectMapping | Warning | Collection element type has no mapping method
