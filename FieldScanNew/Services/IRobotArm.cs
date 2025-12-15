using System.Threading.Tasks;

namespace FieldScanNew.Services
{
    public interface IRobotArm
    {
        string DeviceName { get; }
        bool IsConnected { get; }
        Task ConnectAsync();
        void Disconnect();

        Task MoveJogAsync(float stepX, float stepY, float stepZ, float stepR);
        Task MoveToAsync(float x, float y, float z, float r);

        // **核心修正：新增不等待到位的移动接口 (用于强制 Z 轴归位)**
        Task MoveToNoWaitAsync(float x, float y, float z, float r);

        Task<RobotPosition> GetPositionAsync();
        Task SetDragModeAsync(bool enable);
    }
}