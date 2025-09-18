// Models/InstrumentSettings.cs
// This file is used to store the settings of the instrument.
using FieldScanNew.Infrastructure;

namespace FieldScanNew.Models
{
    // 继承自 ViewModelBase
    public class InstrumentSettings : ViewModelBase
    {
        private string _ipAddress = "192.168.0.22";
        public string IpAddress { get => _ipAddress; set { _ipAddress = value; OnPropertyChanged(); } }

        private int _port = 5025;
        public int Port { get => _port; set { _port = value; OnPropertyChanged(); } }

        private double _centerFrequencyHz = 1e9;
        public double CenterFrequencyHz { get => _centerFrequencyHz; set { _centerFrequencyHz = value; OnPropertyChanged(); } }

        private double _spanHz = 100e6;
        public double SpanHz { get => _spanHz; set { _spanHz = value; OnPropertyChanged(); } }

        private int _points = 1001;
        public int Points { get => _points; set { _points = value; OnPropertyChanged(); } }

        private string _centerFrequencyUnit = "GHz";
        public string CenterFrequencyUnit { get => _centerFrequencyUnit; set { _centerFrequencyUnit = value; OnPropertyChanged(); } }

        private string _spanUnit = "MHz";
        public string SpanUnit { get => _spanUnit; set { _spanUnit = value; OnPropertyChanged(); } }

        private double _referenceLevelDb = 0;
        public double ReferenceLevelDb { get => _referenceLevelDb; set { _referenceLevelDb = value; OnPropertyChanged(); } }
    }
}