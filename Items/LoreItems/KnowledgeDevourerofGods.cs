﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class KnowledgeDevourerofGods : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Devourer of Gods");
            Tooltip.SetDefault("This serpent’s power to assimilate the abilities and energy of those it consumed is unique in almost all the known cosmos, save for its lesser brethren.\n" +
                "I would have soon had to eliminate it as a threat had it been given more time and creatures to feast upon.\n" +
                "Place in your inventory to boost your true melee damage by 50%.");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = 10;
            item.consumable = false;
            item.Calamity().postMoonLordRarity = 13;
        }

        public override bool CanUseItem(Player player)
        {
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.DoGLore = true;
        }
    }
}
