﻿using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class BloodstoneCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bloodstone Core");
        }

        public override void SetDefaults()
        {
            item.width = 15;
            item.height = 12;
            item.maxStack = 999;
            item.rare = 10;
            item.value = Item.sellPrice(gold: 4);
            item.Calamity().postMoonLordRarity = 13;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "Bloodstone", 5);
            recipe.AddIngredient(null, "BloodOrb", 2);
            recipe.AddIngredient(null, "Phantoplasm");
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.SetResult(this, 2);
            recipe.AddRecipe();
        }
    }
}
