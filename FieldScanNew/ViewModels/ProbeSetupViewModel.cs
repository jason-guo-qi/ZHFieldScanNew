using FieldScanNew.Infrastructure;

namespace FieldScanNew.ViewModels
{
    // 第2个栏目: 探头配置
    public class ProbeSetupViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "2. 探头配置";

        // 未来可以在这里添加探头列表、校准因子等属性
    }
}