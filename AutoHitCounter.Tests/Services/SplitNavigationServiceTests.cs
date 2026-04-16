using System.Collections.ObjectModel;
using AutoHitCounter.Enums;
using AutoHitCounter.Services;
using AutoHitCounter.ViewModels;
using Xunit;

namespace AutoHitCounter.Tests.Services;

public class SplitNavigationServiceTests
{
    private readonly SplitNavigationService _sut = new();

    private static SplitViewModel Child(string name = "Split") =>
        new() { Name = name, Type = SplitType.Child };

    private static SplitViewModel Parent(string name = "Group") =>
        new() { Name = name, Type = SplitType.Parent };

    private void LoadSplits(params SplitViewModel[] splits)
    {
        _sut.Load(new ObservableCollection<SplitViewModel>(splits));
    }

    #region SetPosition

    [Fact]
    public void SetPosition_SetsCurrentSplitAndIsRunComplete()
    {
        var split = Child();

        _sut.SetPosition(split, true);

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.True(_sut.IsRunComplete);
    }

    [Fact]
    public void SetPosition_SetsIsRunCompleteToFalse()
    {
        var split = Child();

        _sut.SetPosition(split, false);

        Assert.False(_sut.IsRunComplete);
    }

    [Fact]
    public void SetPosition_ClearsIsCurrentOnPreviousSplit()
    {
        var first = Child("First");
        var second = Child("Second");

        _sut.SetPosition(first, false);
        _sut.SetPosition(second, false);

        Assert.False(first.IsCurrent);
        Assert.True(second.IsCurrent);
    }

    [Fact]
    public void SetPosition_WithNull_ResetsToNoActiveSplit()
    {
        var split = Child();
        _sut.SetPosition(split, false);

        _sut.SetPosition(null, false);

        Assert.False(split.IsCurrent);
        Assert.Null(_sut.CurrentSplit);
    }

    [Fact]
    public void SetPosition_DoesNotFireStateChanged()
    {
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.SetPosition(Child(), false);

        Assert.False(fired);
    }

    #endregion

    #region InitFresh

    [Fact]
    public void InitFresh_SetsCurrentSplitToFirstChild()
    {
        LoadSplits(Child("First"), Child("Second"));

        _sut.InitFresh();

        Assert.Equal("First", _sut.CurrentSplit?.Name);
        Assert.True(_sut.CurrentSplit?.IsCurrent);
    }

    [Fact]
    public void InitFresh_SkipsParentSplits()
    {
        LoadSplits(Parent(), Child("First"));

        _sut.InitFresh();

        Assert.Equal("First", _sut.CurrentSplit?.Name);
    }

    [Fact]
    public void InitFresh_ResetsIsRunComplete()
    {
        LoadSplits(Child());
        _sut.SetPosition(null, true);

        _sut.InitFresh();

        Assert.False(_sut.IsRunComplete);
    }

    [Fact]
    public void InitFresh_ClearsIsCurrentOnPreviousSplit()
    {
        var second = Child("Second");
        LoadSplits(Child("First"), second);
        _sut.SetPosition(second, false);

        _sut.InitFresh();

        Assert.False(second.IsCurrent);
    }

    [Fact]
    public void InitFresh_WithNoChildren_SetsCurrentSplitToNull()
    {
        LoadSplits(Parent());

        _sut.InitFresh();

        Assert.Null(_sut.CurrentSplit);
    }

    [Fact]
    public void InitFresh_DoesNotFireStateChanged()
    {
        LoadSplits(Child());
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.InitFresh();

        Assert.False(fired);
    }

    #endregion

    #region Advance

    [Fact]
    public void Advance_MovesToNextChildSplit()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.InitFresh();

        _sut.Advance();

        Assert.Equal(second, _sut.CurrentSplit);
        Assert.True(second.IsCurrent);
        Assert.False(first.IsCurrent);
    }

    [Fact]
    public void Advance_SkipsParentSplits()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, Parent(), second);
        _sut.InitFresh();

        _sut.Advance();

        Assert.Equal(second, _sut.CurrentSplit);
    }

    [Fact]
    public void Advance_OnLastSplit_SetsIsRunComplete()
    {
        LoadSplits(Child());
        _sut.InitFresh();

        _sut.Advance();

        Assert.True(_sut.IsRunComplete);
    }

    [Fact]
    public void Advance_OnLastSplit_ClearsIsCurrent()
    {
        var only = Child();
        LoadSplits(only);
        _sut.InitFresh();

        _sut.Advance();

        Assert.False(only.IsCurrent);
    }

    [Fact]
    public void Advance_OnLastSplit_PreservesCurrentSplit()
    {
        var only = Child();
        LoadSplits(only);
        _sut.InitFresh();

        _sut.Advance();

        Assert.Equal(only, _sut.CurrentSplit);
    }

    [Fact]
    public void Advance_DoesNothingWhenIsRunComplete()
    {
        var split = Child();
        LoadSplits(split);
        _sut.SetPosition(split, true);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Advance();

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void Advance_DoesNothingWhenCurrentSplitIsNull()
    {
        LoadSplits(Child());
        _sut.SetPosition(null, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Advance();

        Assert.Null(_sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void Advance_FiresStateChanged()
    {
        LoadSplits(Child("First"), Child("Second"));
        _sut.InitFresh();
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Advance();

        Assert.True(fired);
    }

    [Fact]
    public void Advance_FiresStateChangedOnRunComplete()
    {
        LoadSplits(Child());
        _sut.InitFresh();
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Advance();

        Assert.True(fired);
    }

    #endregion

    #region Previous

    [Fact]
    public void Previous_MovesToPreviousChildSplit()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, false);

        _sut.Previous();

        Assert.Equal(first, _sut.CurrentSplit);
        Assert.True(first.IsCurrent);
        Assert.False(second.IsCurrent);
    }

    [Fact]
    public void Previous_SkipsParentSplits()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, Parent(), second);
        _sut.SetPosition(second, false);

        _sut.Previous();

        Assert.Equal(first, _sut.CurrentSplit);
    }

    [Fact]
    public void Previous_AtFirstSplit_DoesNothing()
    {
        var first = Child();
        LoadSplits(first);
        _sut.SetPosition(first, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Previous();

        Assert.Equal(first, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void Previous_DoesNothingWhenCurrentSplitIsNull()
    {
        LoadSplits(Child());
        _sut.SetPosition(null, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Previous();

        Assert.Null(_sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void Previous_FiresStateChanged()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Previous();

        Assert.True(fired);
    }

    [Fact]
    public void Previous_DoesNotFireStateChangedAtFirstSplit()
    {
        var first = Child();
        LoadSplits(first);
        _sut.SetPosition(first, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.Previous();

        Assert.False(fired);
    }

    [Fact]
    public void Previous_WhenRunComplete_NavigatesBack()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, true);

        _sut.Previous();

        Assert.Equal(first, _sut.CurrentSplit);
    }

    [Fact]
    public void Previous_WhenRunComplete_ResetsIsRunComplete()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, true);

        _sut.Previous();

        Assert.False(_sut.IsRunComplete);
    }

    #endregion

    #region JumpTo

    [Fact]
    public void JumpTo_WithNull_DoesNothing()
    {
        var split = Child();
        LoadSplits(split);
        _sut.SetPosition(split, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(null);

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void JumpTo_WithParent_DoesNothing()
    {
        var split = Child();
        var parent = Parent();
        LoadSplits(split, parent);
        _sut.SetPosition(split, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(parent);

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void JumpTo_ToCurrentSplit_DoesNothing()
    {
        var split = Child();
        LoadSplits(split);
        _sut.SetPosition(split, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(split);

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void JumpTo_ToSplitNotInList_DoesNothing()
    {
        var split = Child();
        var notInList = Child("NotInList");
        LoadSplits(split);
        _sut.SetPosition(split, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(notInList);

        Assert.Equal(split, _sut.CurrentSplit);
        Assert.False(fired);
    }

    [Fact]
    public void JumpTo_Forward_MovesToTarget()
    {
        var first = Child("First");
        var second = Child("Second");
        var third = Child("Third");
        LoadSplits(first, second, third);
        _sut.SetPosition(first, false);

        _sut.JumpTo(third);

        Assert.Equal(third, _sut.CurrentSplit);
    }

    [Fact]
    public void JumpTo_Forward_FiresStateChanged()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(first, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(second);

        Assert.True(fired);
    }

    [Fact]
    public void JumpTo_Backward_MovesToTarget()
    {
        var first = Child("First");
        var second = Child("Second");
        var third = Child("Third");
        LoadSplits(first, second, third);
        _sut.SetPosition(third, false);

        _sut.JumpTo(first);

        Assert.Equal(first, _sut.CurrentSplit);
    }

    [Fact]
    public void JumpTo_Backward_FiresStateChanged()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, false);
        var fired = false;
        _sut.StateChanged += () => fired = true;

        _sut.JumpTo(first);

        Assert.True(fired);
    }

    [Fact]
    public void JumpTo_Backward_ResetsIsRunComplete()
    {
        var first = Child("First");
        var second = Child("Second");
        LoadSplits(first, second);
        _sut.SetPosition(second, true);

        _sut.JumpTo(first);

        Assert.False(_sut.IsRunComplete);
    }

    #endregion
}
