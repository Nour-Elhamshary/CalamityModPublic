using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons.GreatSandShark
{
    public class DuststormInABottle : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Duststorm in a Bottle");
            Tooltip.SetDefault("Explodes into a dust cloud");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.damage = 47;
            item.thrown = true;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.useAnimation = 25;
            item.useStyle = 1;
            item.useTime = 25;
            item.knockBack = 5f;
            item.UseSound = SoundID.Item106;
            item.autoReuse = true;
            item.height = 24;
            item.maxStack = 1;
            item.value = 100000;
            item.rare = 7;
            item.shoot = mod.ProjectileType("DuststormInABottle");
            item.shootSpeed = 12f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HolyWater, 20);
            recipe.AddIngredient(null, "GrandScale");
            recipe.AddIngredient(ItemID.SandstorminaBottle);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
