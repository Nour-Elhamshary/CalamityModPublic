using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons.Perforators
{
	public class Aorta : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aorta");
		}

	    public override void SetDefaults()
	    {
	    	item.CloneDefaults(ItemID.Valor);
	        item.damage = 25;
	        item.useTime = 22;
	        item.useAnimation = 22;
	        item.useStyle = 5;
	        item.channel = true;
	        item.melee = true;
	        item.knockBack = 4.25f;
	        item.value = 70000;
	        item.rare = 3;
	        item.autoReuse = false;
	        item.shoot = mod.ProjectileType("AortaProjectile");
	    }
	    
	    public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(null, "BloodSample", 6);
	        recipe.AddIngredient(ItemID.Vertebrae, 3);
	        recipe.AddIngredient(ItemID.CrimtaneBar, 3);
	        recipe.AddTile(TileID.DemonAltar);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
		}
	}
}