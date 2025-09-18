using FieldScanNew.Infrastructure;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    public class MeasurementViewModel : ViewModelBase, IStepViewModel
    {
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing == value) return;
                _isEditing = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IStepViewModel> Steps { get; }
        public ICommand StartEditingCommand { get; }

        // **新增**：用于确认重命名的命令
        public ICommand CommitEditCommand { get; }

        private string _originalName; // 用于在取消时恢复原名

        public MeasurementViewModel(string name)
        {
            _originalName = name;
            DisplayName = name;
            IsEditing = false;

            StartEditingCommand = new RelayCommand(_ =>
            {
                _originalName = DisplayName; // 开始编辑前，保存当前的名字
                IsEditing = true;
            });

            CommitEditCommand = new RelayCommand(_ =>
            {
                IsEditing = false;
                // 可以在这里触发一次自动保存
            });

            Steps = new ObservableCollection<IStepViewModel>
            {
                new InstrumentSetupViewModel(),
                new ProbeSetupViewModel(),
                new ScanSettingsViewModel(),
                new ZCalibViewModel(),
                new XYCalibViewModel(),
                new ScanAreaViewModel()
            };
        }
    }
}