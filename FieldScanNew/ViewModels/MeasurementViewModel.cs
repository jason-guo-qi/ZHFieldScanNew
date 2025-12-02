using FieldScanNew.Infrastructure;
using FieldScanNew.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    public class MeasurementViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName { get; set; }
        public ObservableCollection<IStepViewModel> Steps { get; }
        public ProjectViewModel ParentProject { get; }
        public ICommand RenameCommand { get; }

        public MeasurementViewModel(string name, ProjectViewModel parent)
        {
            DisplayName = name;
            ParentProject = parent;

            RenameCommand = new RelayCommand(_ => ExecuteRename());

            Steps = new ObservableCollection<IStepViewModel>
            {
                new InstrumentSetupViewModel(ParentProject.ProjectData.InstrumentConfig),
                new ProbeSetupViewModel(),
                new ScanSettingsViewModel(),
                new ZCalibViewModel(),
                
                // **核心修正：传入项目文件夹路径**
                new XYCalibViewModel(ParentProject.ProjectData, ParentProject.ProjectFolderPath),

                new ScanAreaViewModel(ParentProject.ProjectData)
            };
        }

        private void ExecuteRename()
        {
            var inputDialog = new InputDialog("请输入新的测量名称:", this.DisplayName);
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                DisplayName = inputDialog.Answer;
                OnPropertyChanged(nameof(DisplayName));
                ParentProject.ParentMainViewModel.AutoSaveCurrentProject(ParentProject);
            }
        }
    }
}