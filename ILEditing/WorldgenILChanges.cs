﻿using CalamityMod.World;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.WorldBuilding;
using CalamityMod.Systems;
using System.Linq;

namespace CalamityMod.ILEditing
{
    public partial class ILChanges
    {
        #region Replacement of Pharaoh Set in Pyramids
        // Note: There is no need to replace the other Pharaoh piece, due to how the vanilla code works.
        // The other Pharaoh vanity piece is added automatically if the mask is found in the chest.
        private static void ReplacePharaohSetInPyramids(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Find the instruction which loads in the item ID of the Pharaoh's Mask.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(ItemID.PharaohsMask)))
            {
                LogFailure("Pharaoh Set Pyramid Replacement", "Could not locate the Pharaoh's Mask item ID.");
                return;
            }

            // Replace the Pharaoh's Mask with the Amber Hook.
            // THIS INT CAST IS MANDATORY. DO NOT REMOVE IT.
            // In TML's DLL, despite item IDs being shorts (16 bits), the hardcoded 848 here is a 32-bit integer literal.
            // As such, it must be replaced with a 32-bit integer literal or the branches of the switch will misalign and the IL edit fails.
            cursor.Next.Operand = (int)ItemID.AmberHook;
        }
        #endregion Replacement of Pharaoh Set in Pyramids

        #region Fixing of Living Tree/Sulphurous Sea Interactions
        private static void BlockLivingTreesNearOcean(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, int>>(x => Utils.Clamp(x, 560, Main.maxTilesX - 560));
            cursor.Emit(OpCodes.Starg, 0);
        }
        #endregion Fixing of Living Tree/Sulphurous Sea Interactions

        #region Removal of Hardmode Ore Generation from Evil Altars
        private static void PreventSmashAltarCode(Terraria.On_WorldGen.orig_SmashAltar orig, int i, int j)
        {
            if (CalamityConfig.Instance.EarlyHardmodeProgressionRework)
                return;

            orig(i, j);
        }
        #endregion Removal of Hardmode Ore Generation from Evil Altars

        #region Chlorophyte Spread Improvements
        private static void AdjustChlorophyteSpawnRate(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(300))) // 1 in 300 genRand call used to generate Chlorophyte in mud tiles near jungle grass.
            {
                LogFailure("Chlorophyte Spread Rate", "Could not locate the update chance.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 150); // Increase the chance to 1 in 150.
        }

        private static void AdjustChlorophyteSpawnLimits(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(40))) // Find the 40 Chlorophyte tile limit. This limit is checked within a 71x71-tile square, with the reference tile as the center.
            {
                LogFailure("Chlorophyte Spread Limit", "Could not locate the lower limit.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 60); // Increase the limit to 60.

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(130))) // Find the 130 Chlorophyte tile limit. This limit is checked within a 171x171-tile square, with the reference tile as the center.
            {
                LogFailure("Chlorophyte Spread Limit", "Could not locate the upper limit.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 200); // Increase the limit to 200.
        }
        #endregion Chlorophyte Spread Improvements

        #region World Creation UI Default Size Change
        /// <summary>
        /// Modifies the default world size on the world creation menu to be Large instead of Small.
        /// </summary>
        private static void ChangeDefaultWorldSize(ILContext il)
        {
            // Objective 1: Pop value '0' off the stack and emit value '2'. This changes the enum used for setting the default world size.
            // Objective 2: Invoke UpdatePreviewPlate at the end of the method and set _optionSize to Large.
            var c = new ILCursor(il);
            
            // OBJECTIVE 1
            
            // Find and anchor ourselves at roughly the start of the first for loop of this method.
            if (!c.TryGotoNext(x => x.MatchBr(out _)))
            {
                LogFailure("Change Default World Size", "Could not match start of branched for loop.");
                return;
            }
            
            // Position ourselves directly after where '0' is pushed.
            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(0)))
            {
                LogFailure("Change Default World Size", "Could not match '0' indicating WorldSizeId.Small.");
                return;
            }

            // Pop original value off.
            c.Emit(OpCodes.Pop);
            
            // Push '2' to the stack.
            c.Emit(OpCodes.Ldc_I4_2);
            
            // OBJECTIVE 2

            // Match right before the method returns.
            if (!c.TryGotoNext(x => x.MatchRet()))
            {
                LogFailure("Change Default World Size", "Could not match end of method.");
                return;
            }
            
            // Set _optionSize to Large.
            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldc_I4_2); // '2'
            c.Emit<UIWorldCreation>(OpCodes.Stfld, "_optionSize"); // UIWorldCreation._optionSize

            // Invoke UpdatePreviewPlate with our current instance.
            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit<UIWorldCreation>(OpCodes.Call, "UpdatePreviewPlate"); // UIWorldCreation.UpdatePreviewPlate
        }
        #endregion

        #region Change Small World Description
        /// <summary>
        /// Changes the description of Small worlds to serve as a warning.
        /// </summary>
        private static void SwapSmallDescriptionKey(ILContext il)
        {
            // Objective: Swap the string "UI.WorldDescriptionSizeSmall" with "Mods.CalamityMod.UI.SmallWorldWarning".
            var c = new ILCursor(il);

            // Position ourselves after "UI.WorldDescriptionSizeSmall".
            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.WorldDescriptionSizeSmall")))
            {
                LogFailure("Change Small World Description", "Could not match string \"UI.WorldDescriptionSizeSmall\".");
                return;
            }
            // Pop original value off.
            c.Emit(OpCodes.Pop);
            
            // Emit our new string "Mods.CalamityMod.UI.SmallWorldWarning".
            c.Emit(OpCodes.Ldstr, "Mods.CalamityMod.UI.SmallWorldWarning");
        }
        #endregion

        #region Clear temporary modded tiles
        private static void ClearModdedTempTiles(Terraria.IO.On_WorldFile.orig_ClearTempTiles orig)
        {
            orig();

            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (TempTilesManagerSystem.TemporaryTileIDs.Contains(Main.tile[i, j].TileType))
                        WorldGen.KillTile(i, j);
                }
            }
        }
        #endregion
    }
}
