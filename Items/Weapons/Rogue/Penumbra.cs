﻿using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Penumbra : RogueWeapon
    {
        public static float ShootSpeed = 9f;
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 32;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item103;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;

            Item.damage = 830;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.knockBack = 8f;
            Item.shoot = ModContent.ProjectileType<PenumbraBomb>();
            Item.shootSpeed = 9f;

            Item.rare = ModContent.RarityType<DarkBlue>();
            Item.DamageType = RogueDamageClass.Instance;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 16;

        public override float StealthDamageMultiplier => 0.85f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
                float num78 = Main.mouseX + Main.screenPosition.X - vector2.X;
                float num79 = Main.mouseY + Main.screenPosition.Y - vector2.Y;
                if (player.gravDir == -1f)
                {
                    num79 = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - vector2.Y;
                }
                if ((float.IsNaN(num78) && float.IsNaN(num79)) || (num78 == 0f && num79 == 0f))
                {
                    num78 = player.direction;
                    num79 = 0f;
                }
                vector2 += new Vector2(num78, num79);
                int proj = Projectile.NewProjectile(source, vector2, new Vector2(0f,-0.5f), ModContent.ProjectileType<PenumbraBomb>(), damage, knockback, player.whoAmI, 0f, 1f);
                if (proj.WithinBounds(Main.maxProjectiles))
                    Main.projectile[proj].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RuinousSoul>(6).
                AddIngredient<CosmiliteBar>(8).
                AddIngredient<NightmareFuel>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
