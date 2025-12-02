using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing; // 需要引用 System.Drawing.Common 或 AForge 自带的 Bitmap
using System.IO;
using System.Windows.Media.Imaging;

namespace FieldScanNew.Services
{
    public class CameraService
    {
        private FilterInfoCollection? _videoDevices;
        private VideoCaptureDevice? _videoSource;

        // 当有新的一帧图像时触发
        public event Action<BitmapSource>? NewFrameReceived;

        public List<string> GetCameraList()
        {
            var cameras = new List<string>();
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in _videoDevices)
            {
                cameras.Add(device.Name);
            }
            return cameras;
        }

        public void StartCamera(int cameraIndex)
        {
            if (_videoDevices == null || _videoDevices.Count == 0) return;
            if (cameraIndex < 0 || cameraIndex >= _videoDevices.Count) return;

            StopCamera();

            _videoSource = new VideoCaptureDevice(_videoDevices[cameraIndex].MonikerString);
            _videoSource.NewFrame += OnNewFrame;
            _videoSource.Start();
        }

        public void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= OnNewFrame;
                _videoSource = null;
            }
        }

        private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // AForge 返回的是 System.Drawing.Bitmap
                // 我们需要将其转换为 WPF 的 BitmapSource
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var bi = ToBitmapImage(bitmap);
                    bi.Freeze(); // 必须冻结才能跨线程传递给 UI
                    NewFrameReceived?.Invoke(bi);
                }
            }
            catch { /* 忽略转换错误 */ }
        }

        // 辅助方法：Bitmap -> BitmapImage
        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}