using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Weapons 
{
	public class PrimordialEarth : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Primordial Earth");
			Tooltip.SetDefault("An ancient relic from an ancient land");
		}

	    public override void SetDefaults()
	    {
	        item.damage = 80;
	        item.magic = true;
	        item.mana = 19;
	        item.width = 28;
	        item.height = 30;
	        item.useTime = 20;
	        item.useAnimation = 20;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 7;
	        item.value = 1000000;
	        item.rare = 8;
	        item.UseSound = SoundID.Item20;
	        item.autoReuse = true;
	        item.shoot = mod.ProjectileType("SupremeDustProjectile");
	        item.shootSpeed = 4f;
	    }
	
	    public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(null, "DeathValley");
	        recipe.AddIngredient(ItemID.AncientBattleArmorMaterial, 3);
	        recipe.AddIngredient(ItemID.MeteoriteBar, 5);
	        recipe.AddIngredient(ItemID.Ectoplasm, 5);
	        recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	}
}