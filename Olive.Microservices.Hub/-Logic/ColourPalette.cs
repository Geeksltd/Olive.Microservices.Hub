﻿using System.Collections;

namespace Domain
{
    public class ColourPalette
    {
        static readonly string[] Palette = new[] { "#CC3542", "#FA7902", "#FAB320", "#2CC6D2", "#0A98CF", "#065280" };
        //static readonly string[] Palette = new[] { "#3faaa9", "#3faaa9", "#3faaa9", "#3faaa9", "#3faaa9", "#3faaa9" };

        static Stack UsageTracker = new Stack(Palette);

        public static string GetColourCode()
        {
            lock (Palette)
            {
                if (UsageTracker.Count <= 0)
                    for (var i = 0; i < Palette.Length; i++)
                        UsageTracker.Push(Palette[i]);

                return UsageTracker.Pop().ToString();
            }
        }
    }
}