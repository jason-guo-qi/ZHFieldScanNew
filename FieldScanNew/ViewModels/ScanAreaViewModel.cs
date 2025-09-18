using FieldScanNew.Infrastructure;

namespace FieldScanNew.ViewModels
{
    // 第6个栏目: 扫描区域配置
    public class ScanAreaViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "6. 扫描区域配置";

        // 未来可以在这里添加扫描点数、与XY校准联动的框选逻辑
    }
}