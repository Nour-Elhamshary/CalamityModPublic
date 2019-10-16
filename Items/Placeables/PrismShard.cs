﻿using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class PrismShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Prism Shard");
        }

        public override void SetDefaults()
        {
            item.createTile = ModContent.TileType<SeaPrismCrystals>();
            item.useStyle = 1;
            item.useTurn = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.autoReuse = true;
            item.consumable = true;
            item.width = 26;
            item.height = 26;
            item.maxStack = 999;
            item.value = Item.sellPrice(silver: 3);
            item.rare = 2;
        }
    }
}
