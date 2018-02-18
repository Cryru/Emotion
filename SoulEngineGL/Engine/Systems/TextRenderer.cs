// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Legacy;

#endregion

namespace Soul.Engine.Systems
{
    internal partial class TextRenderer : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(TextData)};
        }

        protected internal override void Setup()
        {
            // Initialize the tag dictionary.
            if (TagFactory.TagDict == null) TagFactory.Initialize();

            // We need this to run before any rendering.
            Order = 10;
        }

        internal override void Update(Entity link)
        {
        }

        internal override void Draw(Entity link)
        {
            // Get components.
            TextData textData = link.GetComponent<TextData>();

            // Check if we need to generate cache.
            if (textData.Recalculate || link.Size != textData.CachedSize)
                GenerateCache(textData, link.Size);

            // If there is no cache, skip.
            if (textData.LinesCache == null) return;

            // Start drawing to the render target.
            Core.Context.Ink.Start(DrawLocation.Screen);

            float offsetY = 0;
            int currentChar = 0;
            float firstLineJustifiedCenterOffset = 0;

            for (int i = 0; i < textData.LinesCache.Count; i++)
            {
                // Determine X offset based on the selected style.
                //---------------------------------------------------------------------------------------
                int wordSpacing = 0;
                float offsetX;
                switch (textData.Style)
                {
                    case TextStyle.Right:
                        offsetX = textData.LinesCache[i].SpaceLeft;
                        break;
                    case TextStyle.Center:
                        offsetX = textData.LinesCache[i].SpaceLeft / 2;
                        break;
                    case TextStyle.JustifiedCenter:
                    case TextStyle.Justified:
                        float spaces = textData.LinesCache[i].Chars.Select(x => x.Content).Count(x => x == " ");
                        float currentBoost = wordSpacing * spaces;
                        float nextBoost = (wordSpacing + 1) * spaces;
                        // If manually going to a new line then don't apply justification.
                        if (!textData.LinesCache[i].Manual && i != textData.LinesCache.Count - 1)
                            if (spaces != 0)
                                while (textData.LinesCache[i].SpaceLeft - currentBoost >= 0 &&
                                       textData.LinesCache[i].SpaceLeft - nextBoost >= 0)
                                {
                                    wordSpacing += 1;
                                    currentBoost = wordSpacing * spaces;
                                    nextBoost = (wordSpacing + 1) * spaces;
                                }

                        // If this is the first line set the first line justification.
                        if (i == 0)
                            firstLineJustifiedCenterOffset = (textData.LinesCache[0].SpaceLeft - currentBoost) / 2;
                        offsetX = textData.Style == TextStyle.JustifiedCenter ? firstLineJustifiedCenterOffset : 0;
                        break;
                    default:
                        offsetX = 0;
                        break;
                }
                //---------------------------------------------------------------------------------------

                // Each character.
                foreach (CharData cachedChar in textData.LinesCache[i].Chars)
                {
                    //Check if trying to draw past limit.
                    if (currentChar == textData.DrawLimit) break;

                    //Each character effect.
                    foreach (Tag tag in cachedChar.Tags)
                    {
                        tag.Effect(cachedChar, new DrawData(offsetX, offsetY));
                    }

                    // Draw the character.
                    Core.Context.Ink.DrawString(textData.Font, cachedChar.Content,
                        new Vector2(link.X + offsetX, link.Y + offsetY), cachedChar.Color * 1f);

                    // Increment character counter and add its width to the offset.
                    currentChar++;
                    offsetX += StringWidth(cachedChar.Content, textData.Font);

                    if (cachedChar.Content == " ") offsetX += wordSpacing;
                }

                // Move the y offset to draw on a new line, and check if there is space for a new line.
                if (offsetY + StringHeight(" ", textData.Font) + StringHeight(" ", textData.Font) * 0.5 >= link.Height)
                    break;
                offsetY += StringHeight(" ", textData.Font);
            }

            // Finish drawing on the render target.
            Core.Context.Ink.Stop();
        }
    }
}