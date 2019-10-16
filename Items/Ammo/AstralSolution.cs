﻿
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class AstralSolution : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Solution");
            Tooltip.SetDefault("Used by the Clentaminator.\nSpreads the Astral.");
        }

        public override void SetDefaults()
        {
            item.ammo = AmmoID.Solution;
            item.shoot = ModContent.ProjectileType<AstralSpray>() - ProjectileID.PureSpray;
            item.width = 10;
            item.height = 12;
            item.value = Item.buyPrice(0, 0, 5, 0);
            item.rare = 3;
            item.maxStack = 999;
            item.consumable = true;
            return;
        }

        public override bool ConsumeAmmo(Player player)
        {
            return !(player.itemAnimation < player.HeldItem.useAnimation - 3);
        }
    }
}
