# Contributing

Contributions are welcome: bug reports, feature requests and pull requests.

## Prerequisites

- .NET 10 SDK (the version is pinned in `global.json`).

## Build and test

CI builds in Release with warnings treated as errors, so build that way before you open a pull request:

```bash
dotnet build Atc.SourceGenerators.slnx -c Release
```

The tests use xUnit v3 on Microsoft.Testing.Platform:

```bash
# All unit tests (what CI runs)
dotnet test --solution Atc.SourceGenerators.slnx -c Release --filter-query "/[category!=integration]"

# A single test or a group of tests
dotnet test --project test/Atc.SourceGenerators.Tests/Atc.SourceGenerators.Tests.csproj --filter-method "*Nested_Source_Path*"
```

## Writing generator tests

- Every generator test compiles the generated code, and fails on compiler errors in it. Use the existing `GetGeneratedOutput` helpers, which go through `GeneratorTestHarness`.
- For a bug fix, add a test that fails before the fix.
- `Snapshots/SnapshotTests` compare the complete generated output for one representative input per generator with the files in `Snapshots/Verified/`. When a change to the generated code is intended, rerun the tests with the environment variable `ATC_SG_ACCEPT_SNAPSHOTS=1` and review the snapshot diff in the pull request.
- New or changed diagnostics need an entry in `src/Atc.SourceGenerators/AnalyzerReleases.Unshipped.md` and a section in the [Diagnostics Reference](https://github.com/atc-net/atc-source-generators/wiki/Diagnostics-Reference).

## Commit messages

Releases, version numbers and the changelog are produced by [release-please](https://github.com/googleapis/release-please) from the commit messages, so use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat(ObjectMapping): ...` for a new feature (minor version)
- `fix(OptionsBinding): ...` for a bug fix (patch version)
- `docs: ...`, `test: ...`, `chore: ...`, `refactor: ...` for everything else

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
