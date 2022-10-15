﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.TileEntities;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CalamityMod.UI.DraedonSummoning
{
    public static partial class CodebreakerUI
    {
        public static float CommunicationPanelScale
        {
            get;
            set;
        } = 0f;

        public static int DraedonTextCreationTimer
        {
            get;
            set;
        }

        public static string DraedonText
        {
            get;
            set;
        } = string.Empty;

        public static List<string> DialogHistory
        {
            get;
            set;
        } = new();

        public static int DialogHistoryIndex
        {
            get;
            set;
        }

        public static float DraedonScreenStaticInterpolant
        {
            get;
            set;
        } = 1f;

        public static float DraedonTextOptionsOpacity
        {
            get;
            set;
        } = 1f;

        // The text that Draedon should attempt to spell out.
        public static string DraedonTextComplete
        {
            get;
            set;
        } = string.Empty;

        // This is used to give a one-frame buffer before dialog actually appears. The reason for this is to prevent dialog sometimes showing up for one frame
        // in the case of dialog entries being pruned because of going past the natural box.
        public static bool CanDisplayLatestDialogEntries
        {
            get;
            set;
        } = true;

        public static string InquiryText => "State your inquiry.";

        public static Dictionary<string, string> DialogQueries => new()
        {
            ["Mrrp"] = "Mrrp is cringe.",
            ["Calamitas"] = "She owes me $20."
        };

        public static void DisplayCommunicationPanel()
        {
            // Draw the background panel. This pops up.
            float panelWidthScale = Utils.Remap(CommunicationPanelScale, 0f, 0.5f, 0.08f, 1f);
            float panelHeightScale = Utils.Remap(CommunicationPanelScale, 0.5f, 1f, 0.08f, 1f);
            Vector2 panelScale = GeneralScale * new Vector2(panelWidthScale, panelHeightScale) * 2.4f;
            Texture2D panelTexture = ModContent.Request<Texture2D>("CalamityMod/UI/DraedonSummoning/DraedonContactPanel").Value;
            float basePanelHeight = GeneralScale * panelTexture.Height * 2.4f;
            Vector2 panelCenter = new Vector2(Main.screenWidth * 0.5f, BackgroundCenter.Y + panelTexture.Height * panelScale.Y * 0.5f - basePanelHeight * 0.5f);
            Rectangle panelArea = Utils.CenteredRectangle(panelCenter, panelTexture.Size() * panelScale);

            Main.spriteBatch.Draw(panelTexture, panelCenter, null, Color.White, 0f, panelTexture.Size() * 0.5f, panelScale, 0, 0f);

            // Draw static if the static interpolant is sufficiently high.
            if (DraedonScreenStaticInterpolant > 0f)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

                // Apply a glitch shader.
                GameShaders.Misc["CalamityMod:BlueStatic"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SharpNoise"));
                GameShaders.Misc["CalamityMod:BlueStatic"].Shader.Parameters["useStaticLine"].SetValue(false);
                GameShaders.Misc["CalamityMod:BlueStatic"].Shader.Parameters["coordinateZoomFactor"].SetValue(0.5f);
                GameShaders.Misc["CalamityMod:BlueStatic"].Shader.Parameters["useTrueNoise"].SetValue(true);
                GameShaders.Misc["CalamityMod:BlueStatic"].Apply();

                float readjustedInterpolant = Utils.GetLerpValue(0.42f, 1f, DraedonScreenStaticInterpolant, true);
                Color staticColor = Color.White * (float)Math.Pow(CalamityUtils.AperiodicSin(readjustedInterpolant * 2.94f) * 0.5f + 0.5f, 0.54) * (float)Math.Pow(readjustedInterpolant, 0.51D);
                Main.spriteBatch.Draw(panelTexture, panelCenter, null, staticColor, 0f, panelTexture.Size() * 0.5f, panelScale, 0, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            }

            // Disable clicks if hovering over the panel.
            if (panelArea.Intersects(MouseScreenArea))
                Main.blockMouse = Main.LocalPlayer.mouseInterface = true;

            // Draw a panel that has Draedon's face.
            float draedonIconDrawInterpolant = Utils.GetLerpValue(0.51f, 0.36f, DraedonScreenStaticInterpolant, true);
            Vector2 draedonIconDrawTopRight = panelCenter + panelTexture.Size() * new Vector2(-0.5f, -0.5f) * panelScale + new Vector2(2f, 4f) * panelScale;
            draedonIconDrawTopRight += new Vector2(24f, 4f) * panelScale;

            Vector2 draedonIconScale = panelScale * new Vector2(0.32f, 0.25f);
            Vector2 draedonIconCenter = draedonIconDrawTopRight + panelTexture.Size() * new Vector2(0.5f, 0.5f) * draedonIconScale;
            Rectangle draedonIconArea = Utils.CenteredRectangle(draedonIconCenter, panelTexture.Size() * draedonIconScale * 0.95f);
            Main.spriteBatch.Draw(panelTexture, draedonIconDrawTopRight, null, Color.White * draedonIconDrawInterpolant, 0f, Vector2.Zero, draedonIconScale, 0, 0f);

            // Draw Draedon's face inside the panel.
            // This involves restarting the sprite batch with a rasterizer state that can cut out Draedon's face if it exceeds the icon area.
            Main.spriteBatch.EnforceCutoffRegion(draedonIconArea, Matrix.Identity, SpriteSortMode.Immediate);

            // Apply a glitch shader.
            GameShaders.Misc["CalamityMod:TeleportDisplacement"].UseOpacity(0.015f);
            GameShaders.Misc["CalamityMod:TeleportDisplacement"].UseSecondaryColor(Color.White * 0.75f);
            GameShaders.Misc["CalamityMod:TeleportDisplacement"].UseSaturation(0.75f);
            GameShaders.Misc["CalamityMod:TeleportDisplacement"].Shader.Parameters["frameCount"].SetValue(Vector2.One);
            GameShaders.Misc["CalamityMod:TeleportDisplacement"].Apply();

            Vector2 draedonScale = new Vector2(draedonIconDrawInterpolant, 1f) * 1.6f;
            SpriteEffects draedonDirection = SpriteEffects.FlipHorizontally;
            Texture2D draedonFaceTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/ExoMechs/HologramDraedon").Value;

            Main.spriteBatch.Draw(draedonFaceTexture, draedonIconCenter, null, Color.White * draedonIconDrawInterpolant, 0f, draedonFaceTexture.Size() * 0.5f, draedonScale, draedonDirection, 0f);
            Main.spriteBatch.ReleaseCutoffRegion(Matrix.Identity, SpriteSortMode.Immediate);

            // Draw a glitch effect over the panel and Draedon's icon.
            GameShaders.Misc["CalamityMod:BlueStatic"].UseImage1("Images/Misc/noise");
            GameShaders.Misc["CalamityMod:BlueStatic"].Shader.Parameters["useStaticLine"].SetValue(true);
            GameShaders.Misc["CalamityMod:BlueStatic"].Shader.Parameters["coordinateZoomFactor"].SetValue(1f);
            GameShaders.Misc["CalamityMod:BlueStatic"].Apply();
            Main.spriteBatch.Draw(panelTexture, draedonIconDrawTopRight, null, Color.White * draedonIconDrawInterpolant, 0f, Vector2.Zero, draedonIconScale, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, Matrix.Identity);

            DisplayTextSelectionOptions(panelArea, panelScale);
            DisplayDialogHistory(panelArea, panelScale);
        }

        public static void DisplayTextSelectionOptions(Rectangle panelArea, Vector2 panelScale)
        {
            // Draw the outline for the text selection options.
            float selectionOptionsDrawInterpolant = Utils.GetLerpValue(0.3f, 0f, DraedonScreenStaticInterpolant, true);
            Texture2D selectionOutline = ModContent.Request<Texture2D>("CalamityMod/UI/DraedonSummoning/DraedonSelectionOutline").Value;
            Vector2 selectionCenter = panelArea.BottomLeft() - new Vector2(selectionOutline.Width * -0.5f - 24f, selectionOutline.Height * 0.5f + 24f) * panelScale;
            Rectangle selectionArea = Utils.CenteredRectangle(selectionCenter, selectionOutline.Size() * panelScale);
            Main.spriteBatch.Draw(selectionOutline, selectionCenter, null, Color.White * selectionOptionsDrawInterpolant, 0f, selectionOutline.Size() * 0.5f, panelScale, 0, 0f);

            // Update the options opacity.
            bool canChooseQuery = DraedonText.Length == DraedonTextComplete.Length;
            DraedonTextOptionsOpacity = MathHelper.Clamp(DraedonTextOptionsOpacity + canChooseQuery.ToDirectionInt() * 0.1f, 0f, 1f);

            // Display text options in the box.
            float opacity = DraedonTextOptionsOpacity * (1f - DraedonScreenStaticInterpolant);
            Vector2 textTopLeft = selectionArea.TopLeft() + new Vector2(16f, 12f) * panelScale;
            Texture2D markerTexture = ModContent.Request<Texture2D>("CalamityMod/UI/DraedonSummoning/DraedonInquirySelector").Value;
            foreach (string query in DialogQueries.Keys)
            {
                // Draw the text marker.
                Vector2 markerScale = panelScale * 0.2f;
                Vector2 markerDrawPosition = textTopLeft - Vector2.UnitX * markerTexture.Width * markerScale.X * 0.67f;
                markerDrawPosition.Y += markerScale.Y * 14f;

                Color textColor = Color.Cyan;
                Color markerColor = Color.White;
                Vector2 textArea = FontAssets.MouseText.Value.MeasureString(query) * GeneralScale * 0.67f;
                Rectangle textAreaRect = new((int)textTopLeft.X, (int)textTopLeft.Y, (int)textArea.X, (int)textArea.Y);
                Rectangle markerArea = Utils.CenteredRectangle(markerDrawPosition, markerTexture.Size() * markerScale);

                // Check if the player clicks on the text or hovers over it.
                // If they're hovering over it, change the color to a yellow hover.
                // If they clicked, have the player select that option as the dialog to ask Draedon about.
                if ((MouseScreenArea.Intersects(textAreaRect) || MouseScreenArea.Intersects(markerArea)) && opacity >= 1f)
                {
                    textColor = Color.Lerp(textColor, Color.Yellow, 0.5f);
                    markerColor = Color.Yellow;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        DialogHistory[^1] = query;
                        DialogHistory.Add(string.Empty);
                        DraedonTextComplete = DialogQueries[query];
                        DraedonText = string.Empty;
                        CanDisplayLatestDialogEntries = false;
                    }
                }
                Main.spriteBatch.Draw(markerTexture, markerDrawPosition, null, markerColor * opacity, 0f, markerTexture.Size() * 0.5f, markerScale, 0, 0f);

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, query, textTopLeft, textColor * opacity, 0f, Vector2.Zero, Vector2.One * GeneralScale * 0.67f);
                textTopLeft.Y += panelScale.Y * 12f;
            }
        }

        public static void DisplayDialogHistory(Rectangle panelArea, Vector2 panelScale)
        {
            // Draw the outline for the dialog history.
            float dialogHistoryDrawInterpolant = Utils.GetLerpValue(0.3f, 0f, DraedonScreenStaticInterpolant, true);
            Texture2D dialogOutline = ModContent.Request<Texture2D>("CalamityMod/UI/DraedonSummoning/DraedonDialogOutline").Value;
            Vector2 selectionCenter = panelArea.TopRight() - new Vector2(dialogOutline.Width * 0.5f + 16f, dialogOutline.Height * -0.5f - 6.5f) * panelScale;
            Rectangle dialogArea = Utils.CenteredRectangle(selectionCenter, dialogOutline.Size() * panelScale);
            Main.spriteBatch.Draw(dialogOutline, selectionCenter, null, Color.White * dialogHistoryDrawInterpolant, 0f, dialogOutline.Size() * 0.5f, panelScale, 0, 0f);

            // Intialize Draedon's dialog if necessary.
            if (string.IsNullOrEmpty(DraedonTextComplete))
                DraedonTextComplete = InquiryText;

            // Type out Draedon text.
            if (DraedonScreenStaticInterpolant <= 0f)
                DraedonTextCreationTimer++;
            if (DraedonTextCreationTimer >= DraedonTextCreationRate && DraedonText.Length < DraedonTextComplete.Length)
            {
                char nextLetter = DraedonTextComplete[DraedonText.Length];
                DraedonText += nextLetter;
                DraedonTextCreationTimer = 0;

                // Initialize the dialog history if it's empty.
                if (DialogHistory.Count <= 0)
                    DialogHistory.Add(string.Empty);

                // Update the last dialog history entry as Draedon types.
                DialogHistory[^1] = DraedonText;

                // Move to the next index in the dialog history once Draedon is finished speaking.
                if (DraedonText.Length >= DraedonTextComplete.Length)
                    DialogHistory.Add(string.Empty);
            }

            // Display text in the box.
            int textIndex = 0;
            int entriesToPrune = 0;
            bool showNewEntries = CanDisplayLatestDialogEntries;
            var dialogEntries = DialogHistory.Where(d => !string.IsNullOrEmpty(d));
            Vector2 textTopLeft = dialogArea.TopLeft() + new Vector2(16f, 14f) * panelScale;
            Texture2D markerTexture = ModContent.Request<Texture2D>("CalamityMod/UI/DraedonSummoning/DraedonInquirySelector").Value;
            foreach (string entry in dialogEntries)
            {
                // Define a bunch of variables for text. These vary based on whether it's Draedon speaking or not.
                string dialog = entry;
                bool textIsFromDraedon = textIndex % 2 == 0;
                Color dialogColor = Draedon.TextColor;
                Vector2 localTextTopLeft = textTopLeft;
                Vector2 markerScale = panelScale * 0.2f;
                Vector2 markerDrawPosition = textTopLeft - Vector2.UnitX * markerTexture.Width * markerScale.X * 0.67f;
                markerDrawPosition.Y += markerScale.Y * 14f;
                SpriteEffects markerDirection = SpriteEffects.None;
                if (!textIsFromDraedon)
                {
                    // Flip positions to the other side of the dialog outline if the text is being said by the player.
                    Vector2 anchorPoint = new(dialogArea.Center.X, markerDrawPosition.Y);
                    markerDrawPosition.X = anchorPoint.X + (anchorPoint.X - markerDrawPosition.X);
                    localTextTopLeft.X = anchorPoint.X + (anchorPoint.X - localTextTopLeft.X);
                    localTextTopLeft.X -= FontAssets.MouseText.Value.MeasureString(dialog).X;

                    // Use a neutral grey-ish color if text is being said by the player.
                    dialogColor = Color.LightGray;

                    markerDirection = SpriteEffects.FlipHorizontally;
                }

                if (entriesToPrune <= 0 && (textIndex < dialogEntries.Count() - 2 || showNewEntries))
                {
                    // Draw the text marker.
                    Main.spriteBatch.Draw(markerTexture, markerDrawPosition, null, Color.White * dialogHistoryDrawInterpolant, 0f, markerTexture.Size() * 0.5f, markerScale, markerDirection, 0f);

                    // Draw the text itself.
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, dialog, localTextTopLeft, dialogColor * dialogHistoryDrawInterpolant, 0f, Vector2.Zero, Vector2.One * GeneralScale * 0.67f);
                }
                textTopLeft.Y += panelScale.Y * 12f;
                if (textTopLeft.Y >= dialogArea.Bottom)
                    entriesToPrune++;

                textIndex++;
            }

            // If the text entries went past the dialog box, prune the oldest ones.
            while (entriesToPrune >= 1)
            {
                for (int i = 0; i < 2; i++)
                    DialogHistory.RemoveAt(0);
                entriesToPrune--;
            }

            CanDisplayLatestDialogEntries = true;
        }
    }
}
