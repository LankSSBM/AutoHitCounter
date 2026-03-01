// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    private EldenRingHitService _hitService;
    private EldenRingEventService _eventService;
    public string GameVersion => EldenRingOffsets.Version switch
{
    EldenRingVersion.Version1_2_0 => "1.02",
    EldenRingVersion.Version1_2_1 => "1.2.1",
    EldenRingVersion.Version1_2_2 => "1.2.2",
    EldenRingVersion.Version1_2_3 => "1.2.3",
    EldenRingVersion.Version1_3_0 => "1.3.0",
    EldenRingVersion.Version1_3_1 => "1.3.1",
    EldenRingVersion.Version1_3_2 => "1.3.2",
    EldenRingVersion.Version1_4_0 => "1.4.0",
    EldenRingVersion.Version1_4_1 => "1.4.1",
    EldenRingVersion.Version1_5_0 => "1.5.0",
    EldenRingVersion.Version1_6_0 => "1.6.0",
    EldenRingVersion.Version1_7_0 => "1.7.0",
    EldenRingVersion.Version1_8_0 => "1.8.0",
    EldenRingVersion.Version1_8_1 => "1.8.1",
    EldenRingVersion.Version1_9_0 => "1.09",
    EldenRingVersion.Version1_9_1 => "1.09.1",
    EldenRingVersion.Version2_0_0 => "1.10",
    EldenRingVersion.Version2_0_1 => "1.10.1",
    EldenRingVersion.Version2_2_0 => "1.12.2",
    EldenRingVersion.Version2_2_3 => "1.12.4",
    EldenRingVersion.Version2_3_0 => "1.13.1",
    EldenRingVersion.Version2_4_0 => "1.14.1",
    EldenRingVersion.Version2_5_0 => "1.15",
    EldenRingVersion.Version2_6_0 => "1.16",
    EldenRingVersion.Version2_6_1 => "1.16.1",
    _ => "Unknown"
};

    private DateTime? _lastHit;

    public event Action<int> OnHit;

    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;

    private nint _igtPtr;

    public EldenRingModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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

        EldenRingCustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();
#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)EldenRingCustomCodeOffsets.Base:X}");
#endif

        _hitService = new EldenRingHitService(_memoryService, _hookManager);
        _eventService = new EldenRingEventService(_memoryService, _hookManager, _events);
        _eventService.InstallHook();
        _hitService.InstallHooks();
        _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        _tickService.RegisterGameTick(Tick);
    }

    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        EldenRingOffsets.Initialize(fileVersion, moduleBase);
    }

    private void Tick()
    {
        if (!IsLoaded()) return;

        if (_hitService.HasHit() && (_lastHit == null || (DateTime.Now - _lastHit.Value).TotalSeconds > 3))
        {
            OnHit?.Invoke(1);
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        OnIgtChanged?.Invoke(_memoryService.Read<long>(_igtPtr));
    }

    private bool IsLoaded()
    {
        var worldChrman = _memoryService.Read<nint>(WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrman + WorldChrMan.PlayerIns) != 0;
    }

    public void Dispose()
    {
        _stateService.Unsubscribe(State.Attached, Initialize);
        _tickService.UnregisterGameTick();
        OnHit = null;
        OnEventSet = null;
        OnIgtChanged = null;
    }
}