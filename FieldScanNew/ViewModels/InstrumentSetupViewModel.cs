using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    // 第1个栏目: 仪器设置
    public class InstrumentSetupViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "1. 仪器连接";

        // 此处应包含仪器型号、IP地址等属性
        private InstrumentSettings _instrumentSettings = new InstrumentSettings();
        public InstrumentSettings InstrumentSettings
        {
            get => _instrumentSettings;
            set { _instrumentSettings = value; OnPropertyChanged(); }
        }

        // 可以在这里添加“连接”、“断开”等命令
        public ICommand ConnectCommand { get; }

        public InstrumentSetupViewModel()
        {
            // ConnectCommand = new RelayCommand(...);
        }
    }

    // 后续可以按照这个模板创建其他5个ViewModel
    // public class ProbeSetupViewModel : ViewModelBase, IStepViewModel { ... }
    // public class ScanSettingsViewModel : ViewModelBase, IStepViewModel { ... }
    // ...等等
}