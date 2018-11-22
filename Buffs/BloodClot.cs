﻿using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs
{
	public class BloodClot : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Blood Clot");
			Description.SetDefault("The blood clot will protect you");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		
		public override void Update(Player player, ref int buffIndex)
		{
			CalamityPlayer modPlayer = player.GetModPlayer<CalamityPlayer>(mod);
			if (player.ownedProjectileCounts[mod.ProjectileType("BloodClotMinion")] > 0)
			{
				modPlayer.bClot = true;
			}
			if (!modPlayer.bClot)
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