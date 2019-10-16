﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class KnowledgeAstrumDeus : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astrum Deus");
            Tooltip.SetDefault("God of the stars and largest vessel for the Astral Infection.\n" +
                "Though struck down from its place among the stars its remnants have gathered strength, aiming to take its rightful place in the cosmos once more.\n" +
                "Place in your inventory to gain increased movement speed in space.");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = 9;
            item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.astrumDeusLore = true;
        }
    }
}
