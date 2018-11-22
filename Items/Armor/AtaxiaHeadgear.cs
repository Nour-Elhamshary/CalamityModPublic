﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class AtaxiaHeadgear : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ataxia Headgear");
            Tooltip.SetDefault("12% increased ranged damage and 10% increased ranged critical strike chance\n" +
                "Reduces ammo cost by 25%\n" +
                "Immune to lava and fire damage");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 450000;
            item.rare = 8;
            item.defense = 15; //53
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == mod.ItemType("AtaxiaArmor") && legs.type == mod.ItemType("AtaxiaSubligar");
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawOutlines = true;
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
            drawAltHair = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "5% increased ranged damage and critical strike chance\n" +
                "Inferno effect when below 50% life\n" +
                "You have a 50% chance to fire a homing chaos flare when using ranged weapons\n" +
                "You have a 20% chance to emit a blazing explosion when you are hit";
            CalamityPlayer modPlayer = player.GetModPlayer<CalamityPlayer>(mod);
            modPlayer.ataxiaBlaze = true;
            modPlayer.ataxiaBolt = true;
            if (player.statLife <= (player.statLifeMax2 * 0.5f))
            {
                player.AddBuff(BuffID.Inferno, 2);
            }
            player.rangedDamage += 0.05f;
            player.rangedCrit += 5;
        }

        public override void UpdateEquip(Player player)
        {
            player.ammoCost75 = true;
            player.rangedDamage += 0.12f;
            player.rangedCrit += 10;
            player.lavaImmune = true;
            player.buffImmune[BuffID.OnFire] = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "CruptixBar", 7);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}