// Models/ProjectData.cs
// This file is used to store all the data that needs to be saved in the project.json file.
namespace FieldScanNew.Models
{
    // 这个类定义了要保存到 project.json 文件中的所有数据
    public class ProjectData
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? DutImagePath { get; set; }
        public string? ActiveProbeName { get; set; }

        // 嵌套了仪器设置和扫描区域设置
        public InstrumentSettings InstrumentConfig { get; set; }
        public ScanSettings ScanConfig { get; set; }

        public ProjectData()
        {
            // 初始化，防止空引用
            InstrumentConfig = new InstrumentSettings();
            ScanConfig = new ScanSettings();
        }
    }
}