# Changelog

## [0.2.0](https://github.com/atc-net/atc-source-generators/compare/v0.1.67...v0.2.0) (2026-11-27)


### New features

* generate the attributes as internal [Embedded] types so referencing projects need no CS0436 suppression ([71326e8](https://github.com/atc-net/atc-source-generators/commit/71326e8da2014d6fcb02a35f6f60d1a715fa797b))
* link every diagnostic to its section in the Diagnostics Reference ([37c8146](https://github.com/atc-net/atc-source-generators/commit/37c8146b502ef1a8bfa0820c6d3a29b4e737b896))
* **Mapping:** apply the same built-in conversions in [MapTo] as in configuration mappings ([408a132](https://github.com/atc-net/atc-source-generators/commit/408a1320f9d8355cb245fa3faf565139255a8312))
* **Mapping:** cast numeric types in [MapTo] mappings ([10de568](https://github.com/atc-net/atc-source-generators/commit/10de56816696874595a1afb262d64a7797bfb9a5))
* **MappingConfiguration:** map nested source paths in MapConfigProperty and PropertyMap ([a999089](https://github.com/atc-net/atc-source-generators/commit/a9990899e238c0fec09e7d3a4897daa18ee99ee9))
* **MappingConfiguration:** use user-implemented conversion methods for property types ([80c6e37](https://github.com/atc-net/atc-source-generators/commit/80c6e37fd8dab98f8f2418d9193dd29160f10ed6))
* **Mapping:** construct positional records in [MapTo] with defaults for unmatched parameters ([a9b8b29](https://github.com/atc-net/atc-source-generators/commit/a9b8b2986638861ae804ca87b54dde465cb785bd))
* **Mapping:** DeepClone = true copies same-type properties instead of sharing them ([5a2bdc7](https://github.com/atc-net/atc-source-generators/commit/5a2bdc72099c0cc813f3b380a380edc345a061f8))
* **Mapping:** map nested source paths with class-level [MapProperty] in [MapTo] mappings ([f90587f](https://github.com/atc-net/atc-source-generators/commit/f90587f59d512b951df90533af34a8dbaa688dba))
* **Mapping:** report [MapTo] nested properties whose type has no mapper (ATCMAP015) ([d72687c](https://github.com/atc-net/atc-source-generators/commit/d72687c9de5f7e9e50cf4a4b8fdd5144ce310b1a))
* **Mapping:** report unmapped target properties (ATCMAP014, ATCMCF016) with opt-in strict mode ([1a78250](https://github.com/atc-net/atc-source-generators/commit/1a782501ea50170a34ca0914dd14705fa47575ab))
* **Mapping:** use conversion methods of the [MapTo] class for property types ([e5113b0](https://github.com/atc-net/atc-source-generators/commit/e5113b0e76bfeccf59f8f2e4deef5bc475f9c24a))


### Upgrades

* **deps:** update analyzers, test packages and sample packages ([73cd315](https://github.com/atc-net/atc-source-generators/commit/73cd315dad8bad4fdb4aa47b0bf6568d31096125))


### Bug fixes

* **AnnotationConstants:** generate constants for partial types and same-name nested types ([9375575](https://github.com/atc-net/atc-source-generators/commit/937557545f3eda32c288fb04e138dfb3354cc00b))
* avoid generated name collisions across namespaces and mappings ([b042467](https://github.com/atc-net/atc-source-generators/commit/b0424678d31c3fa38645330d42b83d8db6d9bfc1))
* **DependencyRegistration:** generated decorator and factory code compiles without implicit usings ([454b073](https://github.com/atc-net/atc-source-generators/commit/454b073d8f4868e60ceff72fe6c7b4d562ad59bf))
* escape user values and keyword identifiers in generated code ([059068a](https://github.com/atc-net/atc-source-generators/commit/059068a8625fa1f520cca8af4eb732a93c0991ed))
* **Mapping:** add using System.Linq to [MapTo] generated code ([63eafc0](https://github.com/atc-net/atc-source-generators/commit/63eafc09e96dbb0d29976a050280788c03f5e0ce))
* **Mapping:** cast a null nullable source to default instead of throwing ([ead9782](https://github.com/atc-net/atc-source-generators/commit/ead9782f6f72c6de912816750b4fbfb71c04e238))
* **MappingConfiguration:** call a nested type's declared mapper when it has a custom name ([681af51](https://github.com/atc-net/atc-source-generators/commit/681af515d2bb5b482dc5b65d28ffe2f5a995e6a4))
* **MappingConfiguration:** convert nullable value types and nullable strings to and from string ([32a7214](https://github.com/atc-net/atc-source-generators/commit/32a7214dccede98b02b7671616e50cf054935adf))
* **MappingConfiguration:** generate AddMappings as internal so referencing projects can call it ([3067d68](https://github.com/atc-net/atc-source-generators/commit/3067d68fc4d3a43080136f4b5fbd3ac673cd3fd1))
* **MappingConfiguration:** generate nested mappers for the reverse direction of bidirectional mappings ([60800ef](https://github.com/atc-net/atc-source-generators/commit/60800ef7cc248b3cc523eeab54670b5f3d9fb7fc))
* **MappingConfiguration:** ignore unrelated Configure calls and parse expression-bodied mappings ([016e544](https://github.com/atc-net/atc-source-generators/commit/016e544e6055741c6cf1ee740d3fee35a14a0723))
* **MappingConfiguration:** keep constructor arguments in place for ignored and renamed properties ([9e89093](https://github.com/atc-net/atc-source-generators/commit/9e89093912f28947092bfa10152d0d714c64ca57))
* **MappingConfiguration:** report invalid mapping methods and Map() arguments; remove unused rule IDs ([cfb0d59](https://github.com/atc-net/atc-source-generators/commit/cfb0d59b1d8f83c9ad3b8fbb573f8684a177c0c8))
* **Mapping:** construct [MapTo] targets from inherited and collection properties ([3a61cbf](https://github.com/atc-net/atc-source-generators/commit/3a61cbfdaefa93a20035f9c7d292dabdbd237a55))
* **Mapping:** convert nullable DateTime and DateTimeOffset in [MapTo] mappings ([197af22](https://github.com/atc-net/atc-source-generators/commit/197af22e6d6ebcd0f9885df1349cb48ada10d9e6))
* **Mapping:** don't flatten a target property that is already mapped ([0773b7f](https://github.com/atc-net/atc-source-generators/commit/0773b7f864a1e922be750889f7a45535f5240df9))
* **Mapping:** flatten value-type members through a nullable reference ([23cb4bc](https://github.com/atc-net/atc-source-generators/commit/23cb4bc28a06f75c7ef24ce3dcb91b9013ce730a))
* **Mapping:** map a null enum or DateTime source to default instead of throwing ([8f4a7b4](https://github.com/atc-net/atc-source-generators/commit/8f4a7b4834a8310e2f73f8ce3925d4b9378e60ed))
* **Mapping:** map a null source collection to a null Collection&lt;T&gt; or ReadOnlyCollection&lt;T&gt; ([38162ca](https://github.com/atc-net/atc-source-generators/commit/38162ca1303fc008c94f242d85f2568c6b805b87))
* **Mapping:** map dictionaries and drop trailing comma in constructor calls ([a3bd18b](https://github.com/atc-net/atc-source-generators/commit/a3bd18b35d75a8d6e68356e387b7bd8caf109d5d))
* **Mapping:** map dictionary values and support dictionaries in [MapTo] ([5a31f49](https://github.com/atc-net/atc-source-generators/commit/5a31f49db516bda0da9a6651895c30f83d280e38))
* **Mapping:** map Nullable enum to a different Nullable enum in [MapTo] ([109911b](https://github.com/atc-net/atc-source-generators/commit/109911ba1fcc0311c200fc8d78c6de74f25390df))
* **Mapping:** match constructor parameters through [MapProperty] renames ([e24357f](https://github.com/atc-net/atc-source-generators/commit/e24357fed293919a9d4c045b26cc3ca719120cab))
* **Mapping:** match constructor parameters with the same type rule as property mappings ([6765436](https://github.com/atc-net/atc-source-generators/commit/6765436d6b37733e839c58ead752f6f96f61d3b0))
* **Mapping:** name the parsed enum variable per property ([8676699](https://github.com/atc-net/atc-source-generators/commit/86766996a2a7b79aa8ae6c9be1c72899dd8ba3df))
* **Mapping:** name the private nested-property variable per property ([fad05c5](https://github.com/atc-net/atc-source-generators/commit/fad05c5f1017702aa1d86a5a190d0c8c77d68a10))
* **Mapping:** read private source members through UnsafeAccessor in every conversion ([959f53f](https://github.com/atc-net/atc-source-generators/commit/959f53fa3c6f3c651297ec2bfa36a3356e05f922))
* **Mapping:** report BeforeMap, AfterMap and Factory methods that can't be called (ATCMAP013) ([37d3afd](https://github.com/atc-net/atc-source-generators/commit/37d3afda9bf0d01a81c8448f0120a9ddeecfb1c3))
* **Mapping:** set private target members through UnsafeAccessor and support bidirectional IncludePrivateMembers ([cdabd62](https://github.com/atc-net/atc-source-generators/commit/cdabd626d492d31a4de87d4078ff3e38171e1a54))
* **Mapping:** support types in the global namespace and qualify generated type names ([bf7bf30](https://github.com/atc-net/atc-source-generators/commit/bf7bf309f7e56ee998d95609b6929b554b6127c2))
* **Mapping:** use UnsafeAccessor for private members of a [MapProperty] source path ([ddd5175](https://github.com/atc-net/atc-source-generators/commit/ddd5175bf4f3fea78cce514f7469113e7e07fe5c))
* **OptionsBinding:** avoid early-access method name clashes for same-named options classes ([fe07127](https://github.com/atc-net/atc-source-generators/commit/fe07127f7ca2abf64626c605674255746fa53527))
* **OptionsBinding:** report private OnChange, PostConfigure and ConfigureAll callbacks ([ffe5b93](https://github.com/atc-net/atc-source-generators/commit/ffe5b931d9786d87fd60ab78b0b3e7e2fbc05602))
* **package:** describe all generators in the package description and tags ([43a3bef](https://github.com/atc-net/atc-source-generators/commit/43a3befa365aa9d7c9b3fa3a02cad43e89ac0e9b))
* **package:** stop shipping the generator DLL as a runtime lib asset ([91d8060](https://github.com/atc-net/atc-source-generators/commit/91d80604953786443db7e857620179a9e6f0008c))


### Performance improvements

* **AnnotationConstants:** make the generator fully incremental ([21f9793](https://github.com/atc-net/atc-source-generators/commit/21f97939a4f3ee81f21718117a22a4587176a350))
* **DependencyRegistration:** cache generation across unrelated edits ([ad2fd15](https://github.com/atc-net/atc-source-generators/commit/ad2fd1555a1a5e208286057f26b55eebbbcd5b15))
* **EnumMapping:** make the generator fully incremental ([e531ed0](https://github.com/atc-net/atc-source-generators/commit/e531ed08e224771ca2b2ac3751762f2b38081170))
* find [MapTo] and [MappingConfiguration] classes with ForAttributeWithMetadataName ([735bf0a](https://github.com/atc-net/atc-source-generators/commit/735bf0a1f0a01465662897118c3c8d86147139da))
* **MappingConfiguration:** collect [MapTo] pairs incrementally instead of scanning the compilation ([add959f](https://github.com/atc-net/atc-source-generators/commit/add959f5b764fdab9c5cbb72b824798ac64e3e28))
* **OptionsBinding:** cache generation across unrelated edits ([93a0a27](https://github.com/atc-net/atc-source-generators/commit/93a0a2796f35e67382856832b027880417e9744f))
* pass cancellation tokens from generator transforms to the semantic model ([c0095b0](https://github.com/atc-net/atc-source-generators/commit/c0095b006c7b40913518b1b37ee94f0774f09879))
* skip framework assemblies when scanning references for transitive registration ([2f2f2ec](https://github.com/atc-net/atc-source-generators/commit/2f2f2ec69f9ae475c25fffb224e7ec609badeb68))

## [0.1.67](https://github.com/atc-net/atc-source-generators/compare/v0.1.66...v0.1.67) (2026-11-26)


### Upgrades

* **deps:** upgrade Microsoft.AspNetCore.OpenApi to 10.0.15 in samples ([61b45ec](https://github.com/atc-net/atc-source-generators/commit/61b45ecec61447f1fcbfcb6276fd20dfd30301ec))


### Bug fixes

* **MappingConfiguration:** call nested mappers by name to avoid ambiguity across mapping classes ([d32cae4](https://github.com/atc-net/atc-source-generators/commit/d32cae45f35fc90e3a74dc47fda965f26faef9fa))
* **Mapping:** map T? &lt;-&gt; T for value types instead of treating BCL structs as nested types ([44dd519](https://github.com/atc-net/atc-source-generators/commit/44dd51922fb364fbc69e2be37eeaefcc07940c17))
