using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Backgrounds
{
    public class AstralSnowSurfaceBGStyle : ModSurfaceBgStyle
    {
        public override int ChooseFarTexture() => mod.GetBackgroundSlot("Backgrounds/AstralSnowSurfaceFar");

        public override int ChooseMiddleTexture() => mod.GetBackgroundSlot("Backgrounds/AstralSnowSurfaceMiddle");

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => mod.GetBackgroundSlot("Backgrounds/AstralSurfaceClose");

        public override bool ChooseBgStyle() => !Main.gameMenu && Main.LocalPlayer.InAstral() && Main.LocalPlayer.ZoneSnow;

        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            //This just fades in the background and fades out other backgrounds.
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                    {
                        fades[i] = 1f;
                    }
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                    {
                        fades[i] = 0f;
                    }
                }
            }
        }
    }
}
