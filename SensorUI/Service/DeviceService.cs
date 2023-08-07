using Microsoft.Extensions.Logging;
using SensorDrive;
using SensorUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Tmds.DBus.Protocol;

namespace SensorUI.Service
{
    public class DeviceService : IDeviceService
    {
        private readonly IDriver driver;
        private readonly ILogger<DeviceService> logger;
        private readonly List<Device> devices = new();

        private Device? selectDevice;

        private readonly ObservableCollection<Message> loggerMessages = new();

        public event Action<Message>? OnNewRecord_LoggerMessages;
        public Device? SelectDevice
        {
            get => selectDevice;
            set
            {
                selectDevice = value;
                logger.LogInformation("Выбрано устройство\t- {0}",selectDevice?.GetDeviceDescription());
            }
        }

        public DeviceService(IDriver driver, ILogger<DeviceService> logger)
        {
            this.driver = driver;
            this.logger = logger;
            this.driver.OnNewMessageFromSensor += Driver_OnNewMessageFromSensor;
            loggerMessages.CollectionChanged += LoggerMessages_CollectionChanged;
        }

        private void LoggerMessages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems?[0] is Message message)
            {
                OnNewRecord_LoggerMessages?.Invoke(message);
            }
        }

        private void Driver_OnNewMessageFromSensor(byte[] message)
        {
            logger.LogInformation("Получен пакет от устройства\t- {0}", string.Join("-", message));

            ulong serialNumber = BitConverter.ToUInt64(message.AsSpan(0,8));
            Word word = (Word)message[8];
            byte state = message[9];

            var device = devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

            if (device != null)
            {
                if (device.Fire != word.HasFlag(Word.Fire)) device.Fire = word.HasFlag(Word.Fire);
                if (device.Relay != word.HasFlag(Word.Relay)) device.Relay = word.HasFlag(Word.Relay);
                if (device.Test != word.HasFlag(Word.Test)) device.Test = word.HasFlag(Word.Test);

                if (device.State != state) device.State = state;
            }
            else
            {
                device = new Device(serialNumber)
                {
                    Fire = word.HasFlag(Word.Fire),
                    Relay = word.HasFlag(Word.Relay),
                    Test = word.HasFlag(Word.Test),
                    State = state
                };
            }

            loggerMessages.Add(new Message(DateTime.Now, device, MessageDirection.Incoming, message));
        }

        public IEnumerable<Device> GetDeviceByPattern(string pattern = @"\d")
        {
            var results = new List<Device>();
            Regex regex;
            try
            {
                regex = new(pattern);
            }
            catch (Exception ex)
            {
                logger.LogError("Не допустимый шаблон для поиска - {0}", ex);
                return results;
            }

            foreach (var device in devices)
            {
                var matches = regex.Matches(device.GetSerialNumber().ToString());
                var any = matches.Any();
                if (any)
                {
                    results.Add(device);
                }
            }
            return results;
        }

        public IEnumerable<Device> GetAllDevice()
        {
            var results = new List<Device>();

            var serialNumbers = driver.GetAllSerialNumbers();

            var deviceSerialNumbers = devices.Select(s => s.SerialNumber).ToHashSet();

            foreach (var number in serialNumbers)
            {
                if (deviceSerialNumbers.Contains(number))
                {
                    results.Add(devices.First(s => s.SerialNumber == number));
                }
                else
                {
                    var device = new Device(number);
                    devices.Add(device);
                    results.Add(device);
                }
            }
            return results;
        }

        public void SendCommandToDevice(byte command)
        {
            if (selectDevice is null)
            {
                logger.LogError("Команда {0} не может быть выполнена. Не выбрано устройство.", command);
                return;
            }

            byte[] message = new byte[9];
            var s = BitConverter.GetBytes(selectDevice.SerialNumber);

            for (int i = 0; i < 8; i++)
                message[i] = s[i];

            message[8] = command;

            logger.LogInformation("Отправлен пакет на устройство\t- {0}", string.Join("-", message));
            loggerMessages.Add(new Message(DateTime.Now, selectDevice, MessageDirection.Outgoing, message));
            driver.SendMessageToSensor(message);
        }
    }

    public record Message(DateTime DateTime, Device Device, MessageDirection Direction, byte[] message);
}
