using Microsoft.Xna.Framework;

namespace TrexRunner.Entities
{
    public interface IDayNightCycle
    {
        public int NightCount { get; }
        public bool IsNight { get; }

        Color ClearColor { get;  }
    }
}
