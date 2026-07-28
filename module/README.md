# Herds & Logistics

A campaign-model mod for Mount & Blade II: Bannerlord 1.3.15 through 1.4.7. The same package supports Native, War Sails, and The Old Realms: War in the Mountains.

## Added mechanics

- **Herding Recovery** distinguishes loose livestock, tethered pack animals, active remount strings, and excessive reserve horses. Steward, Scouting, and Riding improve handling capacity.
- **Remount Rotation** uses surplus riding horses as a remount pool, granting up to +4% campaign movement while the party remains within inventory capacity.
- **Balanced Loads** offsets the handled pack train's share of native cargo drag and a capped portion of overload drag.
- **Balanced Panniers** grants 5-15 additional carrying capacity per handled pack animal, scaled by Steward.

## Compatibility architecture

Herds & Logistics has no compile-time or runtime assembly dependency on TOR. At campaign startup it captures the inventory-capacity and party-speed models already registered by the active game profile, then delegates all baseline calculations to those models before applying its own adjustments.

- Native and War Sails retain their active TaleWorlds models.
- TOR retains its TOR-specific speed and capacity models when TOR loads first.
- The package never includes or replaces `TOR_Core.dll`.

## Installation

Delete the existing `Modules/HerdsAndLogistics` folder, then install the new folder from the release archive.

Use separate TOR and War Sails launcher profiles. Do not enable TOR and War Sails together.
