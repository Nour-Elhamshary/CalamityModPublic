﻿using Terraria.DataStructures;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Items.Materials;

namespace CalamityMod.Items.Weapons.Summon
{
    public class SlimePuppetStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            // In other words, bootleg mage :TaxEvasion:
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.mana = 10;
            Item.width = 48;
            Item.height = 56;
            Item.useTime = Item.useAnimation = 29;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.6f;
            Item.value = CalamityGlobalItem.Rarity4BuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SlimeGodCore.PossessionSound;
            Item.shoot = ModContent.ProjectileType<SlimePuppet>();
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Summon;
        }

        public override Vector2? HoldoutOrigin() => new Vector2(12f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                int p = Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, Main.myPlayer);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = Item.damage;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(18).
                AddIngredient<BlightedGel>(18).
                AddTile<StaticRefiner>().
                Register();
        }
    }
}
