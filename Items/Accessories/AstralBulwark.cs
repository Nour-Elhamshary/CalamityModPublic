﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;

namespace CalamityMod.Items.Accessories {
public class AstralBulwark : ModItem
{
	public override void SetStaticDefaults()
	{
		DisplayName.SetDefault("Astral Bulwark");
		Tooltip.SetDefault("Taking damage drops astral stars from the sky\n" +
		                   "Provides immunity to the god slayer inferno debuff");
	}
	
	public override void SetDefaults()
	{
		item.width = 26;
		item.height = 26;
		item.value = 100000;
		item.expert = true;
		item.accessory = true;
	}
	
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		CalamityPlayer modPlayer = player.GetModPlayer<CalamityPlayer>(mod);
		modPlayer.aBulwark = true;
		player.buffImmune[mod.BuffType("GodSlayerInferno")] = true;
	}
}}