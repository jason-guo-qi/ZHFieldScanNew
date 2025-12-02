using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using FieldScanNew.Services;
using System;
using System.Collections.ObjectModel;
using System.IO; // 需要引用 IO
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace FieldScanNew.ViewModels
{
    public class XYCalibViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "5. XY平面校准";

        private readonly ProjectData _projectData;
        private readonly string _projectFolderPath; // **新增：保存项目路径**
        private readonly CameraService _cameraService;
        private readonly HardwareService _hardwareService;

        private BitmapSource? _dutImageSource;
        public BitmapSource? DutImageSource { get => _dutImageSource; set { _dutImageSource = value; OnPropertyChanged(); } }

        private BitmapSource? _cameraPreviewSource;
        public BitmapSource? CameraPreviewSource { get => _cameraPreviewSource; set { _cameraPreviewSource = value; OnPropertyChanged(); } }

        private bool _isCameraMode = false;
        public bool IsCameraMode { get => _isCameraMode; set { _isCameraMode = value; OnPropertyChanged(); } }

        public ObservableCollection<string> CameraList { get; }
        private int _selectedCameraIndex = 0;
        public int SelectedCameraIndex { get => _selectedCameraIndex; set { _selectedCameraIndex = value; OnPropertyChanged(); } }

        private float _jogStep = 10.0f;
        public float JogStep { get => _jogStep; set { _jogStep = value; OnPropertyChanged(); } }

        private string _instructionText = "步骤1：拍照并点击图片上的【特征点1】。";
        public string InstructionText { get => _instructionText; set { _instructionText = value; OnPropertyChanged(); } }

        private Point _pixelP1;
        private Point _pixelP2;
        public string PixelP1Text => $"像素 P1: ({_pixelP1.X:F0}, {_pixelP1.Y:F0})";
        public string PixelP2Text => $"像素 P2: ({_pixelP2.X:F0}, {_pixelP2.Y:F0})";

        private double _physicalX1;
        public double PhysicalX1 { get => _physicalX1; set { _physicalX1 = value; OnPropertyChanged(); } }
        private double _physicalY1;
        public double PhysicalY1 { get => _physicalY1; set { _physicalY1 = value; OnPropertyChanged(); } }
        private double _physicalX2;
        public double PhysicalX2 { get => _physicalX2; set { _physicalX2 = value; OnPropertyChanged(); } }
        private double _physicalY2;
        public double PhysicalY2 { get => _physicalY2; set { _physicalY2 = value; OnPropertyChanged(); } }

        private int _clickStep = 0;
        private bool _showP1 = false;
        public bool ShowP1 { get => _showP1; set { _showP1 = value; OnPropertyChanged(); } }
        private bool _showP2 = false;
        public bool ShowP2 { get => _showP2; set { _showP2 = value; OnPropertyChanged(); } }

        public double P1Left => _pixelP1.X - 5;
        public double P1Top => _pixelP1.Y - 5;
        public double P2Left => _pixelP2.X - 5;
        public double P2Top => _pixelP2.Y - 5;

        public ICommand ToggleCameraCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand CalibrateCommand { get; }
        public ICommand JogCommand { get; }
        public ICommand ReadRobotPosCommand { get; }

        // **核心修正：构造函数增加 projectFolderPath 参数**
        public XYCalibViewModel(ProjectData projectData, string projectFolderPath)
        {
            _projectData = projectData;
            _projectFolderPath = projectFolderPath;
            _hardwareService = HardwareService.Instance;
            _cameraService = new CameraService();
            CameraList = new ObservableCollection<string>(_cameraService.GetCameraList());
            if (CameraList.Count > 0) SelectedCameraIndex = 0;

            ToggleCameraCommand = new RelayCommand(ExecuteToggleCamera);
            CaptureCommand = new RelayCommand(ExecuteCapture);
            ResetCommand = new RelayCommand(ExecuteReset);
            CalibrateCommand = new RelayCommand(ExecuteCalibrate);
            JogCommand = new RelayCommand(async (param) => await ExecuteJog(param));
            ReadRobotPosCommand = new RelayCommand(async (param) => await ExecuteReadRobotPos(param));

            _cameraService.NewFrameReceived += OnNewFrameReceived;

            if (!string.IsNullOrEmpty(_projectData.DutImagePath) && System.IO.File.Exists(_projectData.DutImagePath))
            {
                LoadImageFromPath(_projectData.DutImagePath);
            }
        }

        private async Task ExecuteJog(object? parameter)
        {
            if (_hardwareService.ActiveRobot == null || !_hardwareService.ActiveRobot.IsConnected)
            {
                MessageBox.Show("机械臂未连接，无法移动！请先在“仪器连接”中连接。", "错误");
                return;
            }

            string direction = parameter as string ?? "";
            float x = 0, y = 0;
            switch (direction)
            {
                case "X+": x = JogStep; break;
                case "X-": x = -JogStep; break;
                case "Y+": y = JogStep; break;
                case "Y-": y = -JogStep; break;
            }
            try { await _hardwareService.ActiveRobot.MoveJogAsync(x, y, 0); }
            catch (Exception ex) { MessageBox.Show("移动失败: " + ex.Message); }
        }

        private async Task ExecuteReadRobotPos(object? parameter)
        {
            if (_hardwareService.ActiveRobot == null || !_hardwareService.ActiveRobot.IsConnected)
            {
                MessageBox.Show("机械臂未连接，无法读取坐标！", "错误");
                return;
            }
            try
            {
                var pos = await _hardwareService.ActiveRobot.GetPositionAsync();
                string target = parameter as string ?? "1";
                if (target == "1") { PhysicalX1 = pos.X; PhysicalY1 = pos.Y; }
                else if (target == "2") { PhysicalX2 = pos.X; PhysicalY2 = pos.Y; }
            }
            catch (Exception ex) { MessageBox.Show("读取坐标失败: " + ex.Message); }
        }

        public void HandleImageClick(Point pixelPoint)
        {
            if (IsCameraMode) { MessageBox.Show("请先拍照定格画面，再进行选点。", "提示"); return; }
            if (DutImageSource == null) return;

            if (_clickStep == 0)
            {
                _pixelP1 = pixelPoint;
                ShowP1 = true;
                OnPropertyChanged(nameof(P1Left)); OnPropertyChanged(nameof(P1Top)); OnPropertyChanged(nameof(PixelP1Text));
                _clickStep = 1;
                InstructionText = "步骤2：请移动机械臂到该点上方，记录物理坐标。\n然后点击图片上的【特征点2】。";
            }
            else if (_clickStep == 1)
            {
                _pixelP2 = pixelPoint;
                ShowP2 = true;
                OnPropertyChanged(nameof(P2Left)); OnPropertyChanged(nameof(P2Top)); OnPropertyChanged(nameof(PixelP2Text));
                _clickStep = 2;
                InstructionText = "步骤3：移动机械臂到点2记录坐标，最后点击“计算校准”。";
            }
        }

        private void ExecuteReset(object? obj)
        {
            _clickStep = 0; ShowP1 = false; ShowP2 = false;
            InstructionText = "步骤1：拍照并点击图片上的【特征点1】。";
        }

        private void ExecuteCalibrate(object? obj)
        {
            if (_clickStep < 2) { MessageBox.Show("请先在图片上选取两个点。", "提示"); return; }

            double dxPix = _pixelP2.X - _pixelP1.X;
            double dyPix = _pixelP2.Y - _pixelP1.Y;
            double lenPixSq = dxPix * dxPix + dyPix * dyPix;
            if (lenPixSq < 1e-6) { MessageBox.Show("两个像素点重合了，无法校准！", "错误"); return; }

            double dxPhy = PhysicalX2 - PhysicalX1;
            double dyPhy = PhysicalY2 - PhysicalY1;

            double a = (dxPhy * dxPix + dyPhy * dyPix) / lenPixSq;
            double b = (dyPhy * dxPix - dxPhy * dyPix) / lenPixSq;

            _projectData.MatrixM11 = a; _projectData.MatrixM12 = -b;
            _projectData.MatrixM21 = b; _projectData.MatrixM22 = a;
            _projectData.OffsetX = PhysicalX1 - (a * _pixelP1.X - b * _pixelP1.Y);
            _projectData.OffsetY = PhysicalY1 - (b * _pixelP1.X + a * _pixelP1.Y);
            _projectData.IsCalibrated = true;

            MessageBox.Show($"校准成功！\n缩放系数: {Math.Sqrt(a * a + b * b):F4}", "成功");
        }

        private void OnNewFrameReceived(BitmapSource frame)
        {
            Application.Current.Dispatcher.Invoke(() => { CameraPreviewSource = frame; });
        }

        private void ExecuteToggleCamera(object? obj)
        {
            if (!IsCameraMode)
            {
                if (CameraList.Count == 0) { MessageBox.Show("未检测到摄像头！", "提示"); return; }
                _cameraService.StartCamera(SelectedCameraIndex);
                IsCameraMode = true;
                ExecuteReset(null);
            }
            else
            {
                _cameraService.StopCamera();
                IsCameraMode = false;
                CameraPreviewSource = null;
            }
        }

        private void ExecuteCapture(object? obj)
        {
            if (CameraPreviewSource != null)
            {
                DutImageSource = CameraPreviewSource;
                _cameraService.StopCamera();
                IsCameraMode = false;

                // **核心修正：拍照后立即保存图片到硬盘，并更新 ProjectData**
                SaveCaptureToFile(DutImageSource);
            }
        }

        private void SaveCaptureToFile(BitmapSource image)
        {
            try
            {
                // 1. 确保项目下的 Images 文件夹存在
                string imagesFolder = Path.Combine(_projectFolderPath, "Images");
                if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

                // 2. 生成文件名 (例如 Capture_20231027_123000.jpg)
                string fileName = $"Capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string fullPath = Path.Combine(imagesFolder, fileName);

                // 3. 保存为 JPG
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                // 4. 更新项目数据中的路径
                _projectData.DutImagePath = fullPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存图片失败: {ex.Message}", "错误");
            }
        }

        private void LoadImageFromPath(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze();
                DutImageSource = bitmap;
            }
            catch { }
        }

        ~XYCalibViewModel()
        {
            _cameraService.StopCamera();
        }
    }
}