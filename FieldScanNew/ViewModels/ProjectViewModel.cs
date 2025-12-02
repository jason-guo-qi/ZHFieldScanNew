using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    public class ProjectViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName { get; set; }
        public string ProjectFolderPath { get; }
        public ObservableCollection<MeasurementViewModel> Measurements { get; }
        public ProjectData ProjectData { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
                if (_isSelected && ParentMainViewModel != null)
                {
                    ParentMainViewModel.LoadProjectDataIntoViewModel(this);
                }
            }
        }

        // **核心修正 1：将 private 改为 public，让子级能访问到主ViewModel**
        public MainViewModel ParentMainViewModel { get; }

        public ICommand AddNewMeasurementCommand { get; }

        public ProjectViewModel(string name, string folderPath, MainViewModel parent)
        {
            DisplayName = name;
            ProjectFolderPath = folderPath;
            ParentMainViewModel = parent;
            ProjectData = new ProjectData { ProjectName = name };

            Measurements = new ObservableCollection<MeasurementViewModel>();

            AddNewMeasurementCommand = new RelayCommand(ExecuteAddNewMeasurement);
        }

        private void ExecuteAddNewMeasurement(object? parameter)
        {
            var newMeasurement = new MeasurementViewModel($"New Measurement {Measurements.Count + 1}", this);
            Measurements.Add(newMeasurement);

            // **核心修正 2：新建测量项后，立即触发自动保存**
            ParentMainViewModel.AutoSaveCurrentProject(this);
        }
    }
}