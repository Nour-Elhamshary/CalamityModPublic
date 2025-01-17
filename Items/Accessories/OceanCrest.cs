﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class OceanCrest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 26;
            Item.value = CalamityGlobalItem.Rarity2BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.npcTypeNoAggro[NPCID.Shark] = true;
            player.npcTypeNoAggro[NPCID.SeaSnail] = true;
            player.npcTypeNoAggro[NPCID.PinkJellyfish] = true;
            player.npcTypeNoAggro[NPCID.Crab] = true;
            player.npcTypeNoAggro[NPCID.Squid] = true;
            player.gills = true;
        }
    }
}
