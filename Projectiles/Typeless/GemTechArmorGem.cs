using CalamityMod.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class GemTechArmorGem : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public ref float Variant => ref projectile.ai[1];
        public const int UpwardFlyTime = 24;
        public const int RedirectTime = 12;
        public PrimitiveTrail FlameTrailDrawer = null;
        public override string Texture => "CalamityMod/ExtraTextures/GemTechArmor/YellowGem";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trireme's Gem");
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 14;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = 2;
            projectile.MaxUpdates = 2;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 13;
            projectile.timeLeft = projectile.MaxUpdates * 420;
            projectile.Calamity().rogue = true;
        }

        public Color GemColor => GemTechArmorState.GetColorFromGemType((GemTechArmorGemType)Variant);

        public override void AI()
        {
            // Create a puff of energy on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 7; i++)
                {
                    Dust energyPuff = Dust.NewDustPerfect(projectile.Center, 267);
                    energyPuff.velocity = -Vector2.UnitY.RotatedByRandom(0.81f) * Main.rand.NextFloat(1.25f, 4.5f);
                    energyPuff.color = Color.Lerp(GemColor, Color.White, Main.rand.NextFloat(0.5f));
                    energyPuff.scale = 1.1f;
                    energyPuff.alpha = 185;
                    energyPuff.noGravity = true;
                }
                projectile.localAI[0] = 1f;
            }

            // This intentionally uses boss priority when homing.
            NPC potentialTarget = projectile.Center.ClosestNPCAt(2700f, true, true);

            // Increment the timer on the last extra update.
            if (projectile.FinalExtraUpdate())
                Time++;

            // Fly into the air.
            if (Time < UpwardFlyTime)
            {
                projectile.velocity = Vector2.Lerp(projectile.velocity, -Vector2.UnitY * 3f, 0.1f);
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
                return;
            }

            // Point at nearby targets, if there is one. Also rapidly stop in place.
            if (Time < UpwardFlyTime + RedirectTime)
            {
                if (potentialTarget != null)
                {
                    float angleToTarget = projectile.AngleTo(potentialTarget.Center) + MathHelper.PiOver2;
                    projectile.rotation = projectile.rotation.AngleLerp(angleToTarget, 0.225f).AngleTowards(angleToTarget, 0.6f);
                    projectile.velocity = projectile.velocity.MoveTowards(Vector2.Zero, 1.9f) * 0.9f;
                }
                return;
            }

            // Create some visual and acoustic effects right before charging.
            if (Time == UpwardFlyTime + RedirectTime && projectile.FinalExtraUpdate())
            {
                Main.PlaySound(SoundID.Item72, projectile.Center);
                for (int i = 0; i < 12; i++)
                {
                    Dust energyPuff = Dust.NewDustPerfect(projectile.Center, 267);
                    energyPuff.velocity = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 5f;
                    energyPuff.color = GemColor;
                    energyPuff.scale = 1.125f;
                    energyPuff.alpha = 175;
                    energyPuff.noGravity = true;
                }
            }

            // Rapidly move towards the nearest target.
            if (potentialTarget != null && projectile.penetrate >= projectile.maxPenetrate)
            {
                float distanceFromTarget = projectile.Distance(potentialTarget.Center);
                float moveInterpolant = Utils.InverseLerp(0f, 100f, distanceFromTarget, true) * Utils.InverseLerp(600f, 400f, distanceFromTarget, true);
                Vector2 targetCenterOffsetVec = potentialTarget.Center - projectile.Center;
                float movementSpeed = MathHelper.Min(37.5f, targetCenterOffsetVec.Length());
                Vector2 idealVelocity = targetCenterOffsetVec.SafeNormalize(Vector2.Zero) * movementSpeed;

                // Ensure velocity never has a magnitude less than 4.
                if (projectile.velocity.Length() < 4f)
                    projectile.velocity += projectile.velocity.RotatedBy(MathHelper.PiOver4).SafeNormalize(Vector2.Zero) * 4f;

                // Die if anything goes wrong with the velocity.
                if (projectile.velocity.HasNaNs())
                    projectile.Kill();

                // Approach the ideal velocity.
                projectile.velocity = Vector2.Lerp(projectile.velocity, idealVelocity, moveInterpolant * 0.08f);
                projectile.velocity = projectile.velocity.MoveTowards(idealVelocity, 2f);
            }
        }

        public override bool CanDamage() => Time > UpwardFlyTime + RedirectTime;

        public Color TrailColor(float completionRatio)
        {
            float trailOpacity = Utils.InverseLerp(0f, 0.067f, completionRatio, true) * Utils.InverseLerp(0.7f, 0.58f, completionRatio, true);
            Color startingColor = Color.Lerp(Color.White, GemColor, 0.47f);
            Color middleColor = GemColor;
            Color endColor = Color.Transparent;
            return CalamityUtils.MulticolorLerp(completionRatio, startingColor, middleColor, endColor) * trailOpacity;
        }

        public static float TrailWidth(float completionRatio) => MathHelper.SmoothStep(12f, 4.25f, completionRatio);

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (FlameTrailDrawer is null)
                FlameTrailDrawer = new PrimitiveTrail(TrailWidth, TrailColor, null, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);

            // Prepare the flame trail shader with its map texture.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.GetTexture("CalamityMod/ExtraTextures/ScarletDevilStreak"));

            Texture2D texture;
            switch (Variant)
            {
                case (int)GemTechArmorGemType.Melee:
                default:
                    texture = Main.projectileTexture[projectile.type];
                    break;
                case (int)GemTechArmorGemType.Ranged:
                    texture = ModContent.GetTexture("CalamityMod/ExtraTextures/GemTechArmor/GreenGem");
                    break;
                case (int)GemTechArmorGemType.Magic:
                    texture = ModContent.GetTexture("CalamityMod/ExtraTextures/GemTechArmor/PurpleGem");
                    break;
                case (int)GemTechArmorGemType.Summoner:
                    texture = ModContent.GetTexture("CalamityMod/ExtraTextures/GemTechArmor/BlueGem");
                    break;
                case (int)GemTechArmorGemType.Rogue:
                    texture = ModContent.GetTexture("CalamityMod/ExtraTextures/GemTechArmor/RedGem");
                    break;
                case (int)GemTechArmorGemType.Base:
                    texture = ModContent.GetTexture("CalamityMod/ExtraTextures/GemTechArmor/PinkGem");
                    break;
            }

            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);

            if (projectile.ai[0] > UpwardFlyTime + RedirectTime)
                FlameTrailDrawer.Draw(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 71);

            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            projectile.damage = 0;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = ProjectileID.Sets.TrailCacheLength[projectile.type];
            projectile.netUpdate = true;
        }
    }
}