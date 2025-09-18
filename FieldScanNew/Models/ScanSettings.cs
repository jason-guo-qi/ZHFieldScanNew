// Models/ScanSettings.cs
// This file is used to store the scan settings.
using FieldScanNew.Infrastructure;

namespace FieldScanNew.Models
{
    // 继承自 ViewModelBase 来获得通知功能
    public class ScanSettings : ViewModelBase
    {
        private float _startX;
        public float StartX { get => _startX; set { _startX = value; OnPropertyChanged(); } }

        private float _startY;
        public float StartY { get => _startY; set { _startY = value; OnPropertyChanged(); } }

        private float _stopX;
        public float StopX { get => _stopX; set { _stopX = value; OnPropertyChanged(); } }

        private float _stopY;
        public float StopY { get => _stopY; set { _stopY = value; OnPropertyChanged(); } }

        private int _numX;
        public int NumX { get => _numX; set { _numX = value; OnPropertyChanged(); } }

        private int _numY;
        public int NumY { get => _numY; set { _numY = value; OnPropertyChanged(); } }

        private float _scanHeightZ;
        public float ScanHeightZ { get => _scanHeightZ; set { _scanHeightZ = value; OnPropertyChanged(); } }
    }
}