//

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using NSubstitute;
using Xunit;

namespace AutoHitCounter.Tests.Services;

public class GameSessionOrchestratorTests
{
    private readonly IMemoryService _memoryService = Substitute.For<IMemoryService>();
    private readonly IHotkeyManager _hotkeyManager = Substitute.For<IHotkeyManager>();
    private readonly IGameModuleFactory _factory = Substitute.For<IGameModuleFactory>();
    private readonly StateService _stateService = new();
    private readonly GameSessionOrchestrator _sut;

    public GameSessionOrchestratorTests()
    {
        _sut = new GameSessionOrchestrator(_memoryService, _hotkeyManager, _factory, _stateService);
        _sut.Initialize(
            Substitute.For<IHitRulesProvider>(),
            () => new Dictionary<uint, (string Name, int Required, int Hit)>());
    }

    [Fact]
    public void Track_CreatesModuleViaFactory()
    {
        var game = new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false };
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(Substitute.For<IGameModule>());

        _sut.Track(game);

        _factory.Received(1).CreateModule(game, Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>());
    }

    [Fact]
    public void Track_WhenModuleFiresOnHit_HitReceivedIsForwarded()
    {
        var module = Substitute.For<IGameModule>();
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(module);
        _sut.Track(new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false });

        var fired = false;
        _sut.HitReceived += () => fired = true;
        module.OnHit += Raise.Event<Action>();

        Assert.True(fired);
    }

    [Fact]
    public void Track_ManualGame_MarksAttachedAndSkipsAutoAttach()
    {
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(Substitute.For<IGameModule>());
        var game = new Game { GameName = "MyCustomGame", ProcessName = "", IsManual = true };

        var attachmentChangedCount = 0;
        _sut.AttachmentChanged += () => attachmentChangedCount++;

        _sut.Track(game);

        Assert.True(_sut.IsAttached);
        Assert.Equal("Custom Game: MyCustomGame", _sut.AttachedText);
        Assert.Equal(1, attachmentChangedCount);
        _hotkeyManager.Received(1).SetManualGameActive(true);
        _memoryService.DidNotReceive().StartAutoAttach(Arg.Any<string>());
    }

    [Fact]
    public void Track_WhenCalledAgain_DisposesPreviousModule()
    {
        var firstModule = Substitute.For<IGameModule, IDisposable>();
        var secondModule = Substitute.For<IGameModule, IDisposable>();
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(firstModule, secondModule);

        _sut.Track(new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false });
        _sut.Track(new Game { GameName = "DS2", ProcessName = "darksoulsii", IsManual = false });

        ((IDisposable)firstModule).Received(1).Dispose();
    }

    [Fact]
    public void StateAttached_UpdatesAttachmentAndFiresEvent()
    {
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(Substitute.For<IGameModule>());
        _sut.Track(new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false });

        var attachmentChangedCount = 0;
        _sut.AttachmentChanged += () => attachmentChangedCount++;

        _stateService.Publish(State.Attached);

        Assert.True(_sut.IsAttached);
        Assert.Equal("Attached to DS3", _sut.AttachedText);
        Assert.Equal(1, attachmentChangedCount);
    }

    [Fact]
    public void StateNotAttached_OnManualGame_IsNoOp()
    {
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(Substitute.For<IGameModule>());
        _sut.Track(new Game { GameName = "MyCustomGame", ProcessName = "", IsManual = true });

        var attachmentChangedCount = 0;
        _sut.AttachmentChanged += () => attachmentChangedCount++;

        _stateService.Publish(State.NotAttached);

        Assert.True(_sut.IsAttached);
        Assert.Equal("Custom Game: MyCustomGame", _sut.AttachedText);
        Assert.Equal(0, attachmentChangedCount);
    }

    [Fact]
    public void SetEventLogEnabled_PersistsAndAppliesToNextModule()
    {
        var module = Substitute.For<IGameModule>();
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(module);

        _sut.SetEventLogEnabled(true);
        _sut.Track(new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false });

        module.Received().SetEventLogEnabled(true);
    }
}
