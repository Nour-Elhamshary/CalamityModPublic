﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class MomentumCapacitor : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Momentum Capacitor");
            Tooltip.SetDefault("Pressing U, for the cost of 25% of max stealth, causes an energy field to appear at the cursor position\n" +
                               "Rogue projectiles that enter the field get a constant acceleration and 15% damage boost\n" +
                               "These boosts can only happen to a projectile once\n" +
                               "There can only be one field");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 32;
            item.value = Item.buyPrice(0, 60, 0, 0);
            item.accessory = true;
            item.rare = 6;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.momentumCapacitor = true;
        }
    }
}
