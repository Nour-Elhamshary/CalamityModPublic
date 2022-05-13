﻿using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Accessories
{
    public class FlameLickedShell : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            DisplayName.SetDefault("Flame-Licked Shell");
            Tooltip.SetDefault("35% decreased movement speed\n" +
                                "Enemies take damage when they hit you\n" +
                                "You move faster and lose 18 defense for 3 seconds if you take damage\n" +
                                "Temporary immunity to lava\n" +
                                "Grants immunity to Armor Crunch");
        }

        public override void SetDefaults()
        {
            Item.defense = 36;
            Item.width = 36;
            Item.height = 42;
            Item.value = CalamityGlobalItem.Rarity5BuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.flameLickedShell = true;
            player.buffImmune[ModContent.BuffType<ArmorCrunch>()] = true;
            player.lavaMax += 240;
            float moveSpeedDecrease = modPlayer.shellBoost ? 0.15f : 0.35f;
            player.moveSpeed -= moveSpeedDecrease;
            player.thorns += 0.25f;
            if (modPlayer.shellBoost)
                player.statDefense -= 18;
        }
    }
}