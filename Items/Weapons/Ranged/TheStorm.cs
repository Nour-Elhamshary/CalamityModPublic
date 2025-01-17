﻿using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class TheStorm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.damage = 35;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 34;
            Item.height = 50;
            Item.useTime = 7;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.5f;
            Item.value = CalamityGlobalItem.Rarity12BuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.UseSound = SoundID.Item122;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Bolt>();
            Item.shootSpeed = 28f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int i = Main.myPlayer;
            float num72 = Main.rand.Next(25, 30);
            player.itemTime = Item.useTime;
            Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
            float num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
            float num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
            if (player.gravDir == -1f)
            {
                num79 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
            }
            float num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
            if ((float.IsNaN(num78) && float.IsNaN(num79)) || (num78 == 0f && num79 == 0f))
            {
                num78 = (float)player.direction;
                num79 = 0f;
                num80 = num72;
            }
            else
            {
                num80 = num72 / num80;
            }

            for (int j = 0; j < 3; j++)
            {
                vector2 = new Vector2(player.position.X + (float)player.width * 0.5f + (float)(Main.rand.Next(201) * -(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
                vector2.X = (vector2.X + player.Center.X) / 2f + (float)Main.rand.Next(-200, 201);
                vector2.Y -= (float)(100 * j);
                num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
                num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
                if (num79 < 0f)
                {
                    num79 *= -1f;
                }
                if (num79 < 20f)
                {
                    num79 = 20f;
                }
                num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
                num80 = num72 / num80;
                num78 *= num80;
                num79 *= num80;
                float speedX4 = num78 + (float)Main.rand.Next(-120, 121) * 0.01f;
                float speedY5 = num79 + (float)Main.rand.Next(-120, 121) * 0.01f;
                if (CalamityUtils.CheckWoodenAmmo(type, player))
                {
                    Projectile.NewProjectile(source, vector2.X, vector2.Y, speedX4, speedY5 * 0.9f, ModContent.ProjectileType<Bolt>(), damage, knockback, i);
                    Projectile.NewProjectile(source, vector2.X, vector2.Y, speedX4, speedY5 * 0.8f, ModContent.ProjectileType<Bolt>(), damage, knockback, i);
                }
                else
                {
                    int arrow1 = Projectile.NewProjectile(source, vector2.X, vector2.Y, speedX4, speedY5 * 0.9f, type, damage, knockback, i);
                    Main.projectile[arrow1].noDropItem = true;
                    int arrow2 = Projectile.NewProjectile(source, vector2.X, vector2.Y, speedX4, speedY5 * 0.8f, type, damage, knockback, i);
                    Main.projectile[arrow2].noDropItem = true;
                }
            }
            return false;
        }
    }
}
