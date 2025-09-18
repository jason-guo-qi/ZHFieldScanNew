using FieldScanNew.Infrastructure;

namespace FieldScanNew.ViewModels
{
    // 第4个栏目: Z坐标校准
    public class ZCalibViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "4. Z轴校准";

        // 未来可以在这里添加手动控制按钮对应的命令和Z高度属性
    }
}