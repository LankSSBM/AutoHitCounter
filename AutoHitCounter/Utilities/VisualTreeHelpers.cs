// 

using System.Windows;
using System.Windows.Media;

namespace AutoHitCounter.Utilities;

public static class VisualTreeHelpers
{
    public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
                return target;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    public static T FindDescendant<T>(DependencyObject parent, string name = null) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed && (name == null || (child is FrameworkElement fe && fe.Name == name)))
                return typed;
            var result = FindDescendant<T>(child, name);
            if (result != null) return result;
        }

        return null;
    }
}