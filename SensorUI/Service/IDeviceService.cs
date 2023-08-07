using SensorUI.Models;
using System;
using System.Collections.Generic;

namespace SensorUI.Service
{
    public interface IDeviceService
    {
        Device? SelectDevice { get; set; }

        event Action<Message>? OnNewRecord_LoggerMessages;

        IEnumerable<Device> GetAllDevice();
        IEnumerable<Device> GetDeviceByPattern(string pattern);
        void SendCommandToDevice(byte command);
    }
}