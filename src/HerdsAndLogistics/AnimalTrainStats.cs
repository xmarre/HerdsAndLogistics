using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace HerdsAndLogistics;

internal struct AnimalTrainStats
{
    internal int Men;
    internal int MountedMen;
    internal int Footmen;
    internal int Mounts;
    internal int PackAnimals;
    internal int Livestock;
    internal float WeightCarried;

    internal int MountedFootmen => Math.Min(Footmen, Mounts);
    internal int SurplusRidingAnimals => Math.Max(0, Mounts - MountedFootmen);
    internal int Riders => MountedMen + MountedFootmen;
    internal float VanillaHerdSize => PackAnimals + Livestock + SurplusRidingAnimals;

    internal static AnimalTrainStats GatherForSpeed(
        MobileParty party,
        bool includeFollowers,
        int additionalFootmen,
        int additionalMountedMen)
    {
        AnimalTrainStats result = GatherCurrentParties(party, includeFollowers);
        result.Men += additionalFootmen + additionalMountedMen;
        result.Footmen += additionalFootmen;
        result.MountedMen += additionalMountedMen;
        return result;
    }

    internal static AnimalTrainStats GatherForInventoryCapacity(
        MobileParty party,
        bool includeFollowers,
        int additionalTroops,
        int additionalSpareMounts,
        int additionalPackAnimals)
    {
        AnimalTrainStats result = GatherCurrentParties(party, includeFollowers);
        result.Men += additionalTroops;
        result.Mounts += additionalSpareMounts;
        result.PackAnimals += additionalPackAnimals;
        return result;
    }

    private static AnimalTrainStats GatherCurrentParties(MobileParty party, bool includeFollowers)
    {
        AnimalTrainStats stats = default;
        AddParty(ref stats, party);
        if (party is not null && includeFollowers)
        {
            foreach (MobileParty attachedParty in party.AttachedParties)
            {
                AddParty(ref stats, attachedParty);
            }
        }

        return stats;
    }

    internal static LogisticsCapacityMetrics GetCombinedCapacityMetrics(
        MobileParty party,
        bool includeFollowers,
        int additionalFootmen,
        int additionalMountedMen)
    {
        LogisticsCapacityMetrics metrics = default;
        if (party is null)
        {
            return metrics;
        }

        AddPartyCapacityMetrics(ref metrics, party, additionalFootmen, additionalMountedMen);
        if (includeFollowers)
        {
            foreach (MobileParty attachedParty in party.AttachedParties)
            {
                AddPartyCapacityMetrics(ref metrics, attachedParty, 0, 0);
            }
        }

        return metrics;
    }

    private static void AddPartyCapacityMetrics(
        ref LogisticsCapacityMetrics metrics,
        MobileParty party,
        int additionalTroops,
        int additionalSpareMounts)
    {
        if (party is null || party.Party is null)
        {
            return;
        }

        ExplainedNumber capacity;
        if (Campaign.Current?.Models?.InventoryCapacityModel is { } capacityModel)
        {
            capacity = capacityModel.CalculateInventoryCapacity(
                party,
                party.IsCurrentlyAtSea,
                includeDescriptions: false,
                additionalTroops,
                additionalSpareMounts,
                additionalPackAnimals: 0,
                includeFollowers: false);
        }
        else
        {
            capacity = new ExplainedNumber(party.InventoryCapacity);
        }

        metrics.TotalCapacity += capacity.ResultNumber;
        float globalFactor = Math.Max(0f, 1f + capacity.SumOfFactors);
        metrics.NativePackCapacity += CalculatePartyNativePackCapacityBeforeGlobalFactors(party) * globalFactor;

        AnimalTrainStats stats = GatherForInventoryCapacity(
            party,
            includeFollowers: false,
            additionalTroops,
            additionalSpareMounts,
            additionalPackAnimals: 0);
        metrics.PannierCapacity += CalculateBalancedPannierBonus(party, stats) * globalFactor;
    }

    private static void AddParty(ref AnimalTrainStats stats, MobileParty party)
    {
        if (party is null || party.Party is null)
        {
            return;
        }

        stats.Men += party.MemberRoster.TotalManCount;
        stats.MountedMen += party.Party.NumberOfMenWithHorse;
        stats.Footmen += party.Party.NumberOfMenWithoutHorse;
        stats.Mounts += party.ItemRoster.NumberOfMounts;
        stats.PackAnimals += party.ItemRoster.NumberOfPackAnimals;
        stats.Livestock += party.ItemRoster.NumberOfLivestockAnimals;
        stats.WeightCarried += party.TotalWeightCarried;
    }

    internal static float GetHandlerSkillBonus(MobileParty party)
    {
        float steward = NormalizedSkill(GetSkill(party.EffectiveQuartermaster, DefaultSkills.Steward));
        float scouting = NormalizedSkill(GetSkill(party.EffectiveScout, DefaultSkills.Scouting));
        float riding = NormalizedSkill(GetSkill(party.LeaderHero, DefaultSkills.Riding));
        return 0.18f * steward + 0.1f * scouting + 0.07f * riding;
    }

    internal static float GetStewardSkill(MobileParty party)
    {
        return NormalizedSkill(GetSkill(party.EffectiveQuartermaster, DefaultSkills.Steward));
    }

    internal static float GetRidingSkill(MobileParty party)
    {
        return NormalizedSkill(GetSkill(party.LeaderHero, DefaultSkills.Riding));
    }

    private static int GetSkill(Hero? hero, SkillObject skill)
    {
        return hero is not null && skill is not null ? hero.GetSkillValue(skill) : 0;
    }

    private static float NormalizedSkill(int value)
    {
        return Clamp(value / 300f, 0f, 1f);
    }

    internal static float GetPackAnimalHandlingWeight(AnimalTrainStats stats)
    {
        if (stats.PackAnimals <= 0)
        {
            return 0.8f;
        }

        float nominalCapacity = stats.PackAnimals * 100f;
        float loadRatio = Clamp(stats.WeightCarried / Math.Max(1f, nominalCapacity), 0f, 1f);
        return Lerp(0.8f, 0.65f, loadRatio);
    }

    internal static float CalculateVanillaHerdPenalty(float men, float herdAnimals)
    {
        float excessAnimals = herdAnimals - men;
        if (excessAnimals <= 0f)
        {
            return 0f;
        }

        if (men <= 0f)
        {
            return -0.8f;
        }

        return Math.Max(-0.8f, -0.3f * excessAnimals / men);
    }

    internal static float CalculateOrganizedHerdPenalty(MobileParty party, AnimalTrainStats stats)
    {
        float handlerCapacity = stats.Men * (1f + GetHandlerSkillBonus(party));
        int activeRemounts = Math.Min(stats.SurplusRidingAnimals, stats.Riders);
        int excessReserveHorses = Math.Max(0, stats.SurplusRidingAnimals - activeRemounts);
        float reserveHorseWeight = activeRemounts * 0.7f + excessReserveHorses;
        float handledHerdSize = stats.Livestock
            + stats.PackAnimals * GetPackAnimalHandlingWeight(stats)
            + reserveHorseWeight;
        return CalculateVanillaHerdPenalty(handlerCapacity, handledHerdSize);
    }

    internal static float CalculateHerdRecoveryFactor(MobileParty party, AnimalTrainStats stats)
    {
        float nativePenalty = CalculateVanillaHerdPenalty(stats.Men, stats.VanillaHerdSize);
        float organizedPenalty = CalculateOrganizedHerdPenalty(party, stats);
        float recovery = Math.Max(0f, organizedPenalty - nativePenalty);

        PerkObject shepherd = DefaultPerks.Riding.Shepherd;
        if (party is not null && shepherd is not null && party.HasPerk(shepherd, false))
        {
            float remainingNativePenalty = -nativePenalty;
            recovery = Math.Min((shepherd.PrimaryBonus + 1f) * remainingNativePenalty, recovery);
        }

        return Math.Min(0.25f, recovery);
    }

    internal static float CalculateHandledPackAnimals(MobileParty party, AnimalTrainStats stats)
    {
        if (stats.PackAnimals <= 0 || stats.Men <= 0)
        {
            return 0f;
        }

        float handlerCapacity = stats.Men * (1f + GetHandlerSkillBonus(party));
        int activeRemounts = Math.Min(stats.SurplusRidingAnimals, stats.Riders);
        int excessReserveHorses = Math.Max(0, stats.SurplusRidingAnimals - activeRemounts);
        float reserveHorseWeight = activeRemounts * 0.7f + excessReserveHorses;
        float packAnimalWeight = stats.PackAnimals * GetPackAnimalHandlingWeight(stats);
        float totalHandlingDemand = stats.Livestock + reserveHorseWeight + packAnimalWeight;
        if (totalHandlingDemand <= 0f)
        {
            return stats.PackAnimals;
        }

        float handledShare = Clamp(handlerCapacity / totalHandlingDemand, 0f, 1f);
        return stats.PackAnimals * handledShare;
    }

    internal static float CalculateBalancedPannierBonus(MobileParty party, AnimalTrainStats stats)
    {
        float handledPackAnimals = CalculateHandledPackAnimals(party, stats);
        if (handledPackAnimals <= 0f)
        {
            return 0f;
        }

        float capacityPerPackAnimal = 5f + 10f * GetStewardSkill(party);
        return handledPackAnimals * capacityPerPackAnimal;
    }

    internal static float CalculateHandledPackShare(MobileParty party, AnimalTrainStats stats)
    {
        if (stats.PackAnimals <= 0)
        {
            return 0f;
        }

        return Clamp(CalculateHandledPackAnimals(party, stats) / stats.PackAnimals, 0f, 1f);
    }

    private static float CalculatePartyNativePackCapacityBeforeGlobalFactors(MobileParty party)
    {
        if (party is null || party.Party is null || party.IsCurrentlyAtSea)
        {
            return 0f;
        }

        int packAnimals = party.Party.NumberOfPackAnimals;
        if (packAnimals <= 0)
        {
            return 0f;
        }

        ExplainedNumber result = new(packAnimals * 100f);
        PerkObject beastWhisperer = DefaultPerks.Scouting.BeastWhisperer;
        if (beastWhisperer is not null && party.HasPerk(beastWhisperer, true))
        {
            result.AddFactor(beastWhisperer.SecondaryBonus);
        }

        PerkObject deeperSacks = DefaultPerks.Riding.DeeperSacks;
        if (deeperSacks is not null && party.HasPerk(deeperSacks, false))
        {
            result.AddFactor(deeperSacks.PrimaryBonus);
        }

        PerkObject arenicosMules = DefaultPerks.Steward.ArenicosMules;
        if (arenicosMules is not null && party.HasPerk(arenicosMules, false))
        {
            result.AddFactor(arenicosMules.PrimaryBonus);
        }

        return result.ResultNumber;
    }

    internal static float Clamp(float value, float minimum, float maximum)
    {
        return Math.Max(minimum, Math.Min(maximum, value));
    }

    private static float Lerp(float from, float to, float amount)
    {
        return from + (to - from) * amount;
    }
}
