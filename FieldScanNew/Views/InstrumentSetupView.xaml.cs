using System.Windows.Controls; // 需要引用UserControl所在的命名空间

namespace FieldScanNew.Views
{
    /// <summary>
    /// InstrumentSetupView.xaml 的交互逻辑
    /// </summary>
    public partial class InstrumentSetupView : System.Windows.Controls.UserControl // <-- 核心修正：明确继承自 UserControl
    {
        public InstrumentSetupView()
        {
            InitializeComponent();
        }
    }
}