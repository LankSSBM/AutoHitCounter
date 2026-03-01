// 

using System;
using System.Collections.Generic;
using System.IO;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DSR;

public class DSRModule : IGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    
    private DateTime? _lastHit;
    private nint _igtPtr;
    private DSRHitService _hitService;
    private DSREventService _eventService;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
    
    public DSRModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, string> events)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        _events = events;
        
        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }
    
    private void Initialize()
    {
        InitializeOffsets();

//         SKCustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();
//         
// #if DEBUG
//         Console.WriteLine($@"Code cave: 0x{(long)SKCustomCodeOffsets.Base:X}");
// #endif
//
//         _hitService = new SKHitService(_memoryService, _hookManager);
//         _eventService = new SKEventService(_memoryService, _hookManager, _events);
//         // _eventService.InstallHook();
//         _hitService.InstallHooks();
//         // _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
//         _tickService.RegisterGameTick(Tick);
    }
    
    private void InitializeOffsets()
    {
        var module = _memoryService.TargetProcess?.MainModule;
        if (module == null) return;
        var fileInfo = new FileInfo(module.FileName);
        var fileSize = fileInfo.Length;
        var moduleBase = _memoryService.BaseAddress;
        DSROffsets.Initialize(fileSize, moduleBase);
    }
}