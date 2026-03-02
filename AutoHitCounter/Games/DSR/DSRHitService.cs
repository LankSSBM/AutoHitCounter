// 

using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DSR.DSRCustomCodeOffsets;
using static AutoHitCounter.Games.DSR.DSROffsets;

namespace AutoHitCounter.Games.DSR;

public class DSRHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    public void InstallHooks()
    {
        InstallHitHook();
        InstallApplyHealthDeltaHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    public void EnsureHooksInstalled()
    {
        nint[] hooks = [Hooks.Hit];
        if (hooks.Any(h => memoryService.Read<byte>(h) != 0xE9))
            InstallHooks();
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
        var hit = Base + Hit;
        var code = Base + HitCode;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x65, hit, 6, 0x65 + 2),
            (code + 0x71, Hooks.Hit + 5, 5, 0x71 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x6C, 0x24, 0x10]);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRApplyHealthDelta);
        var hit = Base + Hit;
        var code = Base + ApplyHealthDelta;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x28, WorldChrMan.Base, 7, 0x28 + 3),
            (code + 0x3A, hit, 6, 0x3A + 2),
            (code + 0x41, Hooks.ApplyHealthDelta + 5, 5, 0x41 + 1),
        ]);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (FallDmgRetAddr, 0x6 + 2),
            (AuxDeathRetAddr, 0x17 + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x89, 0x7C, 0x24, 0x40]);
    }
}