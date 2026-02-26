// 

using System.Windows;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly OverlayServerService _overlayServerService;

    public SettingsViewModel(IStateService stateService, OverlayServerService overlayServerService)
    {
        _overlayServerService = overlayServerService;
        stateService.Subscribe(State.AppStart, OnAppStart);
    }

    
    #region Properties
    
    private bool _isAlwaysOnTopEnabled;

    public bool IsAlwaysOnTopEnabled
    {
        get => _isAlwaysOnTopEnabled;
        set
        {
            if (!SetProperty(ref _isAlwaysOnTopEnabled, value)) return;
            SettingsManager.Default.AlwaysOnTop = value;
            SettingsManager.Default.Save();
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null) mainWindow.Topmost = _isAlwaysOnTopEnabled;
        }
    }
    
    private bool _isShowNotesEnabled;

    public bool IsShowNotesEnabled
    {
        get => _isShowNotesEnabled;
        set
        {
            if (!SetProperty(ref _isShowNotesEnabled, value)) return;
            SettingsManager.Default.ShowNotesSection = value;
            SettingsManager.Default.Save();
        }
    }
    
    private bool _allowManualSplitOnAutoSplits;

    public bool AllowManualSplitOnAutoSplits
    {
        get => _allowManualSplitOnAutoSplits;
        set
        {
            if (!SetProperty(ref _allowManualSplitOnAutoSplits, value)) return;
            SettingsManager.Default.AllowManualSplitOnAutoSplits = value;
            SettingsManager.Default.Save();
        }
    }

    private bool _isPracticeMode;

    public bool IsPracticeMode
    {
        get => _isPracticeMode;
        set
        {
            if (!SetProperty(ref _isPracticeMode, value)) return;
            SettingsManager.Default.PracticeMode = value;
            SettingsManager.Default.Save();
        }
    }

    private int _maxSplits;

    public int MaxSplits
    {
        get => _maxSplits;
        set
        {
            if (!SetProperty(ref _maxSplits, value)) return;
            SettingsManager.Default.MaxSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private int _prevSplits;

    public int PrevSplits
    {
        get => _prevSplits;
        set
        {
            if (!SetProperty(ref _prevSplits, value)) return;
            SettingsManager.Default.PrevSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private int _nextSplits;

    public int NextSplits
    {
        get => _nextSplits;
        set
        {
            if (!SetProperty(ref _nextSplits, value)) return;
            SettingsManager.Default.NextSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private bool _showDiff;

    public bool ShowDiff
    {
        get => _showDiff;
        set
        {
            if (!SetProperty(ref _showDiff, value)) return;
            SettingsManager.Default.ShowDiff = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }
    
    private bool _showPb;

    public bool ShowPb
    {
        get => _showPb;
        set
        {
            if (!SetProperty(ref _showPb, value)) return;
            SettingsManager.Default.ShowPb = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }
    
    private bool _showIgt;

    public bool ShowIgt
    {
        get => _showIgt;
        set
        {
            if (!SetProperty(ref _showIgt, value)) return;
            SettingsManager.Default.ShowIgt = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }
    
    
    
    #endregion


    #region Private Methods

    private void OnAppStart()
    {
        IsAlwaysOnTopEnabled = SettingsManager.Default.AlwaysOnTop;
        
        _isShowNotesEnabled = SettingsManager.Default.ShowNotesSection;
        OnPropertyChanged(nameof(IsShowNotesEnabled));

        _allowManualSplitOnAutoSplits = SettingsManager.Default.AllowManualSplitOnAutoSplits;
        OnPropertyChanged(nameof(AllowManualSplitOnAutoSplits));

        _isPracticeMode = SettingsManager.Default.PracticeMode;
        OnPropertyChanged(nameof(IsPracticeMode));
        
        LoadSplitConfig();
    }

    private void LoadSplitConfig()
    {
        _maxSplits = SettingsManager.Default.MaxSplits;
        OnPropertyChanged(nameof(MaxSplits));
        
        _prevSplits = SettingsManager.Default.PrevSplits;
        OnPropertyChanged(nameof(PrevSplits));
        
        _nextSplits = SettingsManager.Default.NextSplits;
        OnPropertyChanged(nameof(NextSplits));
        
        _showDiff = SettingsManager.Default.ShowDiff;
        OnPropertyChanged(nameof(ShowDiff));
        
        _showPb = SettingsManager.Default.ShowPb;
        OnPropertyChanged(nameof(ShowPb));
        
        _showIgt = SettingsManager.Default.ShowIgt;
        OnPropertyChanged(nameof(ShowIgt));
        
        BroadcastConfigChanged();
    }

    private void BroadcastConfigChanged()
    {
        var config = new OverlayConfig(MaxSplits, PrevSplits, NextSplits, ShowDiff, ShowPb, ShowIgt);
        _overlayServerService.BroadcastConfig(config);
    }

    #endregion
    
}