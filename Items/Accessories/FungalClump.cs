﻿using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class FungalClump : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public const int FungalClumpDamage = 10;

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 42;
            Item.value = CalamityGlobalItem.Rarity2BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.fungalClump = true;
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<FungalClumpBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<FungalClumpBuff>(), 3600, true);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<FungalClumpMinion>()] < 1)
                {
                    var source = player.GetSource_Accessory(Item);
                    int damage = (int)player.GetBestClassDamage().ApplyTo(FungalClumpDamage);
                    int p = Projectile.NewProjectile(source, player.Center.X, player.Center.Y, 0f, -1f, ModContent.ProjectileType<FungalClumpMinion>(), damage, 1f, player.whoAmI);
                    if (Main.projectile.IndexInRange(p))
                        Main.projectile[p].originalDamage = FungalClumpDamage;
                }
            }
        }
        public override void UpdateVanity(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.fungalClumpVanity = true;
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<FungalClumpBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<FungalClumpBuff>(), 3600, true);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<FungalClumpMinion>()] < 1)
                {
                    var source = player.GetSource_Accessory(Item);
                    int damage = (int)player.GetBestClassDamage().ApplyTo(FungalClumpDamage);
                    int p = Projectile.NewProjectile(source, player.Center.X, player.Center.Y, 0f, -1f, ModContent.ProjectileType<FungalClumpMinion>(), damage, 1f, player.whoAmI);
                    if (Main.projectile.IndexInRange(p))
                        Main.projectile[p].originalDamage = FungalClumpDamage;
                }
            }
        }
    }
}
