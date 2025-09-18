using FieldScanNew.ViewModels;
using FieldScanNew.Views;
using System.Windows;

namespace FieldScanNew.Services
{
    public class DialogService : IDialogService
    {
        public void ShowDialog(IStepViewModel viewModel)
        {
            var dialog = new DialogWindow
            {
                DataContext = viewModel
            };

            // 根据ViewModel的类型，设置窗口的标题
            if (viewModel is InstrumentSetupViewModel)
                dialog.Title = "1. 仪器连接设置";
            else if (viewModel is ProbeSetupViewModel)
                dialog.Title = "2. 探头配置";
            else if (viewModel is ScanSettingsViewModel)
                dialog.Title = "3. 高级扫描设置";
            else if (viewModel is ZCalibViewModel)
                dialog.Title = "4. Z轴校准";
            else if (viewModel is XYCalibViewModel)
                dialog.Title = "5. XY平面校准";
            else if (viewModel is ScanAreaViewModel)
                dialog.Title = "6. 扫描区域配置";

            // **核心修正：已经删除掉原来引用 StepViewModel 的那一行多余代码**

            dialog.ShowDialog();
        }
    }
}