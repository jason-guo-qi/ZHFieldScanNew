using FieldScanNew.Infrastructure;

namespace FieldScanNew.ViewModels
{
    // 第5个栏目: XY平面校准
    public class XYCalibViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "5. XY平面校准";

        // 未来可以在这里添加加载图片、框选、记录坐标等命令
    }
}