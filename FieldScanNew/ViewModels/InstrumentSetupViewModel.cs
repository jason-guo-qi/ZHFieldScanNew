using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using FieldScanNew.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    public class InstrumentSetupViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "1. 仪器连接";
        private readonly HardwareService _hardwareService;

        // **核心修正：将只读属性改为可读写属性，并添加通知**
        private InstrumentSettings _instrumentSettings;
        public InstrumentSettings InstrumentSettings
        {
            get => _instrumentSettings;
            set
            {
                _instrumentSettings = value;
                OnPropertyChanged(); // 通知界面：设置对象变了
            }
        }

        private bool _isConnecting = false;
        public bool IsConnecting { get => _isConnecting; set { _isConnecting = value; OnPropertyChanged(); } }

        private string _robotStatus = "未连接";
        public string RobotStatus { get => _robotStatus; set { _robotStatus = value; OnPropertyChanged(); } }

        private string _saStatus = "未连接";
        public string SaStatus { get => _saStatus; set { _saStatus = value; OnPropertyChanged(); } }

        public ObservableCollection<string> AvailableRobots { get; }
        public ObservableCollection<string> AvailableDevices { get; }

        private string _selectedRobot = "慧灵科技 Z-Arm 2442";
        public string SelectedRobot { get => _selectedRobot; set { _selectedRobot = value; OnPropertyChanged(); } }

        private string _selectedDevice = "Spectrum Analyzer (VISA)";
        public string SelectedDevice { get => _selectedDevice; set { _selectedDevice = value; OnPropertyChanged(); } }

        public ICommand ConnectRobotCommand { get; }
        public ICommand DisconnectRobotCommand { get; }
        public ICommand ConnectSaCommand { get; }
        public ICommand DisconnectSaCommand { get; }

        public InstrumentSetupViewModel(InstrumentSettings settings)
        {
            _hardwareService = HardwareService.Instance;
            _instrumentSettings = settings; // 初始化

            AvailableRobots = new ObservableCollection<string> { "慧灵科技 Z-Arm 2442" };
            AvailableDevices = new ObservableCollection<string> { "Spectrum Analyzer (VISA)", "Vector Network Analyzer", "示波器" };

            ConnectRobotCommand = new RelayCommand(async _ => await ExecuteConnectRobot(), _ => !IsConnecting);
            DisconnectRobotCommand = new RelayCommand(_ => ExecuteDisconnectRobot(), _ => !IsConnecting);
            ConnectSaCommand = new RelayCommand(async _ => await ExecuteConnectSa(), _ => !IsConnecting);
            DisconnectSaCommand = new RelayCommand(_ => ExecuteDisconnectSa(), _ => !IsConnecting);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            RobotStatus = _hardwareService.ActiveRobot?.IsConnected ?? false ? "已连接" : "未连接";
            SaStatus = _hardwareService.ActiveDevice?.IsConnected ?? false ? "已连接" : "未连接";
        }

        private async Task ExecuteConnectRobot()
        {
            IsConnecting = true;
            RobotStatus = "连接中...";
            try
            {
                IRobotArm robot = SelectedRobot switch
                {
                    "慧灵科技 Z-Arm 2442" => new ScaraRobotArm(),
                    _ => throw new NotImplementedException($"机器人 '{SelectedRobot}' 不支持。")
                };
                _hardwareService.SetActiveRobot(robot);
                await _hardwareService.ActiveRobot!.ConnectAsync();
                RobotStatus = "已连接";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("连接机器人失败: " + ex.Message, "错误");
                RobotStatus = "连接失败";
            }
            finally { IsConnecting = false; }
        }

        private void ExecuteDisconnectRobot()
        {
            _hardwareService.ActiveRobot?.Disconnect();
            UpdateStatus();
        }

        private async Task ExecuteConnectSa()
        {
            IsConnecting = true;
            SaStatus = "连接中...";
            try
            {
                IMeasurementDevice device = SelectedDevice switch
                {
                    "Spectrum Analyzer (VISA)" => new SpectrumAnalyzer(),
                    _ => throw new NotImplementedException($"仪器 '{SelectedDevice}' 不支持。")
                };
                _hardwareService.SetActiveDevice(device);
                await _hardwareService.ActiveDevice!.ConnectAsync(this.InstrumentSettings);
                SaStatus = "已连接";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("连接频谱仪失败: " + ex.Message, "错误");
                SaStatus = "连接失败";
            }
            finally { IsConnecting = false; }
        }

        private void ExecuteDisconnectSa()
        {
            _hardwareService.ActiveDevice?.Disconnect();
            UpdateStatus();
        }
    }
}