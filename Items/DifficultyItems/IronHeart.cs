using CalamityMod.NPCs.SlimeGod;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.DifficultyItems
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
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.UseSound = SoundID.Item119;
            item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            return !CalamityWorld.downedBossAny;
        }

        public override bool UseItem(Player player)
        {
            for (int doom = 0; doom < 200; doom++)
            {
                if ((Main.npc[doom].active && (Main.npc[doom].boss || Main.npc[doom].type == NPCID.EaterofWorldsHead || Main.npc[doom].type == NPCID.EaterofWorldsTail || Main.npc[doom].type == ModContent.NPCType<SlimeGodRun>() ||
                    Main.npc[doom].type == ModContent.NPCType<SlimeGodRunSplit>() || Main.npc[doom].type == ModContent.NPCType<SlimeGod>() || Main.npc[doom].type == ModContent.NPCType<SlimeGodSplit>())) || CalamityWorld.DoGSecondStageCountdown > 0)
                {
					player.KillMeForGood();
                    Main.npc[doom].active = Main.npc[doom].friendly;
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
            CalamityWorld.DoGSecondStageCountdown = 0;

            CalamityMod.UpdateServerBoolean();

            if (Main.netMode == NetmodeID.Server)
            {
                var netMessage = mod.GetPacket();
                netMessage.Write((byte)CalamityModMessageType.IronHeartBoolSync);
                netMessage.Write(CalamityWorld.ironHeart);
                netMessage.Send();
            }
            if (Main.netMode == NetmodeID.Server)
            {
                var netMessage = mod.GetPacket();
                netMessage.Write((byte)CalamityModMessageType.DoGCountdownSync);
                netMessage.Write(CalamityWorld.DoGSecondStageCountdown);
                netMessage.Send();
            }
            return true;
        }
    }
}
