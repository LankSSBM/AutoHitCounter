// 

using AutoHitCounter.Interfaces;
using static AutoHitCounter.Games.SK.SKOffsets;

namespace AutoHitCounter.Games.SK;

public class SKSettingsService(IMemoryService memoryService)
{
    public void ToggleNoLogo(bool isEnabled) =>
        memoryService.WriteBytes(Patches.NoLogo, isEnabled ? [0xEB] : [0x74]);
    
    public void ToggleNoTutorials(bool isEnabled)
    {
        memoryService.WriteBytes(Patches.MenuTutorialSkip, isEnabled ? [0x90, 0x90, 0x90, 0x90] : [0x84, 0xC0, 0x75, 0x08]);
        memoryService.WriteBytes(Patches.ShowSmallHintBox,
            isEnabled ? [0x90, 0x90, 0x90, 0x90, 0x90] : SKOriginalBytes.ShowSmallHintBox.GetOriginal());
        memoryService.WriteBytes(Patches.ShowTutorialText,
            isEnabled ? [0x90, 0x90, 0x90, 0x90, 0x90] : SKOriginalBytes.ShowTutorialText.GetOriginal());
    }
}