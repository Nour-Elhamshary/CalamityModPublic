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
	public class Dualpoon : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dualpoon");
		}

	    public override void SetDefaults()
	    {
	        item.damage = 53;
	        item.ranged = true;
	        item.width = 56;
	        item.height = 28;
	        item.useTime = 20;
	        item.useAnimation = 20;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 6.5f;
	        item.value = 300000;
	        item.rare = 5;
	        item.UseSound = SoundID.Item10;
	        item.autoReuse = true;
	        item.shootSpeed = 20f;
	        item.shoot = mod.ProjectileType("Triploon");
	    }
	    
	    public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
	    {
	    	Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
	    	float num117 = 0.314159274f;
			int num118 = 2;
			Vector2 vector7 = new Vector2(speedX, speedY);
			vector7.Normalize();
			vector7 *= 80f;
			bool flag11 = Collision.CanHit(vector2, 0, 0, vector2 + vector7, 0, 0);
			for (int num119 = 0; num119 < num118; num119++)
			{
				float num120 = (float)num119 - ((float)num118 - 1f) / 2f;
				Vector2 value9 = vector7.RotatedBy((double)(num117 * num120), default(Vector2));
				if (!flag11)
				{
					value9 -= vector7;
				}
				int harpoon = Projectile.NewProjectile(vector2.X + value9.X, vector2.Y + value9.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0.0f, 0.0f);
                Main.projectile[harpoon].timeLeft = 300;
			}
			return false;
		}
	
	    public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(ItemID.Harpoon, 2);
	        recipe.AddIngredient(ItemID.SoulofMight, 10);
	        recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	}
}