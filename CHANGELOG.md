# Changelog

## [0.1.67](https://github.com/atc-net/atc-source-generators/compare/v0.1.66...v0.1.67) (2026-11-26)


### Upgrades

* **deps:** upgrade Microsoft.AspNetCore.OpenApi to 10.0.15 in samples ([61b45ec](https://github.com/atc-net/atc-source-generators/commit/61b45ecec61447f1fcbfcb6276fd20dfd30301ec))


### Bug fixes

* **MappingConfiguration:** call nested mappers by name to avoid ambiguity across mapping classes ([d32cae4](https://github.com/atc-net/atc-source-generators/commit/d32cae45f35fc90e3a74dc47fda965f26faef9fa))
* **Mapping:** map T? &lt;-&gt; T for value types instead of treating BCL structs as nested types ([44dd519](https://github.com/atc-net/atc-source-generators/commit/44dd51922fb364fbc69e2be37eeaefcc07940c17))
