# Reconstruction record

The repository was initialized from the only available compiled v1.0.8 release.

- Uploaded archive SHA-256: `12c4a4c89441b4ee32e47c1550efcadd9a5bb2f701256a153d8a01bb18e5c068`
- Gameplay DLL SHA-256: `94335ed2a738169d534c86290c175d07760372c379bc5e2dacea3ce537d5491b`
- Bundled shim SHA-256: `c1b34b2c3c80c90b0e770dadc8dc6773a80742d8c9c822010e14794817e7a9c2`

The gameplay formulas and public behavior were reconstructed with ILSpy 9.1.0.7988. The universal-loader mechanism was replaced rather than preserved because v1.0.8 shipped a second assembly named `TOR_Core.dll` and compiled the gameplay DLL directly against that identity.

The corrected invariant is:

> The Herds & Logistics package contains only assemblies owned by Herds & Logistics and delegates baseline calculations to the active campaign models.
