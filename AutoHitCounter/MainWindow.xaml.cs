using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (SettingsManager.Default.MainWindowLeft > 0)
                    Left = SettingsManager.Default.MainWindowLeft;

                if (SettingsManager.Default.MainWindowTop > 0)
                    Top = SettingsManager.Default.MainWindowTop;

                if (DataContext is MainViewModel vm)
                    vm.PropertyChanged += MainViewModel_PropertyChanged;
            };
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SettingsManager.Default.MainWindowLeft = Left;
            SettingsManager.Default.MainWindowTop = Top;
            SettingsManager.Default.Save();
        }

        private void Notes_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SaveNotesCommand.Execute(null);
        }

        private void SplitList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            HeaderScrollSpacer.Width = e.ExtentHeight > e.ViewportHeight
                ? new GridLength(SystemParameters.VerticalScrollBarWidth)
                : new GridLength(0);
        }

        private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SplitItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { DataContext: SplitViewModel split }) return;
            if (split.IsParent) return;

            var vm = (MainViewModel)DataContext;

            if (vm.IsUnlocked)
            {
                split.IsEditing = true;
            }
            else
            {
                var targetIndex = vm.Splits.IndexOf(split);
                var currentIndex = vm.CurrentSplit != null ? vm.Splits.IndexOf(vm.CurrentSplit) : -1;

                if (targetIndex < 0 || targetIndex == currentIndex) return;

                if (targetIndex > currentIndex)
                {
                    // Step forward until split is reached
                    while (vm.CurrentSplit != split && !vm.IsRunComplete)
                        vm.AdvanceSplitCommand.Execute(null);
                }
                else
                {
                    // Step backward until split is reached
                    while (vm.CurrentSplit != split)
                        vm.PrevSplitCommand.Execute(null);
                }
            }

            e.Handled = true;
        }

        private void RenameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox { DataContext: SplitViewModel split }) return;

            if (e.Key == Key.Enter)
            {
                ((MainViewModel)DataContext).CommitRename(split);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                split.IsEditing = false;
                e.Handled = true;
            }
        }

        private void RenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox { DataContext: SplitViewModel split })
                ((MainViewModel)DataContext).CommitRename(split);
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedSplit))
            {
                var vm = (MainViewModel)DataContext;
                if (vm.SelectedSplit != null)
                    vm.SelectedSplit.PropertyChanged += SelectedSplit_PropertyChanged;
            }
        }

        private void SelectedSplit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SplitViewModel.IsEditing)) return;
            if (sender is not SplitViewModel { IsEditing: true } split) return;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
            {
                var container = SplitListBox.ItemContainerGenerator.ContainerFromItem(split) as ListBoxItem;
                if (container == null) return;
                var textBox = VisualTreeHelpers.FindDescendant<TextBox>(container, "RenameBox");
                textBox?.Focus();
                textBox?.SelectAll();
            });
        }

        private void ResetAttempts_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.CommitAttemptsEdit("0");
        }

        private void AttemptsBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (e.Key == Key.Enter)
                vm.CommitAttemptsEdit(((TextBox)sender).Text);
            else if (e.Key == Key.Escape)
                vm.CommitAttemptsEdit(vm.AttemptCount.ToString());
        }

        private void AttemptsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.CommitAttemptsEdit(((TextBox)sender).Text);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            var hit = e.OriginalSource as DependencyObject;
            if (DataContext is not MainViewModel vm) return;

            if (vm.IsEditingAttempts && hit != null && !IsDescendantOf(AttemptsBox, hit))
                vm.CommitAttemptsEdit(AttemptsBox.Text);

            var editingSplit = vm.Splits.FirstOrDefault(s => s.IsEditing);
            if (editingSplit != null)
            {
                var renameBox = FindRenameBox(SplitListBox, editingSplit);
                if (renameBox == null || (hit != null && !IsDescendantOf(renameBox, hit)))
                    vm.CommitRename(editingSplit);
            }
        }

        private static bool IsDescendantOf(DependencyObject parent, DependencyObject child)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent) return true;
                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static TextBox FindRenameBox(DependencyObject parent, object dataContext)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBox tb && tb.Name == "RenameBox" && tb.DataContext == dataContext)
                    return tb;
                var result = FindRenameBox(child, dataContext);
                if (result != null) return result;
            }

            return null;
        }
        
        
    }
}