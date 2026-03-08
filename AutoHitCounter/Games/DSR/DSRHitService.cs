// 

using System;
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
        InstallKillChrHook();
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
        nint[] hooks = [Hooks.Hit, Hooks.ApplyHealthDelta];
        if (hooks.Any(h => memoryService.Read<byte>(h) != 0xE9))
            InstallHooks();
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
        var hit = Base + Hit;
        var envDeathFlag = Base + CheckEnvDeathFlag;
        var code = Base + HitCode;
        var originalBytes = DSROriginalBytes.Hit.GetOriginal();

        Array.Copy(originalBytes, 0, bytes, 0x9E, originalBytes.Length);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, envDeathFlag, 7, 2),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x8E, hit, 6, 0x8E + 2),
            (code + 0x96, envDeathFlag, 7, 0x96 + 2),
            (code + 0xA3, Hooks.Hit + 5, 5, 0xA3 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x6C, 0x24, 0x10]);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRApplyHealthDelta);
        var hit = Base + Hit;
        var envDeathFlag = Base + CheckEnvDeathFlag;
        var code = Base + ApplyHealthDelta;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x17, envDeathFlag, 7, 0x17 + 2),
            (code + 0x42, WorldChrMan.Base, 7, 0x42 + 3),
            (code + 0x54, hit, 6, 0x54 + 2),
            (code + 0x5B, Hooks.ApplyHealthDelta + 5, 5, 0x5B + 1),
        ]);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (FallDmgRetAddr, 0x6 + 2),
            (EnvDeathRetAddr, 0x20 + 2),
            (AuxDeathRetAddr, 0x31 + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x89, 0x7C, 0x24, 0x40]);
    }

    private void InstallKillChrHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRKillChr);
        var hit = Base + Hit;
        var code = Base + KillChr;
        var originalBytes = DSROriginalBytes.KillChr.GetOriginal();
        
        Array.Copy(originalBytes, 0, bytes, 0, originalBytes.Length);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, WorldChrMan.Base, 7, 0x6 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x1F, Hooks.KillChr + 5, 5, 0x1F + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.KillChr, originalBytes);
    }
}