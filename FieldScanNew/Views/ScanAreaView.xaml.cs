using FieldScanNew.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using UserControl = System.Windows.Controls.UserControl;
using Point = System.Windows.Point;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace FieldScanNew.Views
{
    public partial class ScanAreaView : UserControl
    {
        private bool _isDragging = false;
        private Point _startPoint;

        public ScanAreaView()
        {
            InitializeComponent();

            // **核心修正：添加 Loaded 事件监听，每次显示时刷新图片**
            this.Loaded += ScanAreaView_Loaded;
        }

        private void ScanAreaView_Loaded(object sender, RoutedEventArgs e)
        {
            // 当视图加载时，通知 ViewModel 重新读取图片路径
            // 这样就能确保显示刚刚在“步骤5”里拍的新照片
            if (DataContext is ScanAreaViewModel vm)
            {
                vm.ReloadImage();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var grid = sender as Grid;
                _startPoint = e.GetPosition(grid);
                _isDragging = true;

                SelectionRect.Width = 0;
                SelectionRect.Height = 0;
                Canvas.SetLeft(SelectionRect, _startPoint.X);
                Canvas.SetTop(SelectionRect, _startPoint.Y);
                SelectionRect.Visibility = Visibility.Visible;

                grid?.CaptureMouse();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var grid = sender as Grid;
                Point currentPoint = e.GetPosition(grid);

                double x = Math.Min(currentPoint.X, _startPoint.X);
                double y = Math.Min(currentPoint.Y, _startPoint.Y);
                double w = Math.Abs(currentPoint.X - _startPoint.X);
                double h = Math.Abs(currentPoint.Y - _startPoint.Y);

                Canvas.SetLeft(SelectionRect, x);
                Canvas.SetTop(SelectionRect, y);
                SelectionRect.Width = w;
                SelectionRect.Height = h;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                var grid = sender as Grid;
                grid?.ReleaseMouseCapture();

                double x = Canvas.GetLeft(SelectionRect);
                double y = Canvas.GetTop(SelectionRect);
                double w = SelectionRect.Width;
                double h = SelectionRect.Height;

                if (w > 5 && h > 5)
                {
                    if (DataContext is ScanAreaViewModel vm)
                    {
                        vm.UpdateScanAreaFromSelection(new Rect(x, y, w, h));
                    }
                }
            }
        }
    }
}