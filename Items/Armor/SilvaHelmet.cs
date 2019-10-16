﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    [AutoloadEquip(EquipType.Head)]
    public class SilvaHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Silva Helmet");
            Tooltip.SetDefault("+5 max minions");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.buyPrice(0, 90, 0, 0);
            item.defense = 13; //110
            item.Calamity().postMoonLordRarity = 15;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<SilvaArmor>() && legs.type == ModContent.ItemType<SilvaLeggings>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.silvaSet = true;
            modPlayer.silvaSummon = true;
            player.setBonus = "75% increased minion damage\n" +
                "You are immune to almost all debuffs\n" +
                "All projectiles spawn healing leaf orbs on enemy hits\n" +
                "Max run speed and acceleration boosted by 5%\n" +
                "If you are reduced to 1 HP you will not die from any further damage for 10 seconds\n" +
                "If you get reduced to 1 HP again while this effect is active you will lose 100 max life\n" +
                "This effect only triggers once per life and if you are reduced to 400 max life the invincibility effect will stop\n" +
                "Your max life will return to normal if you die\n" +
                "Summons an ancient leaf prism to blast your enemies with life energy\n" +
                "After the silva invulnerability time your minions will deal 10% more damage and you will gain +2 max minions";
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<Buffs.SilvaCrystal>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<Buffs.SilvaCrystal>(), 3600, true);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.SilvaCrystal>()] < 1)
                {
                    Projectile.NewProjectile(player.Center.X, player.Center.Y, 0f, -1f, ModContent.ProjectileType<Projectiles.SilvaCrystal>(), (int)(1500f * player.minionDamage), 0f, Main.myPlayer, 0f, 0f);
                }
            }
            player.minionDamage += 0.75f;
        }

        public override void UpdateEquip(Player player)
        {
            player.maxMinions += 5;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "DarksunFragment", 5);
            recipe.AddIngredient(null, "EffulgentFeather", 5);
            recipe.AddIngredient(null, "CosmiliteBar", 5);
            recipe.AddIngredient(null, "Tenebris", 6);
            recipe.AddIngredient(null, "NightmareFuel", 14);
            recipe.AddIngredient(null, "EndothermicEnergy", 14);
            recipe.AddTile(null, "DraedonsForge");
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
