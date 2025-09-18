using FieldScanNew.ViewModels;
using System.Windows;

namespace FieldScanNew // 根命名空间
{
    // 在这里添加关键字 "partial" 来连接 XAML 和 C# 代码
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // 现在这个方法可以被正确找到了

            // 使用正确的命名空间
            this.DataContext = new FieldScanNew.ViewModels.MainViewModel();
            // 添加这个事件处理器
            TaskTreeView.SelectedItemChanged += TaskTreeView_SelectedItemChanged;

        }
        private void TaskTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 将选中的项手动设置到ViewModel的属性上
            if (this.DataContext is MainViewModel vm && e.NewValue is IStepViewModel step)
            {
                vm.SelectedStep = step;
            }
        }

        // 下面的方法现在可以被 XAML 正确找到了
        public void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}