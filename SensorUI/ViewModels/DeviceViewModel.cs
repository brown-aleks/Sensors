using ReactiveUI;
using SensorUI.Models;
using System.Diagnostics;

namespace SensorUI.ViewModels
{
    public class DeviceViewModel : ViewModelBase
    {
        private readonly Device device;
        private string deviceDescription = string.Empty;

        public DeviceViewModel(Device device)
        {
            this.device = device;
            DeviceDescription = this.device.GetDeviceDescription();
            device.OnStateChanged += Device_OnStateChanged;
            device.OnFire += Device_OnWord;
            device.OnRelay += Device_OnWord;
            device.OnTest += Device_OnWord;
        }

        public Device Device => device;
        public string DeviceDescription
        {
            get => deviceDescription;
            set
            {
                this.RaiseAndSetIfChanged(ref deviceDescription, value);
            }
        }

        private void Device_OnWord(bool o) => DeviceDescription = device.GetDeviceDescription();
        private void Device_OnStateChanged(byte o) => DeviceDescription = device.GetDeviceDescription();

    }
}
