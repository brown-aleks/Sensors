using Avalonia.ReactiveUI;
using SensorUI.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive;

namespace SensorUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(action =>
                action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
        }

        private async Task DoShowDialogAsync(InteractionContext<DeviceSettingsViewModel, Unit> interaction)
        {
            var dialog = new DeviceSettingsWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }
    }
}