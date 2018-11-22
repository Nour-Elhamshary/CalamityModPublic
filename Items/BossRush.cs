﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using CalamityMod.NPCs;

namespace CalamityMod.Items
{
	public class BossRush : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Terminus");
            Tooltip.SetDefault("A ritualistic artifact, thought to have brought upon The End many millennia ago\n" +
                                "Sealed away in the abyss, far from those that would seek to misuse it");
        }
		
		public override void SetDefaults()
		{
            item.rare = 1;
			item.width = 28;
			item.height = 28;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.UseSound = SoundID.Item123;
			item.consumable = false;
		}

        public override bool UseItem(Player player)
		{
            for (int doom = 0; doom < 200; doom++)
            {
                if (Main.npc[doom].active && Main.npc[doom].boss)
                {
                    Main.npc[doom].active = false;
                    Main.npc[doom].netUpdate = true;
                }
            }
            if (!CalamityWorld.bossRushActive)
            {
                CalamityPlayer.bossRushStage = 0;
                CalamityWorld.bossRushActive = true;
                string key = "Mods.CalamityMod.BossRushStartText";
                Color messageColor = Color.LightCoral;
                if (Main.netMode == 0)
                {
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                else if (Main.netMode == 2)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
                }
            }
            else
            {
                CalamityPlayer.bossRushStage = 0;
                CalamityWorld.bossRushActive = false;
            }
            if (Main.netMode == 2)
            {
                NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
                var netMessage = mod.GetPacket();
                netMessage.Write((byte)CalamityModMessageType.BossRushStage);
                netMessage.Write(CalamityPlayer.bossRushStage);
                netMessage.Send();
            }
            return true;
		}
    }
}