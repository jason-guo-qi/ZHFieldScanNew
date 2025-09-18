using FieldScanNew.Infrastructure;

namespace FieldScanNew.ViewModels
{
    // 第3个栏目: 扫描高级设置
    public class ScanSettingsViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "3. 高级扫描设置";

        // 未来可以在这里添加磁场方向、RBW、Ref Level等属性
    }
}