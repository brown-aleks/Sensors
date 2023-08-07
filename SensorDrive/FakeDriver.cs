using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorDrive
{
    public class FakeDriver : IDriver
    {
        private readonly IEnumerable<Sensor> _sensors;

        /// <summary>
        /// Получено новое сообщение от прибора.
        /// </summary>
        /// <returns>
        /// <code>[0..7] - серийный номер прибора</code>
        /// <code>[8] - код состояния</code>
        /// <code>[9] - слово состояние флагов</code>
        /// </returns>

        public event Action<byte[]>? OnNewMessageFromSensor;
        public FakeDriver()
        {
            _sensors = new List<Sensor>()
            {
                new Sensor(18_446_744_073_709_551_615),
                new Sensor(18_446_744_073_709_551_614),
                new Sensor(00_000_000_000_000_000_003),
                new Sensor(18_446_744_073_709_551_613),
                new Sensor(00_000_000_000_000_000_002),
                new Sensor(18_446_744_073_709_551_612),
                new Sensor(00_000_000_000_000_000_001),
            };

            foreach (var sensor in _sensors)
            {
                sensor.OnStateChanged += Sensor_OnStateChanged;
            }
        }

        private void Sensor_OnStateChanged(byte[] message)
        {
            OnNewMessageFromSensor?.Invoke(message);
        }
        /// <summary>
        /// Отправить прибору сообщение. Сообщение должно содержать
        /// <code>message[0..7]- серийный номер прибора.</code> 
        /// <code>message[8] - выполняемая команда.</code>
        /// </summary>
        public void SendMessageToSensor(byte[] message)
        {
            foreach (var sensor in _sensors)
            {
                sensor.ExecuteCommand(message);
            }
        }
        /// <summary>
        /// Все серийные номера приборов, которые удалось обнаружить на данный момент.
        /// </summary>
        /// <returns>Последовательность серийных номеров</returns>
        public IEnumerable<ulong> GetAllSerialNumbers()
        {
            return _sensors.Select(d => d.SerialNumber);
        }
    }
}
