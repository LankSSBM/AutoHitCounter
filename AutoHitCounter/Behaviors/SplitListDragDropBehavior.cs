// 

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Behaviors;

public static class SplitListDragDropBehavior
{
    private const string DragDataFormat = "SplitListItem";
    private const double ScrollZone = 0.95;
    private const double ScrollSpeed = 0.05;

    private static System.Windows.Point _dragStartPoint;
    private static bool _isDragging;

    private static InsertionAdorner _currentAdorner;
    private static ListBoxItem _lastAdornedItem;
    private static bool _lastDrawAbove;

    private static DispatcherTimer _scrollTimer;
    private static ListBox _activeScrollListBox;
    private static double _scrollVelocity;

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SplitListDragDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if ((bool)e.NewValue)
        {
            listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove += OnPreviewMouseMove;
            listBox.DragOver += OnDragOver;
            listBox.DragLeave += OnDragLeave;
            listBox.Drop += OnDrop;
            listBox.AllowDrop = true;
        }
        else
        {
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove -= OnPreviewMouseMove;
            listBox.DragOver -= OnDragOver;
            listBox.DragLeave -= OnDragLeave;
            listBox.Drop -= OnDrop;
            listBox.AllowDrop = false;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBox) return;
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.OriginalSource is TextBox) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var diff = _dragStartPoint - e.GetPosition(null);

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (_isDragging) return;

        var listBoxItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (listBoxItem?.DataContext == null) return;

        if (sender is not ListBox listBox) return;

        _isDragging = true;
        listBox.QueryContinueDrag += OnQueryContinueDrag;
        var data = new DataObject(DragDataFormat, listBoxItem.DataContext);
        DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
        listBox.QueryContinueDrag -= OnQueryContinueDrag;
        _isDragging = false;
        StopScrollTimer();
    }

    private static void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        if (sender is not ListBox listBox) return;

        GetCursorPos(out var screenPoint);
        var wpfPoint = listBox.PointFromScreen(new System.Windows.Point(screenPoint.X, screenPoint.Y));

        UpdateScrollVelocity(listBox, wpfPoint);
        StartScrollTimer(listBox);
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DragDataFormat))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        if (targetItem == null)
        {
            RemoveAdorner();
            return;
        }

        var pos = e.GetPosition(targetItem);
        var drawAbove = pos.Y <= targetItem.ActualHeight / 2;

        if (targetItem != _lastAdornedItem || drawAbove != _lastDrawAbove)
        {
            RemoveAdorner();
            ShowAdorner(targetItem, drawAbove);
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        RemoveAdorner();
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        StopScrollTimer();
        RemoveAdorner();

        if (!e.Data.GetDataPresent(DragDataFormat)) return;

        var listBox = sender as ListBox;
        var droppedItem = e.Data.GetData(DragDataFormat);

        if (droppedItem == null || listBox == null) return;

        if (listBox.DataContext is not IReorderHandler handler) return;

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        int dropIndex;

        if (targetItem != null)
        {
            var targetData = targetItem.DataContext;
            dropIndex = listBox.Items.IndexOf(targetData);

            var pos = e.GetPosition(targetItem);
            if (pos.Y > targetItem.ActualHeight / 2)
                dropIndex++;
        }
        else
        {
            dropIndex = listBox.Items.Count;
        }

        handler.MoveItem(droppedItem, dropIndex);
    }

    private static void StartScrollTimer(ListBox listBox)
    {
        _activeScrollListBox = listBox;
        if (_scrollTimer != null) return;

        _scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _scrollTimer.Tick += OnScrollTick;
        _scrollTimer.Start();
    }

    private static void StopScrollTimer()
    {
        _scrollTimer?.Stop();
        _scrollTimer = null;
        _activeScrollListBox = null;
        _scrollVelocity = 0;
    }

    private static void OnScrollTick(object sender, EventArgs e)
    {
        if (_activeScrollListBox == null || _scrollVelocity == 0) return;
        var scrollViewer = FindScrollViewer(_activeScrollListBox);
        scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset + _scrollVelocity);
    }

    private static void UpdateScrollVelocity(ListBox listBox, System.Windows.Point positionInListBox)
    {
        var scrollViewer = FindScrollViewer(listBox);
        if (scrollViewer == null) return;

        var pos = listBox.TranslatePoint(positionInListBox, scrollViewer);
        var height = scrollViewer.ActualHeight;

        if (pos.Y < ScrollZone)
            _scrollVelocity = -ScrollSpeed * (1 - pos.Y / ScrollZone);
        else if (pos.Y > height - ScrollZone)
            _scrollVelocity = ScrollSpeed * (1 - (height - pos.Y) / ScrollZone);
        else
            _scrollVelocity = 0;
    }

    private static ScrollViewer FindScrollViewer(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer sv) return sv;
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }

        return null;
    }

    private static void ShowAdorner(ListBoxItem item, bool drawAbove)
    {
        var layer = AdornerLayer.GetAdornerLayer(item);
        if (layer == null) return;

        _currentAdorner = new InsertionAdorner(item, drawAbove);
        _lastAdornedItem = item;
        _lastDrawAbove = drawAbove;
        layer.Add(_currentAdorner);
    }

    private static void RemoveAdorner()
    {
        if (_currentAdorner != null && _lastAdornedItem != null)
        {
            var layer = AdornerLayer.GetAdornerLayer(_lastAdornedItem);
            layer?.Remove(_currentAdorner);
        }

        _currentAdorner = null;
        _lastAdornedItem = null;
    }
}