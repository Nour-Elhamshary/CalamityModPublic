﻿using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class IronHeart : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iron Heart");
            Tooltip.SetDefault("Makes dying while a boss is alive permanently kill you.\n" +
                "Can be toggled on and off.\n" +
                "Using this while a boss is alive will permanently kill you.\n" +
                "Cannot be activated if any boss has been killed.");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.expert = true;
            item.rare = 9;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = 4;
            item.UseSound = SoundID.Item119;
            item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            if (CalamityWorld.downedBossAny)
            {
                return false;
            }
            return true;
        }

        public override bool UseItem(Player player)
        {
            for (int doom = 0; doom < 200; doom++)
            {
                if (Main.npc[doom].active && Main.npc[doom].boss)
                {
                    player.KillMeForGood();
                    Main.npc[doom].active = false;
                    Main.npc[doom].netUpdate = true;
                }
            }
            if (!CalamityWorld.ironHeart)
            {
                CalamityWorld.ironHeart = true;
                string key = "Mods.CalamityMod.IronHeartText";
                Color messageColor = Color.LightSkyBlue;
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                }
            }
            else
            {
                CalamityWorld.ironHeart = false;
                string key = "Mods.CalamityMod.IronHeartText2";
                Color messageColor = Color.LightSkyBlue;
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                }
            }
            CalamityMod.UpdateServerBoolean();
            return true;
        }
    }
}
