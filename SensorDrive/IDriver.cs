namespace SensorDrive
{
    public interface IDriver
    {
        event Action<byte[]>? OnNewMessageFromSensor;

        IEnumerable<ulong> GetAllSerialNumbers();
        void SendMessageToSensor(byte[] message);
    }
}