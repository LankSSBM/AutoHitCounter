// 

using static AutoHitCounter.Games.DSR.DSRVersion;

namespace AutoHitCounter.Games.DSR;

public static class DSROriginalBytes
{
    public static class KillChr
    {
        public static byte[] GetOriginal() => DSROffsets.Version switch
        {
            Version1_0_1_0 or Version1_0_1_1 or Version1_0_3_0 => [0x48, 0x8D, 0x64, 0x24, 0xF8],
            Version1_0_1_2 or Version1_0_3_1 => [0x48, 0x89, 0x5C, 0x24, 0xF8],
            _ => []
        };
    }
    
}