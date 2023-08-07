using Avalonia.ReactiveUI;
using ReactiveUI;
using SensorUI.ViewModels;
using System;
using System.Reactive.Linq;

namespace SensorUI.Views
{
    public partial class DeviceSettingsWindow : ReactiveWindow<DeviceSettingsViewModel>
    {
        public DeviceSettingsWindow()
        {
            InitializeComponent();

            this.WhenActivated(action =>
                    action(ViewModel!
                    .CloseSettingsCommand
                    .Subscribe(result =>
                        Close(result))));
        }
    }
}