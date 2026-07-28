# Herds & Logistics

Canonical source and build repository for the Bannerlord mod **Herds & Logistics**.

## Supported profiles

- Bannerlord 1.3.15
- Bannerlord 1.4.7
- Native
- War Sails
- The Old Realms, loaded before Herds & Logistics

## Build

```bash
dotnet restore src/HerdsAndLogistics/HerdsAndLogistics.csproj
dotnet build src/HerdsAndLogistics/HerdsAndLogistics.csproj -c Release
```

The default build uses `Bannerlord.ReferenceAssemblies` 1.3.15.110062. Compile against another supported API surface with:

```bash
dotnet build src/HerdsAndLogistics/HerdsAndLogistics.csproj -c Release \
  -p:BannerlordReferenceVersion=1.4.7.117484
```

GitHub Actions compiles against both supported endpoints and packages the lowest-version build as the universal release archive.

## Source provenance

The initial source was reconstructed from the v1.0.8 release binary with SHA-256 `94335ed2a738169d534c86290c175d07760372c379bc5e2dacea3ce537d5491b`. The original uploaded archive SHA-256 is `12c4a4c89441b4ee32e47c1550efcadd9a5bb2f701256a153d8a01bb18e5c068`.
