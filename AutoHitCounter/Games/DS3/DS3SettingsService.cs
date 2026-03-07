// 

using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3SettingsService(IMemoryService memoryService)
{
    public void ToggleNoLogo(bool isEnabled)
    {
        if (isEnabled)
        {
            memoryService.WriteBytes(Patches.NoLogo,
            [
                0x48, 0x31, 0xC0, 0x48, 0x89, 0x02, 0x49, 0x89, 0x04, 0x24, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90
            ]);
        }
        else
        {
            byte[] bytes =
            [
                0xE8, 0x00, 0x00, 0x00, 0x00,     
                0x90,                             
                0x4D, 0x8B, 0xC7,                 
                0x49, 0x8B, 0xD4,                 
                0x48, 0x8B, 0xC8,                 
                0xE8, 0x00, 0x00, 0x00, 0x00      
            ];
            
           AsmHelper.WriteRelativeOffsets(bytes, [
           (Patches.NoLogo, Functions.OriginalLogoFunc, 5, 1),
           (Patches.NoLogo + 0xF, Functions.OriginalLogoFunc, 5, 0xF + 1),
           
           ]);

            memoryService.WriteBytes(Patches.NoLogo, bytes);
            
        }
    }

    public void ToggleStutterFix(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(UserInputManager.Base) + UserInputManager.SteamInputEnum,
            isEnabled);
}