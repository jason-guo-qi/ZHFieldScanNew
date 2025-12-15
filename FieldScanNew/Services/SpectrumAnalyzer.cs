using FieldScanNew.Models;
using Ivi.Visa;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FieldScanNew.Services
{
    public class SpectrumAnalyzer : IMeasurementDevice
    {
        public string DeviceName => "Spectrum Analyzer (VISA)";
        public bool IsConnected { get; private set; } = false;
        private IMessageBasedSession? _saSession;

        public async Task ConnectAsync(InstrumentSettings settings)
        {
            // 如果已经连接，先断开，防止参数没更新
            if (IsConnected) Disconnect();

            await Task.Run(() =>
            {
                // 注意：这里使用的是 SOCKET 协议
                //string visaAddress = $"TCPIP0::{settings.IpAddress}::{settings.Port}::SOCKET";
                string visaAddress = $"TCPIP0::{settings.IpAddress}::INSTR";

                var visaSession = GlobalResourceManager.Open(visaAddress);
                _saSession = visaSession as IMessageBasedSession;

                if (_saSession == null) throw new Exception("无法创建VISA会话，设备可能不支持消息通信。");

                // =========================================================
                // **核心修正：补全通信协议配置 (参考旧工程 Sa.cs)**
                // =========================================================
                _saSession.TimeoutMilliseconds = 30000; // 30秒超时

                // 启用终止符：告诉VISA读到 '\n' 就认为一句话结束了
                _saSession.TerminationCharacterEnabled = true;
                _saSession.TerminationCharacter = (byte)'\n';

                // 对于 SOCKET 连接，通常不需要 SendEnd
                _saSession.SendEndEnabled = false;
                // =========================================================

                // 在 IsConnected = true; 之前加上：
                _saSession.FormattedIO.WriteLine("*IDN?");
                string idn = _saSession.FormattedIO.ReadLine(); // 如果这里没报错，说明连接绝对没问题

                IsConnected = true;
            });
        }

        public void Disconnect()
        {
            if (_saSession != null)
            {
                try { _saSession.Dispose(); } catch { }
                _saSession = null;
            }
            IsConnected = false;
        }

        public async Task<double> GetMeasurementValueAsync(int delayMs)
        {
            if (!IsConnected || _saSession == null) throw new InvalidOperationException("频谱仪未连接");

            return await Task.Run(() =>
            {
                var formattedIO = _saSession.FormattedIO;

                // 发送指令
                formattedIO.WriteLine(":TRAC:TYPE MAXH;");
                Thread.Sleep(delayMs);
                formattedIO.WriteLine(":CALC:MARK:MAX;");
                formattedIO.WriteLine(":CALC:MARK:Y?;");

                // 读取结果 (因为设置了 TerminationCharacter，这里就不会超时了)
                return formattedIO.ReadLineDouble();
            });
        }
    }
}