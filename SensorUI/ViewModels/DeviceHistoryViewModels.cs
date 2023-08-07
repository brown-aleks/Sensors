using ReactiveUI;
using SensorUI.Service;
using System;

namespace SensorUI.ViewModels
{
    public class DeviceHistoryViewModels : ViewModelBase
    {
        private readonly IDeviceService _deviceService;
        private string _deviceHistory = string.Empty;

        public DeviceHistoryViewModels(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            _deviceService.OnNewRecord_LoggerMessages += DeviceService_OnNewRecord_LoggerMessages;
        }

        private void DeviceService_OnNewRecord_LoggerMessages(Message message)
        {
            string newString =  message.DateTime.ToString("hh:mm:ss-ffff") + "\t" +
                                message.Device.SerialNumber.ToString("[ 00-000-000-000-000-000-000 ]") + "\t" +
                                message.Direction.ToString() + "\t" +
                                " byteMessage: " +
                                string.Join('-', message.message);
            DeviceHistory = string.Concat(newString, Environment.NewLine, DeviceHistory);
        }

        public string DeviceHistory
        {
            get => _deviceHistory;
            set => this.RaiseAndSetIfChanged(ref _deviceHistory, value);
        }
    }
}
