using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace FieldScanNew.ViewModels
{
    public class ScanAreaViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "6. 扫描区域配置";

        private readonly ProjectData _projectData;
        public ScanSettings Settings
        {
            get => _projectData.ScanConfig;
            set
            {
                if (_projectData.ScanConfig != value)
                {
                    _projectData.ScanConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapSource? _dutImageSource;
        public BitmapSource? DutImageSource { get => _dutImageSource; set { _dutImageSource = value; OnPropertyChanged(); } }

        private string _statusText = "请在图片上【按住鼠标左键拖拽】以框选扫描区域。";
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        public ScanAreaViewModel(ProjectData projectData)
        {
            _projectData = projectData;
            ReloadImage(); // 初始化时加载
        }

        // **核心修正：改为 Public 方法，允许外部强制刷新**
        public void ReloadImage()
        {
            if (!string.IsNullOrEmpty(_projectData.DutImagePath) && System.IO.File.Exists(_projectData.DutImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 关键：释放文件锁
                    bitmap.UriSource = new Uri(_projectData.DutImagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    DutImageSource = bitmap;
                }
                catch { }
            }
            else
            {
                DutImageSource = null;
                StatusText = "未找到校准图片，请先在“XY平面校准”中加载或拍摄图片。";
            }
        }

        public void UpdateScanAreaFromSelection(Rect rectPixel)
        {
            if (!_projectData.IsCalibrated)
            {
                MessageBox.Show("系统尚未校准！\n请先完成“5. XY平面校准”，否则无法自动计算物理坐标。", "警告");
                return;
            }

            double m11 = _projectData.MatrixM11;
            double m12 = _projectData.MatrixM12;
            double m21 = _projectData.MatrixM21;
            double m22 = _projectData.MatrixM22;
            double offX = _projectData.OffsetX;
            double offY = _projectData.OffsetY;

            double x1 = rectPixel.X;
            double y1 = rectPixel.Y;
            double physX1 = (m11 * x1 + m12 * y1) + offX;
            double physY1 = (m21 * x1 + m22 * y1) + offY;

            double x2 = rectPixel.X + rectPixel.Width;
            double y2 = rectPixel.Y + rectPixel.Height;
            double physX2 = (m11 * x2 + m12 * y2) + offX;
            double physY2 = (m21 * x2 + m22 * y2) + offY;

            Settings.StartX = (float)Math.Min(physX1, physX2);
            Settings.StopX = (float)Math.Max(physX1, physX2);
            Settings.StartY = (float)Math.Min(physY1, physY2);
            Settings.StopY = (float)Math.Max(physY1, physY2);

            StatusText = $"区域已更新：X[{Settings.StartX:F1}, {Settings.StopX:F1}], Y[{Settings.StartY:F1}, {Settings.StopY:F1}]";
        }
    }
}