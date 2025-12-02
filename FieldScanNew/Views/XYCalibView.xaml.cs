using FieldScanNew.ViewModels;
using System.Windows.Input;

// =========================================================================
// **核心修正：添加别名，明确告诉编译器我们这里全部使用 WPF 版本的控件**
// =========================================================================
using UserControl = System.Windows.Controls.UserControl;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace FieldScanNew.Views
{
    public partial class XYCalibView : UserControl
    {
        public XYCalibView()
        {
            InitializeComponent();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 这里的 Image 现在明确指向 System.Windows.Controls.Image
            var image = sender as Image;
            if (image == null || image.Source == null) return;

            // 获取点击位置相对于 Image 控件的坐标
            // 这里的 Point 现在明确指向 System.Windows.Point
            Point clickPoint = e.GetPosition(image);

            if (DataContext is XYCalibViewModel vm)
            {
                vm.HandleImageClick(clickPoint);
            }
        }
    }
}