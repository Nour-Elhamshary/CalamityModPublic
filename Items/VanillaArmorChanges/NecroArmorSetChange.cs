﻿using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class NecroArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.NecroHelmet;

        public override int? BodyPieceID => ItemID.NecroBreastplate;

        public override int? LegPieceID => ItemID.NecroGreaves;

        public override int[] AlternativeHeadPieceIDs => new int[] { ItemID.AncientNecroHelmet };

        public override string ArmorSetName => "Necro";

        public const int PostMortemDuration = 7;
        public static readonly SoundStyle TimerSound = new("CalamityMod/Sounds/Custom/TickingTimer");

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            string extraLines = "\nAllows you to temporarily survive fatal damage\n" +
                $"Grants up to {PostMortemDuration} seconds of life before imminent death";
            setBonusText += extraLines;
        }

        public override void ApplyArmorSetBonus(Player player) => player.Calamity().necroSet = true;
    }
}
