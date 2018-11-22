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
	public class Tumbleweed : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tumbleweed");
            Tooltip.SetDefault("Releases a rolling tumbleweed on enemy hits");
        }

	    public override void SetDefaults()
	    {
	        item.damage = 110;
	        item.melee = true;
	        item.width = 30;
	        item.height = 10;
	        item.useTime = 20;
	        item.useAnimation = 20;
	        item.useStyle = 5;
	        item.noMelee = true;
            item.noUseGraphic = true;
	        item.knockBack = 8f;
	        item.value = 100000;
	        item.rare = 7;
	        item.UseSound = SoundID.Item1;
	        item.autoReuse = true;
            item.channel = true;
	        item.shoot = mod.ProjectileType("Tumbleweed");
	        item.shootSpeed = 12f;
	    }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Sunfury);
            recipe.AddIngredient(null, "GrandScale");
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}