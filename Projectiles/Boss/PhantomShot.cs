﻿using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Boss
{
    public class PhantomShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Boss/PhantomHookShot";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            }
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 9f)
            {
                Projectile.alpha -= 5;
                if (Projectile.alpha < 30)
                    Projectile.alpha = 30;
            }
            if (Projectile.localAI[0] > 180f && Projectile.localAI[0] < 240f && Main.expertMode)
            {
                if (Projectile.ai[0] == 0f)
                    Projectile.ai[0] = Projectile.velocity.Length() * 3f;

                int num189 = Player.FindClosest(Projectile.Center, 1, 1);
                Vector2 vector20 = Main.player[num189].Center - Projectile.Center;
                vector20.Normalize();
                vector20 *= Projectile.ai[0];
                int num190 = 80;
                Projectile.velocity = (Projectile.velocity * (num190 - 1) + vector20) / num190;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(100, 250, 250, Projectile.alpha);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Nightwither>(), 80);
        }
    }
}
