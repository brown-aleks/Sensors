using ReactiveUI;
using SensorUI.Service;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

namespace SensorUI.ViewModels
{
    public class DeviceSettingsViewModel : ViewModelBase
    {
        private readonly IDeviceService deviceService;

        private byte commandSelectedIndex = 0;
        private bool _isBusy;
        private DeviceViewModel? selectedDevice;
        private readonly DeviceHistoryViewModels deviceHistory;

        public ReactiveCommand<Unit, Unit> CloseSettingsCommand { get; }

        public DeviceSettingsViewModel(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
            this.deviceHistory = new(this.deviceService);

            this.WhenAnyValue(x => x.CommandSelectedIndex)
                .Subscribe(x => this.deviceService.SendCommandToDevice(x));

            CloseSettingsCommand = ReactiveCommand.Create(() => { });
        }

        public DeviceViewModel? SelectedDevice
        {
            get => selectedDevice;
            set => selectedDevice = value;
        }
        public DeviceHistoryViewModels DeviceHistory => deviceHistory;
        public byte CommandSelectedIndex
        {
            get => commandSelectedIndex;
            set => this.RaiseAndSetIfChanged(ref commandSelectedIndex, value);
        }
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
    }
}
