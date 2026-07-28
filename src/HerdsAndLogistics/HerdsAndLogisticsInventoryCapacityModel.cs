using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace HerdsAndLogistics;

public sealed class HerdsAndLogisticsInventoryCapacityModel : InventoryCapacityModel
{
    private static readonly TextObject BalancedPanniersText = new("{=HAL_balanced_panniers}Balanced panniers");
    private readonly InventoryCapacityModel _baseModel;

    internal HerdsAndLogisticsInventoryCapacityModel(InventoryCapacityModel baseModel)
    {
        _baseModel = baseModel ?? throw new ArgumentNullException(nameof(baseModel));
    }

    public override ExplainedNumber CalculateInventoryCapacity(
        MobileParty mobileParty,
        bool isCurrentlyAtSea,
        bool includeDescriptions = false,
        int additionalTroops = 0,
        int additionalSpareMounts = 0,
        int additionalPackAnimals = 0,
        bool includeFollowers = false)
    {
        ExplainedNumber result = _baseModel.CalculateInventoryCapacity(
            mobileParty,
            isCurrentlyAtSea,
            includeDescriptions,
            additionalTroops,
            additionalSpareMounts,
            additionalPackAnimals,
            includeFollowers);

        if (mobileParty is null || isCurrentlyAtSea || mobileParty.IsVillager)
        {
            return result;
        }

        AnimalTrainStats stats = AnimalTrainStats.GatherForInventoryCapacity(
            mobileParty,
            includeFollowers,
            additionalTroops,
            additionalSpareMounts,
            additionalPackAnimals);
        float pannierBonus = AnimalTrainStats.CalculateBalancedPannierBonus(mobileParty, stats);
        if (pannierBonus > 0.01f)
        {
            result.Add(pannierBonus, BalancedPanniersText);
        }

        return result;
    }

    public override ExplainedNumber CalculateTotalWeightCarried(
        MobileParty mobileParty,
        bool isCurrentlyAtSea,
        bool includeDescriptions = false)
    {
        return _baseModel.CalculateTotalWeightCarried(mobileParty, isCurrentlyAtSea, includeDescriptions);
    }

    public override int GetItemAverageWeight()
    {
        return _baseModel.GetItemAverageWeight();
    }

    public override float GetItemEffectiveWeight(
        EquipmentElement equipmentElement,
        MobileParty mobileParty,
        bool isCurrentlyAtSea,
        out TextObject description)
    {
        return _baseModel.GetItemEffectiveWeight(
            equipmentElement,
            mobileParty,
            isCurrentlyAtSea,
            out description);
    }
}
