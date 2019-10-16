﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class ArcanumoftheVoid : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arcanum of the Void");
            Tooltip.SetDefault("You have a 5% chance to reflect projectiles when they hit you\n" +
                               "If this effect triggers you get healed for the projectile's damage");
        }

        public override void SetDefaults()
        {
            item.width = 26;
            item.height = 26;
            item.value = Item.buyPrice(0, 60, 0, 0);
            item.accessory = true;
            item.Calamity().postMoonLordRarity = 13;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.projRef = true;
        }
    }
}
