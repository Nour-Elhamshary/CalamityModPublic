﻿using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class KnowledgeCrimson : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Crimson");
            Tooltip.SetDefault("This bloody hell, spawned from a formless mass of flesh that fell from the stars eons ago.\n" +
                "It is now home to many hideous creatures, spawned from the pumping blood and lurching organs deep within.");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = 2;
            item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            return false;
        }
    }
}
