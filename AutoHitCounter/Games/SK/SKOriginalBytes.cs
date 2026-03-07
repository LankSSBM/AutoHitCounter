// 

using static AutoHitCounter.Games.SK.SKOffsets;
using static AutoHitCounter.Games.SK.SKVersion;

namespace AutoHitCounter.Games.SK;

public static class SKOriginalBytes
{
    public static class ShowSmallHintBox
    {
        public static byte[] GetOriginal() => Version switch
        {
            Version1_2_0 => [0xE8, 0xD8, 0x91, 0x4E, 0x00],
            Version1_3_0 or Version1_4_0 => [0xE8, 0x08, 0x96, 0x4E, 0x00],
            Version1_5_0 => [ 0xE8, 0xC8, 0xBB, 0x50, 0x00],
            Version1_6_0 => [0xE8, 0x38, 0xBF, 0x50, 0x00]
        };
    }
    
    public static class ShowTutorialText
    {
        public static byte[] GetOriginal() => Version switch
        {
            Version1_2_0 => [0xE8, 0x88, 0x92, 0x4E, 0x00],
            Version1_3_0 or Version1_4_0  => [0xE8, 0xB8, 0x96, 0x4E, 0x00],
            Version1_5_0 => [0xE8, 0x78, 0xBC, 0x50, 0x00],
            Version1_6_0 => [0xE8, 0xE8, 0xBF, 0x50, 0x00]
           
        };
    }
}