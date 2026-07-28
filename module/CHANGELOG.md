# Changelog

## 1.0.9

- Fixed startup crashes on Bannerlord 1.3.15 and 1.4.7 by removing the bundled replacement `TOR_Core.dll`.
- Removed the hard assembly reference to `TOR_Core`; the mod now wraps the active Bannerlord inventory-capacity and party-speed models.
- Preserved Native, War Sails, and TOR calculations by delegating to the model already registered before Herds & Logistics.
- Added reproducible source builds and compatibility compilation for Bannerlord 1.3.15 and 1.4.7.

## 1.0.8

- Attempted a universal Native, War Sails, and TOR package.

## 1.0.7

- Fixed Shepherd interaction with Herding Recovery.
- Capped Herding Recovery by the remaining native Herding penalty.
