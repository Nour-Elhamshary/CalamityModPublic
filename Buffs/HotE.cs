﻿using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs
{
	public class HotE : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Heart of the Elements");
			Description.SetDefault("All elementals will protect you");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		
		public override void Update(Player player, ref int buffIndex)
		{
			CalamityPlayer modPlayer = player.GetModPlayer<CalamityPlayer>(mod);
			
			if (!modPlayer.allWaifus)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
			else
			{
				player.buffTime[buffIndex] = 18000;
			}
        }
	}
}