using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace HerdsAndLogistics;

public sealed class HerdsAndLogisticsSubModule : MBSubModuleBase
{
    protected override void OnGameStart(Game game, IGameStarter gameStarter)
    {
        base.OnGameStart(game, gameStarter);

        if (gameStarter is not CampaignGameStarter campaignStarter)
        {
            return;
        }

        InventoryCapacityModel? inventoryCapacityModel = campaignStarter.Models
            .OfType<InventoryCapacityModel>()
            .LastOrDefault();
        PartySpeedModel? partySpeedModel = campaignStarter.Models
            .OfType<PartySpeedModel>()
            .LastOrDefault();

        if (inventoryCapacityModel is not null && inventoryCapacityModel is not HerdsAndLogisticsInventoryCapacityModel)
        {
            campaignStarter.AddModel(new HerdsAndLogisticsInventoryCapacityModel(inventoryCapacityModel));
        }

        if (partySpeedModel is not null && partySpeedModel is not HerdsAndLogisticsPartySpeedModel)
        {
            campaignStarter.AddModel(new HerdsAndLogisticsPartySpeedModel(partySpeedModel));
        }
    }
}
