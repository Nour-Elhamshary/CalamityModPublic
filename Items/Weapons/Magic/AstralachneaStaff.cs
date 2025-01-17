﻿using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class AstralachneaStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 19;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = CalamityGlobalItem.Rarity8BuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AstralachneaFang>();
            Item.shootSpeed = 13f;
        }
                
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
            int i = Main.myPlayer;
            float num72 = Item.shootSpeed;
            int num73 = damage;
            float num74 = knockback;
            float num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
            float num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
            float num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
            int spikeAmount = 4;
            if (Main.rand.NextBool(3))
            {
                spikeAmount++;
            }
            if (Main.rand.NextBool(4))
            {
                spikeAmount++;
            }
            if (Main.rand.NextBool(5))
            {
                spikeAmount += 2;
            }
            for (int num131 = 0; num131 < spikeAmount; num131++)
            {
                float num132 = num78;
                float num133 = num79;
                float num134 = 0.05f * (float)num131;
                num132 += (float)Main.rand.Next(-400, 400) * num134;
                num133 += (float)Main.rand.Next(-400, 400) * num134;
                num80 = (float)Math.Sqrt((double)(num132 * num132 + num133 * num133));
                num80 = num72 / num80;
                num132 *= num80;
                num133 *= num80;
                float x2 = vector2.X;
                float y2 = vector2.Y;
                Projectile.NewProjectile(source, x2, y2, num132, num133, ModContent.ProjectileType<AstralachneaFang>(), num73, num74, i, 0f, 0f);
            }
            return false;
        }
    }
}
