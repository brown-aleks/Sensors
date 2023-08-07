namespace SensorUI.Models
{
    public interface IDevice
    {
        string GetDeviceDescription();
        ulong GetSerialNumber();
    }
}