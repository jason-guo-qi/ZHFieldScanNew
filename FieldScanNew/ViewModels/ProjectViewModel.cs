using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System.Collections.ObjectModel;

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
                    // **健壮性修正**: 确保在调用前 ParentMainViewModel 是存在的
                    ParentMainViewModel.LoadProjectDataIntoViewModel(this);
                }
            }
        }

        // **健壮性修正**: 将 ParentMainViewModel 移到构造函数中，确保它在创建时就被赋值
        private MainViewModel ParentMainViewModel { get; }

        public ProjectViewModel(string name, string folderPath, MainViewModel parent)
        {
            DisplayName = name;
            ProjectFolderPath = folderPath;
            ParentMainViewModel = parent; // 强制关联父级
            ProjectData = new ProjectData { ProjectName = name };

            Measurements = new ObservableCollection<MeasurementViewModel>
            {
                new MeasurementViewModel("NearField_N9322C_3")
            };
        }
    }
}