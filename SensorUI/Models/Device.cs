using System;
using System.Linq;

namespace SensorUI.Models
{
    public class Device : IDevice
    {
        private readonly string serialNumberFormat = "Serial number:[ 00-000-000-000-000-000-000 ]";

        private byte state;     //  0 = автоматика, 1 = ручное, 2 = отключен
        private bool fire;      //  Состояние об наличии пожара
        private bool relay;     //  Состояние реле
        private bool test;      //  Состояние режима тестирования

        /// <summary>
        /// Событие изменения режима работы
        /// </summary>
        public event Action<byte>? OnStateChanged;
        /// <summary>
        /// Событие изменения состояния об наличии пожара
        /// </summary>
        public event Action<bool>? OnFire;
        /// <summary>
        /// Событие об изменении состояния реле
        /// </summary>
        public event Action<bool>? OnRelay;
        /// <summary>
        /// Событие об изменении состояния режима тестирования
        /// </summary>
        public event Action<bool>? OnTest;

        /// <summary>
        /// Серийный номер прибора
        /// </summary>
        public ulong SerialNumber { get; init; }

        public Device(ulong serialNumber)
        {
            SerialNumber = serialNumber;
        }

        public byte State
        {
            get => state;
            set
            {
                state = value;
                StateChangeEvent(state);
            }
        }
        public bool Fire
        {
            get => fire;
            set
            {
                fire = value;
                FireChangeEvent(fire);
            }
        }
        public bool Relay
        {
            get => relay;
            set
            {
                relay = value;
                RelayChangeEvent(relay);
            }
        }
        public bool Test
        {
            get => test;
            set
            {
                test = value;
                TestChangeEvent(test);
            }
        }

        private void StateChangeEvent(byte state) => OnStateChanged?.Invoke(state);
        private void FireChangeEvent(bool state) => OnFire?.Invoke(state);
        private void RelayChangeEvent(bool state) => OnRelay?.Invoke(state);
        private void TestChangeEvent(bool state) => OnTest?.Invoke(state);

        public ulong GetSerialNumber() => SerialNumber;

        public string GetDeviceDescription()    //  Например: «[1122334] Ручное (Пожар, Реле включено)»
        {
            string[] strings = { Fire ?  "Пожар"         : string.Empty,
                                 Relay ? "Реле включено" : string.Empty,
                                 Test ?  "Тест"          : string.Empty
                               };

            string word = string.Join(", ", strings.Where(s => s != string.Empty));

            return $"{SerialNumber.ToString(serialNumberFormat)}\t{(State)State}\t({word})";
        }
    }
}
