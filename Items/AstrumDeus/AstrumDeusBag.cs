using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Items.AstrumDeus
{
	public class AstrumDeusBag : ModItem
	{
		public override void SetStaticDefaults()
 		{
 			DisplayName.SetDefault("Treasure Bag");
 			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
 		}
		
		public override void SetDefaults()
		{
			item.maxStack = 999;
			item.consumable = true;
			item.width = 24;
			item.height = 24;
			item.expert = true;
			item.rare = 9;
			bossBagNPC = mod.NPCType("AstrumDeusHeadSpectral");
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void OpenBossBag(Player player)
		{
			player.TryGettingDevArmor();
            player.QuickSpawnItem(mod.ItemType("Stardust"), Main.rand.Next(60, 91));
            if (Main.rand.Next(4) == 0)
            {
                player.QuickSpawnItem(mod.ItemType("Starfall"));
            }
            if (Main.rand.Next(4) == 0)
            {
                player.QuickSpawnItem(mod.ItemType("Nebulash"));
            }
            player.QuickSpawnItem(mod.ItemType("AstralBulwark"));
            if (CalamityWorld.revenge)
            {
                player.QuickSpawnItem(mod.ItemType("StarlightFuelCell"));
            }
            if (Main.rand.Next(7) == 0)
            {
                player.QuickSpawnItem(mod.ItemType("AstrumDeusMask"));
            }
        }
	}
}