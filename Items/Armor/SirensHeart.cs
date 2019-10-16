﻿using CalamityMod.CalPlayer;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class SirensHeart : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Siren's Heart (Blue)");
            Tooltip.SetDefault("Transforms the holder into a siren\n" +
                "Siren scales give increased defense (gives more defense in hardmode and post-ML)\n" +
                "Siren sight reveals enemy locations (blue-only)\n" +
                "Increases life regen (gives more life regen in hardmode and post-ML)\n" +
                "Going underwater gives you a buff\n" +
                "Greatly reduces breath loss in the abyss\n" +
                "Enemies become frozen when they touch you\n" +
                "You have a layer of ice around you that absorbs 15% damage but breaks after one hit\n" +
                "After 30 seconds the ice shield will regenerate\n" +
                "Your alluring figure allows you to buy items at a reduced price from town npcs (only works in hardmode)\n" +
                "Wow, you can swim now!\n" +
                "Most of these effects are only active after Skeletron has been defeated\n" +
                "Revengeance drop");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.accessory = true;
            item.value = Item.buyPrice(0, 45, 0, 0);
            item.rare = 7;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.sirenBoobs = true;
            if (hideVisual)
            {
                modPlayer.sirenBoobsHide = true;
            }
        }
    }

    public class SirenHead : EquipTexture
    {
        public override bool DrawHead()
        {
            return false;
        }
    }

    public class SirenBody : EquipTexture
    {
        public override bool DrawBody()
        {
            return false;
        }
    }

    public class SirenLegs : EquipTexture
    {
        public override bool DrawLegs()
        {
            return false;
        }
    }
}
