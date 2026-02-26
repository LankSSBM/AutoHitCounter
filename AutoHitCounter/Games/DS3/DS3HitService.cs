// 

using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3CustomCodeOffsets;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3HitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    
    private const string Kernel32 = "kernel32.dll";
    private const string GetTickCount64 = "GetTickCount64";
    
    public void InstallHooks()
    {
        WritePlayerDeadCheck();
        WriteGetPlayerSpEffect();

        InstallHitHook();
        InstallLethalFallHook();
        InstallAuxHitHooks();
        InstallJailerDrainHook();
        InstallFallDamageHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void WritePlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void WriteGetPlayerSpEffect()
    {
        var code = Base + GetSpEffect;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3GetSpEffect);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3Hit);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
            (code + 0x7B, hit, 6, 0x7B + 2),
            (code + 0x8D, Hooks.Hit + 8, 5, 0x8D + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x83, 0xEC, 0x50, 0x48, 0x8B, 0x41, 0x08]);
    }

    private void InstallLethalFallHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3LethalFall);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + LethalFall;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x1F, FallDamageKillFloor, 8, 0x1F + 4),
            (code + 0x27, Hooks.LethalFall + 8, 5, 0x27 + 1)
        ]);
        
        var originalBytes = bytes.Skip(0x1F).Take(8).ToArray();
        
        memoryService.WriteBytes(code, bytes);
        
        hookManager.InstallHook(code, Hooks.LethalFall, originalBytes);
        
    }

    private void InstallAuxHitHooks()
    {
        var auxCheckFlag = Base + CheckAuxProcFlag;
        InstallCheckAuxAttackerHook(auxCheckFlag);
        InstallAuxProcHook(auxCheckFlag);
    }

    private void InstallCheckAuxAttackerHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckAuxAttacker);
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + CheckAuxAttacker;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x26, auxCheckFlag, 7, 0x26 + 2),
            (code + 0x2F, auxCheckFlag, 7, 0x2F + 2),
            (code + 0x3E, Hooks.CheckAuxAttacker + 7, 5, 0x3E + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckAuxAttacker, [0x49, 0x89, 0xE3, 0x49, 0x89, 0x4B, 0x08]);
    }

    private void InstallAuxProcHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3AuxProc);
        var hit = Base + Hit;
        var code = Base + AuxProc;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x9, auxCheckFlag, 7, 0x9 + 2),
            (code + 0x12, hit, 6, 0x12 + 2),
            (code + 0x18, Hooks.AuxProc + 9, 5, 0x18 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxProc, [0x41, 0x09, 0x42, 0x4C, 0x43, 0x8B, 0x4C, 0x9A, 0x24]);
        
    }

    private void InstallJailerDrainHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3JailerDrain);
        var getPlayerSpEffect = Base + GetSpEffect;
        var getTickCount = memoryService.GetProcAddress(Kernel32, GetTickCount64);
        var lastHitCountTime = Base + LastJailerCountTime;
        var hit = Base + Hit;
        var code = Base + JailerDrain;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, Hooks.HasJailerDrain + 6, 6, 2),
            (code + 0x16, getPlayerSpEffect, 5, 0x16 + 1),
            (code + 0x2E, lastHitCountTime, 7, 0x2E + 3),
            (code + 0x43, lastHitCountTime, 7, 0x43 + 3),
            (code + 0x4A, hit, 6, 0x4A + 2),
            (code + 0x53, Hooks.HasJailerDrain + 6, 5, 0x53 + 1)
        ]);
        
        AsmHelper.WriteAbsoluteAddress(bytes, getTickCount, 0x22 + 2);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.HasJailerDrain, [0x76, 0x04, 0xf3, 0x0f, 0x59, 0xf0]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3FallDamage);
        var hit = Base + Hit;
        var code = Base + FallDamage;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, WorldChrMan.Base, 7, 0x6 + 3),
            (code + 0x31, hit, 6, 0x31 + 2),
            (code + 0x38, Hooks.FallDamage + 5, 5, 0x38 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FallDamage, [0x41, 0x89, 0xC6, 0xF7, 0xDA]);
    }
}