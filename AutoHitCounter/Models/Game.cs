//

using AutoHitCounter.Enums;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Models
{
    public class Game
    {
        public GameTitle Title { get; set; }
        public string GameName => Title.GetDescription();
        public string ProcessName { get; set; }
        public bool IsEventLogSupported { get; set; }
    }
}