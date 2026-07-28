# Balance reference

The model uses Bannerlord's additive speed-factor system. Values below assume 300 Steward, 300 Scouting, and 300 Riding unless stated otherwise.

| Situation | Result |
|---|---|
| 100 foot troops, 100 riding horses | Native Footmen on horses only; unchanged |
| 100 foot troops, 200 riding horses, under capacity | 100 mounted footmen plus up to +4% Remount Rotation |
| Cargo fully covered by handled pack capacity | Balanced Loads cancels the native Cargo within capacity factor |
| Cargo only partly covered by handled pack capacity | Balanced Loads cancels the same proportion of the native cargo factor |
| Over capacity with handled pack animals | Recovers 25-50% of the pack-covered native overload factor, capped at 12 percentage points |
| Mixed herd with livestock and pack animals | Available handlers are distributed proportionally across the whole train |
| Extreme animal hoard | Herding Recovery remains capped at 25 percentage points |

Balanced Panniers grants 5-15 additional base capacity per handled pack animal. Active TOR/native global capacity factors also apply. The party-speed tooltip reports `Balanced loads (X% covered; panniers +Y)`, where `Y` is the final applied capacity contribution and `X` is the handled pack train's share of the current cargo load.

The Shepherd perk is resolved by Bannerlord against the full native Herding penalty first. Herding Recovery then operates on that resulting penalty at face value and is capped so it can reduce the remaining Herding effect to exactly zero, never turn it into a positive speed bonus.
