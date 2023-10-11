using System.Collections;

namespace Olive.Microservices.Hub
{
    public class ColourPalette
    {
        static readonly string[] Palette = new[] { "#CC3542", "#FA7902", "#FAB320", "#2CC6D2", "#0A98CF", "#065280" };

        static Stack UsageTracker = new Stack(Palette);

        public static string Reset()
        {
            UsageTracker.Clear();
            return null;
        }

        public static string GetColourCode()
        {
            lock (Palette)
            {
                if (UsageTracker.Count <= 0)
                    foreach (var c in Palette)
                        UsageTracker.Push(c);

                return UsageTracker.Pop()?.ToString()?? Palette[0];
            }
        }
    }
}