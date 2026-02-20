using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace AirThermoMod.MNGui.Util {
    public static class CairoFontExtension {
        /// <summary>
        /// Improved AutoFontSize: Roughly calculates with AutoFontSize, then searches actual fitting size
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <param name="onlyShrink"></param>
        public static void AutoFontSizeMN(this CairoFont font, string text, ElementBounds bounds, bool onlyShrink = true) {
            var origSize = font.UnscaledFontsize;
            font.AutoFontSize(text, bounds, onlyShrink);
            var margin = 1;

            // Might be able to (1.0 / GuiScale), since font size should work as an integer (after scaling,) but this should work
            var searchResolution = 0.5;

            for (int i = 0; i < 100; i++) {
                if (font.UnscaledFontsize < 0) {
                    font.UnscaledFontsize = 1.0;
                    break;
                }

                if (font.GetTextExtents(text).Width > (bounds.InnerWidth - margin)) {
                    font.UnscaledFontsize -= searchResolution;
                }
                else if (i == 0) {
                    // Font size might be bigger
                    for (int j = 0; j < 100; j++) {
                        if (font.GetTextExtents(text).Width <= (bounds.InnerWidth - margin)) {
                            font.UnscaledFontsize += searchResolution;
                        }
                        else {
                            break;
                        }
                    }
                    font.UnscaledFontsize -= searchResolution;

                    break;
                }
                else {
                    break;
                }
            }

            if (onlyShrink) {
                font.UnscaledFontsize = Math.Min(origSize, font.UnscaledFontsize);
            }
        }
    }
}
