using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using SensorUI.Models;
using SensorUI.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _searchSerialNumber = string.Empty;
        private bool _isBusy;
        private DeviceViewModel? _selectedDevice;
        private bool _automatic = true;
        private bool _handle = true;
        private bool _disabled = true;
        private bool _test = true;
        private bool _relay = true;
        private bool _fire = true;
        private bool _none = true;
        private readonly DeviceSettingsViewModel deviceSettingsViewModel;
        private readonly IDeviceService deviceService;
        private readonly ILogger logger;

        public ICommand DeviceSettingsCommand { get; }
        public ICommand GetAllCommand { get; }
        public Interaction<DeviceSettingsViewModel, Unit> ShowDialog { get; }
        public ObservableCollection<DeviceViewModel> SearchResults { get; } = new();

        public MainWindowViewModel(DeviceSettingsViewModel deviceSettingsViewModel, IDeviceService deviceService, ILogger<MainWindowViewModel> logger)
        {
            this.deviceSettingsViewModel = deviceSettingsViewModel;
            this.deviceService = deviceService;
            this.logger = logger;

            var isItemSelected = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(x =>
                {
                    this.deviceSettingsViewModel.SelectedDevice = x;
                    this.deviceService.SelectDevice = SelectedDevice?.Device;
                    return SelectedDevice != null;
                });

            ShowDialog = new Interaction<DeviceSettingsViewModel, Unit>();

            DeviceSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.logger.LogInformation($"Нажата кнопка - {nameof(DeviceSettingsCommand)}");
                var result = await ShowDialog.Handle(this.deviceSettingsViewModel);
            }, isItemSelected);

            GetAllCommand = ReactiveCommand.Create(() =>
            {
                SearchSerialNumber = string.Empty;
                Automatic = true;
                Handle = true;
                Disabled = true;
                Test = true;
                Relay = true;
                Fire = true;
                None = true;
            });

            this.WhenAnyValue(s => s.SearchSerialNumber)
                            .Throttle(TimeSpan.FromMilliseconds(500))
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(DoSearch);

            this.WhenAnyValue(  a => a.Automatic,
                                h => h.Handle,
                                d => d.Disabled,
                                t => t.Test,
                                r => r.Relay,
                                f => f.Fire, 
                                n => n.None)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(x =>
                            {
                                DoSearch(SearchSerialNumber!);
                            });

            foreach (var device in this.deviceService.GetAllDevice())
            {
                SearchResults.Add(new DeviceViewModel(device));
            }
        }

        private void DoSearch(string s)
        {
            string pattern;
            IsBusy = true;
            SearchResults.Clear();

            if (string.IsNullOrEmpty(s)) pattern = @"\d";
            else pattern = s;

           IEnumerable<Device> collection = deviceService.GetDeviceByPattern(pattern)
                .Where(d =>
                        (d.State == 0 && Automatic == true) ||
                        (d.State == 1 && Handle == true) ||
                        (d.State == 2 && Disabled == true) ||
                        (d.Test == true && Test == true) ||
                        (d.Relay == true && Relay == true) ||
                        (d.Fire == true && Fire ==true) ||
                        (d.Test == false && d.Relay == false && d.Fire == false && None == true)
                        );

            foreach (var item in collection)
            {
                SearchResults.Add(new DeviceViewModel(item));
            }

            IsBusy = false;
        }

        public string SearchSerialNumber
        {
            get => _searchSerialNumber;
            set => this.RaiseAndSetIfChanged(ref _searchSerialNumber, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        public bool Automatic
        {
            get => _automatic;
            set => this.RaiseAndSetIfChanged(ref _automatic, value);
        }
        public bool Handle
        {
            get => _handle;
            set => this.RaiseAndSetIfChanged(ref _handle, value);
        }
        public bool Disabled
        {
            get => _disabled;
            set => this.RaiseAndSetIfChanged(ref _disabled, value);
        }
        public bool None
        {
            get => _none;
            set => this.RaiseAndSetIfChanged(ref _none, value);
        }
        public bool Fire
        {
            get => _fire;
            set => this.RaiseAndSetIfChanged(ref _fire, value);
        }
        public bool Relay
        {
            get => _relay;
            set => this.RaiseAndSetIfChanged(ref _relay, value);
        }
        public bool Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

        public DeviceViewModel? SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }
    }
}