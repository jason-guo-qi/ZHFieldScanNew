using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using FieldScanNew.Services;
using FieldScanNew.Views;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging; // 必须引用

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;

namespace FieldScanNew.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly HardwareService _hardwareService;

        public PlotModel HeatmapModel { get; set; }
        public PlotModel SpectrumModel { get; set; }
        public ObservableCollection<ProjectViewModel> Projects { get; }

        // **核心修正 1：添加主界面图片属性**
        private BitmapSource? _dutImageSource;
        public BitmapSource? DutImageSource
        {
            get => _dutImageSource;
            set { _dutImageSource = value; OnPropertyChanged(); }
        }

        private IStepViewModel? _selectedStep;
        public IStepViewModel? SelectedStep
        {
            get => _selectedStep;
            set
            {
                if (Equals(value, _selectedStep)) return;
                _selectedStep = value;
                OnPropertyChanged();

                // 只要不是点击的项目或测量项本身，就认为是点击了“步骤”，弹出对话框
                if (_selectedStep != null && !(_selectedStep is ProjectViewModel) && !(_selectedStep is MeasurementViewModel))
                {
                    _dialogService.ShowDialog(_selectedStep);

                    // **核心修正 2：弹窗关闭后，立即刷新主界面图片**
                    // 因为用户可能在弹窗里刚拍了新照片
                    LoadDutImage();
                }
            }
        }

        private ScanSettings _currentScanSettings;
        public ScanSettings CurrentScanSettings
        {
            get => _currentScanSettings;
            set
            {
                if (_currentScanSettings != null) _currentScanSettings.PropertyChanged -= OnSettingsChanged;
                _currentScanSettings = value;
                if (_currentScanSettings != null) _currentScanSettings.PropertyChanged += OnSettingsChanged;
                OnPropertyChanged();
            }
        }

        private InstrumentSettings _currentInstrumentSettings;
        public InstrumentSettings CurrentInstrumentSettings
        {
            get => _currentInstrumentSettings;
            set
            {
                if (_currentInstrumentSettings != null) _currentInstrumentSettings.PropertyChanged -= OnSettingsChanged;
                _currentInstrumentSettings = value;
                if (_currentInstrumentSettings != null) _currentInstrumentSettings.PropertyChanged += OnSettingsChanged;
                OnPropertyChanged();
            }
        }

        private bool _isScanning = false;
        public bool IsScanning { get => _isScanning; set { _isScanning = value; OnPropertyChanged(); } }
        private CancellationTokenSource? _cancellationTokenSource;

        public ICommand AddNewProjectCommand { get; }
        public ICommand LoadProjectCommand { get; }
        public ICommand StartScanCommand { get; }
        public ICommand StopScanCommand { get; }

        public MainViewModel()
        {
            _dialogService = new DialogService();
            _hardwareService = HardwareService.Instance;

            HeatmapModel = new PlotModel { Title = "近场热力图" };
            SpectrumModel = new PlotModel { Title = "实时频谱" };
            Projects = new ObservableCollection<ProjectViewModel>();
            _currentScanSettings = new ScanSettings();
            _currentInstrumentSettings = new InstrumentSettings();

            AddNewProjectCommand = new RelayCommand(ExecuteAddNewProject);
            LoadProjectCommand = new RelayCommand(ExecuteLoadProject);
            StartScanCommand = new RelayCommand(async _ => await ExecuteStartScan(), _ => !IsScanning);
            StopScanCommand = new RelayCommand(_ => ExecuteStopScan(), _ => IsScanning);

            CurrentScanSettings.PropertyChanged += OnSettingsChanged;
            CurrentInstrumentSettings.PropertyChanged += OnSettingsChanged;
        }

        // **核心修正 3：加载图片的方法**
        private void LoadDutImage()
        {
            var project = Projects.FirstOrDefault(p => p.IsSelected);
            // 检查项目是否存在，路径是否有效，文件是否存在
            if (project != null && !string.IsNullOrEmpty(project.ProjectData.DutImagePath) && File.Exists(project.ProjectData.DutImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 释放文件锁
                    bitmap.UriSource = new Uri(project.ProjectData.DutImagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    DutImageSource = bitmap;
                }
                catch
                {
                    DutImageSource = null;
                }
            }
            else
            {
                DutImageSource = null;
            }
        }

        private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
        {
            var selectedProject = Projects.FirstOrDefault(p => p.IsSelected);
            if (selectedProject != null)
            {
                AutoSaveCurrentProject(selectedProject);
            }
        }

        private void ExecuteAddNewProject(object? parameter)
        {
            try
            {
                var inputDialog = new InputDialog("请输入新项目的名称:", "新项目");
                if (inputDialog.ShowDialog() != true) return;
                string projectName = inputDialog.Answer;
                if (string.IsNullOrWhiteSpace(projectName)) return;

                var folderDialog = new System.Windows.Forms.FolderBrowserDialog { Description = "请选择项目的存放路径", UseDescriptionForTitle = true };
                if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                string projectPath = Path.Combine(folderDialog.SelectedPath, projectName);
                if (Directory.Exists(projectPath)) { MessageBox.Show("同名项目文件夹已存在！", "错误"); return; }

                Directory.CreateDirectory(projectPath);

                var newProject = new ProjectViewModel(projectName, projectPath, this);
                Projects.Add(newProject);

                foreach (var proj in Projects.Where(p => p != newProject))
                {
                    proj.IsSelected = false;
                }
                newProject.IsSelected = true;
                AutoSaveCurrentProject(newProject);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建新项目时发生严重错误: " + ex.Message, "错误");
            }
        }

        private void ExecuteLoadProject(object? parameter)
        {
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = "项目文件 (*.json)|*.json" };
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    string fileContent = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(fileContent))
                    {
                        MessageBox.Show("项目文件为空或已损坏。", "错误");
                        return;
                    }

                    var projectData = JsonSerializer.Deserialize<ProjectData>(fileContent);
                    if (projectData == null) { MessageBox.Show("无法解析项目文件。", "错误"); return; }

                    string projectFolder = Path.GetDirectoryName(filePath) ?? string.Empty;
                    if (string.IsNullOrEmpty(projectFolder))
                    {
                        MessageBox.Show("无法获取项目文件夹路径。", "错误");
                        return;
                    }

                    var loadedProject = new ProjectViewModel(projectData.ProjectName, projectFolder, this)
                    {
                        ProjectData = projectData
                    };

                    if (projectData.MeasurementNames != null)
                    {
                        foreach (var name in projectData.MeasurementNames)
                        {
                            loadedProject.Measurements.Add(new MeasurementViewModel(name, loadedProject));
                        }
                    }

                    Projects.Add(loadedProject);
                    foreach (var proj in Projects.Where(p => p != loadedProject))
                    {
                        proj.IsSelected = false;
                    }
                    loadedProject.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载项目时发生严重错误: " + ex.Message, "错误");
            }
        }

        public void AutoSaveCurrentProject(ProjectViewModel project)
        {
            if (project?.ProjectData == null) return;

            project.ProjectData.ScanConfig = this.CurrentScanSettings;
            project.ProjectData.InstrumentConfig = this.CurrentInstrumentSettings;
            project.ProjectData.MeasurementNames = project.Measurements.Select(m => m.DisplayName).ToList();

            try
            {
                string filePath = Path.Combine(project.ProjectFolderPath, "project.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(project.ProjectData, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动保存失败: {ex.Message}");
            }
        }

        internal void LoadProjectDataIntoViewModel(ProjectViewModel? project)
        {
            if (project?.ProjectData == null)
            {
                CurrentScanSettings = new ScanSettings();
                CurrentInstrumentSettings = new InstrumentSettings();
                return;
            }

            CurrentScanSettings = project.ProjectData.ScanConfig ?? new ScanSettings();
            CurrentInstrumentSettings = project.ProjectData.InstrumentConfig ?? new InstrumentSettings();

            foreach (var measurement in project.Measurements)
            {
                var instVm = measurement.Steps.OfType<InstrumentSetupViewModel>().FirstOrDefault();
                if (instVm != null) instVm.InstrumentSettings = CurrentInstrumentSettings;

                var scanVm = measurement.Steps.OfType<ScanAreaViewModel>().FirstOrDefault();
                if (scanVm != null) scanVm.Settings = CurrentScanSettings;
            }

            // **核心修正 4：切换项目时，也刷新图片**
            LoadDutImage();
        }

        private async Task ExecuteStartScan()
        {
            if (_hardwareService.ActiveRobot == null || !_hardwareService.ActiveRobot.IsConnected ||
               _hardwareService.ActiveDevice == null || !_hardwareService.ActiveDevice.IsConnected)
            {
                MessageBox.Show("请先连接机械臂和测量仪器！", "提示");
                return;
            }

            var selectedProject = Projects.FirstOrDefault(p => p.IsSelected);
            if (selectedProject == null) { MessageBox.Show("请先选择一个项目！", "提示"); return; }

            var scanSettings = selectedProject.ProjectData.ScanConfig;
            if (scanSettings.NumX < 2 || scanSettings.NumY < 2) { MessageBox.Show("扫描点数(X和Y)必须都大于等于2！", "错误"); return; }

            IsScanning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var heatMapData = new double[scanSettings.NumX, scanSettings.NumY];
                var heatMapSeries = new HeatMapSeries
                {
                    X0 = Math.Min(scanSettings.StartX, scanSettings.StopX),
                    X1 = Math.Max(scanSettings.StartX, scanSettings.StopX),
                    Y0 = Math.Min(scanSettings.StartY, scanSettings.StopY),
                    Y1 = Math.Max(scanSettings.StartY, scanSettings.StopY),
                    Interpolate = true,
                    Data = heatMapData
                };
                HeatmapModel.Series.Clear();
                HeatmapModel.Series.Add(heatMapSeries);
                HeatmapModel.InvalidatePlot(true);

                for (int j = 0; j < scanSettings.NumY; j++)
                {
                    for (int i = 0; i < scanSettings.NumX; i++)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            MessageBox.Show("扫描已停止。", "提示");
                            IsScanning = false;
                            return;
                        }

                        float x = scanSettings.StartX + i * (scanSettings.StopX - scanSettings.StartX) / (scanSettings.NumX - 1);
                        float y = scanSettings.StartY + j * (scanSettings.StopY - scanSettings.StartY) / (scanSettings.NumY - 1);

                        await _hardwareService.ActiveRobot.MoveToAsync(x, y, scanSettings.ScanHeightZ, 0);
                        double value = await _hardwareService.ActiveDevice.GetMeasurementValueAsync(100);

                        heatMapData[i, j] = value;
                        HeatmapModel.InvalidatePlot(true);
                    }
                }
                MessageBox.Show("扫描完成！", "成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("扫描过程中发生错误: " + ex.Message, "错误");
            }
            finally
            {
                IsScanning = false;
            }
        }

        private void ExecuteStopScan()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}