// 

using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DSR.DSRVersion;

namespace AutoHitCounter.Games.DSR;

public static class DSROffsets
{
    private static DSRVersion? _version;
    
    public static DSRVersion Version => _version
                                        ?? Version1_0_3_0;
    
    public static void Initialize(long fileSize, nint moduleBase)
    {
        _version = fileSize switch
        {
            74186240 => Version1_0_1_0,
            75245056 => Version1_0_1_1,
            56756736 => Version1_0_1_2,
            57067008 => Version1_0_3_0,
            50286344 => Version1_0_3_1,
            _ => null
        };

        if (!_version.HasValue)
        {
            MsgBox.Show(
                $@"Unknown patch version: {_version}, please report it on GitHub",
                "Unknown patch version");
            return;
        }


        InitializeBaseAddresses(moduleBase);
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
     
    }
}