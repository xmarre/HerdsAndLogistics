using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace HerdsAndLogistics;

public sealed class HerdsAndLogisticsPartySpeedModel : PartySpeedModel
{
    private static readonly TextObject HerdingRecoveryText = new("{=HAL_organized_animal_train}Herding recovery");
    private static readonly TextObject RemountRotationText = new("{=HAL_remount_rotation}Remount rotation");
    private readonly PartySpeedModel _baseModel;

    internal HerdsAndLogisticsPartySpeedModel(PartySpeedModel baseModel)
    {
        _baseModel = baseModel ?? throw new ArgumentNullException(nameof(baseModel));
    }

    public override float BaseSpeed => _baseModel.BaseSpeed;
    public override float MinimumSpeed => _baseModel.MinimumSpeed;

    public override ExplainedNumber CalculateBaseSpeed(
        MobileParty mobileParty,
        bool includeDescriptions = false,
        int additionalTroopOnFootCount = 0,
        int additionalTroopOnHorseCount = 0)
    {
        ExplainedNumber result = _baseModel.CalculateBaseSpeed(
            mobileParty,
            includeDescriptions,
            additionalTroopOnFootCount,
            additionalTroopOnHorseCount);

        if (mobileParty is null || mobileParty.IsCurrentlyAtSea || mobileParty.IsVillager)
        {
            return result;
        }

        AnimalTrainStats stats = AnimalTrainStats.GatherForSpeed(
            mobileParty,
            includeFollowers: true,
            additionalTroopOnFootCount,
            additionalTroopOnHorseCount);
        LogisticsCapacityMetrics capacityMetrics = AnimalTrainStats.GetCombinedCapacityMetrics(
            mobileParty,
            includeFollowers: true,
            additionalTroopOnFootCount,
            additionalTroopOnHorseCount);

        AddOrganizedHerdAdjustment(mobileParty, stats, ref result);
        AddRemountRotation(mobileParty, stats, capacityMetrics.TotalCapacity, ref result);
        AddBalancedLoadAdjustment(mobileParty, stats, capacityMetrics, ref result);
        return result;
    }

    public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
    {
        return _baseModel.CalculateFinalSpeed(mobileParty, finalSpeed);
    }

    private static void AddOrganizedHerdAdjustment(
        MobileParty party,
        AnimalTrainStats stats,
        ref ExplainedNumber result)
    {
        float recovery = AnimalTrainStats.CalculateHerdRecoveryFactor(party, stats);
        if (recovery > 0.0001f)
        {
            result.AddFactor(recovery, HerdingRecoveryText);
        }
    }

    private static void AddRemountRotation(
        MobileParty party,
        AnimalTrainStats stats,
        float combinedCapacity,
        ref ExplainedNumber result)
    {
        if (stats.Mounts <= 0 || stats.Men <= 0 || stats.WeightCarried > combinedCapacity)
        {
            return;
        }

        int surplusRidingAnimals = stats.SurplusRidingAnimals;
        int riders = stats.Riders;
        if (surplusRidingAnimals <= 0 || riders <= 0)
        {
            return;
        }

        float reserveRatio = AnimalTrainStats.Clamp((float)surplusRidingAnimals / riders, 0f, 1f);
        float ridingScale = 0.5f + 0.5f * AnimalTrainStats.GetRidingSkill(party);
        float factor = 0.04f * reserveRatio * ridingScale;
        if (factor > 0.0001f)
        {
            result.AddFactor(factor, RemountRotationText);
        }
    }

    private static void AddBalancedLoadAdjustment(
        MobileParty party,
        AnimalTrainStats stats,
        LogisticsCapacityMetrics capacityMetrics,
        ref ExplainedNumber result)
    {
        float totalCapacity = capacityMetrics.TotalCapacity;
        if (stats.PackAnimals <= 0 || stats.WeightCarried <= 0f || totalCapacity <= 0f)
        {
            return;
        }

        float handledPackAnimals = AnimalTrainStats.CalculateHandledPackAnimals(party, stats);
        if (handledPackAnimals <= 0f)
        {
            return;
        }

        int inventoryCapacity = Math.Max(1, (int)totalCapacity);
        float handledPackShare = AnimalTrainStats.CalculateHandledPackShare(party, stats);
        float pannierCapacity = capacityMetrics.PannierCapacity;
        float handledCapacity = capacityMetrics.NativePackCapacity * handledPackShare + pannierCapacity;
        float weightWithinCapacity = Math.Min(stats.WeightCarried, inventoryCapacity);
        float nativeCargoPenaltyMagnitude = 0.02f * weightWithinCapacity / inventoryCapacity;
        float coveredCargoShare = AnimalTrainStats.Clamp(
            handledCapacity / Math.Max(1f, weightWithinCapacity),
            0f,
            1f);
        float cargoRecovery = nativeCargoPenaltyMagnitude * coveredCargoShare;

        float overloadWeight = Math.Max(0f, stats.WeightCarried - inventoryCapacity);
        float overloadRecovery = 0f;
        if (overloadWeight > 0f)
        {
            float nativeOverloadPenalty = CalculateNativeOverloadPenaltyMagnitude(
                party,
                overloadWeight,
                inventoryCapacity);
            float packCoveredOverloadShare = AnimalTrainStats.Clamp(
                handledCapacity / overloadWeight,
                0f,
                1f);
            float stewardScale = 0.25f + 0.25f * AnimalTrainStats.GetStewardSkill(party);
            overloadRecovery = Math.Min(
                0.12f,
                nativeOverloadPenalty * packCoveredOverloadShare * stewardScale);
        }

        float totalRecovery = cargoRecovery + overloadRecovery;
        if (totalRecovery <= 0.0001f)
        {
            return;
        }

        TextObject explanation = new(
            "{=HAL_balanced_loads_with_panniers}Balanced loads ({COVERAGE}% covered; panniers +{CAPACITY})");
        explanation.SetTextVariable("COVERAGE", (int)Math.Round(coveredCargoShare * 100f));
        explanation.SetTextVariable("CAPACITY", (int)Math.Round(pannierCapacity));
        result.AddFactor(totalRecovery, explanation);
    }

    private static float CalculateNativeOverloadPenaltyMagnitude(
        MobileParty party,
        float overloadWeight,
        int inventoryCapacity)
    {
        ExplainedNumber result = new(-0.4f * overloadWeight / inventoryCapacity);

        PerkObject energetic = DefaultPerks.Athletics.Energetic;
        if (energetic is not null)
        {
            PerkHelper.AddPerkBonusForParty(energetic, party, true, ref result, false);
        }

        PerkObject unburdened = DefaultPerks.Scouting.Unburdened;
        if (unburdened is not null)
        {
            PerkHelper.AddPerkBonusForParty(unburdened, party, true, ref result, false);
        }

        return Math.Max(0f, -result.ResultNumber);
    }
}
