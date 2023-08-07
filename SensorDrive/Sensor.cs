using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorDrive
{
    public class Sensor
    {
        private byte state;     //  0 = автоматика, 1 = ручное, 2 = отключен
        private bool fire;      //  Состояние об наличии пожара
        private bool relay;     //  Состояние реле
        private bool test;      //  Состояние режима тестирования

        /// <summary>
        /// Событие изменения состояний прибора. Ответ содержит
        /// <code>[0..7] - серийный номер прибора</code>
        /// <code>[8] - код состояния</code>
        /// <code>[9] - слово состояние флагов</code>
        /// </summary>
        public event Action<byte[]>? OnStateChanged;

        /// <summary>
        /// Серийный номер прибора
        /// </summary>
        public ulong SerialNumber { get; init; }

        public Sensor(ulong serialNumber)
        {
            SerialNumber = serialNumber;
        }

        public byte State
        {
            get => state;
            private set
            {
                state = value;
                StateChangeEvent(Package());
            }
        }
        public bool Fire
        {
            get => fire;
            private set
            {
                fire = value;
                StateChangeEvent(Package());
            }
        }
        public bool Relay
        {
            get => relay;
            private set
            {
                relay = value;
                StateChangeEvent(Package());
            }
        }
        public bool Test
        {
            get => test;
            private set
            {
                test = value;
                StateChangeEvent(Package());
            }
        }

        private void StateChangeEvent(byte[] state)
        {
            OnStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Прибор исполняет команды при вызове этого метода.
        /// <code>pocket[0..7]- серийный номер прибора.</code> 
        /// <code>pocket[8] - выполняемая команда.</code>
        /// </summary>
        public void ExecuteCommand(byte[] pocket)
        {
            //  Если серийный номер не совпал с имеющимся, посылка пришла не по адресу. Завершаем метод.
            if (SerialNumber != BitConverter.ToUInt64(pocket.AsSpan(0,8))) return;

            SetState(pocket[8]);
            StateChangeEvent(Package());
        }

        private void SetState(byte command)
        {
            switch (command)
            {
                case 0: state = 1; break;       //  (Перевод в ручной режим) Перевод в ручной режим (Этой команды в описании нет. Добавил её от себя, потому как режим ручного управления будет не достижим.)
                case 1: state = 2; break;       //  (Отключить) Отключить
                case 2: state = 0; break;       //  (Перевод в автоматику) Перевод в автоматику
                case 3: test = true; break;     //  (Перевод в тестовый режим) Снять тест
                case 4: state = 0;              //  (Сброс состояния) Сброс состояния (Под вопросом. Не понятно что тут нужно делать.)
                        relay = false;          //  Сбрасываем все состояния на значения по умолчанию типов.
                        fire = false;
                        test = false;
                    break;
                case 5: relay = true; break;    //  (Включить реле) Включить реле
                case 6: relay = false; break;   //  (Отключить реле) Отключить реле
                default:                        //  (Опрос состояния) Любая не распознанная команда приведёт к ответу устройства об своём текущем состоянии.
                    break;
            }
        }
        private byte[] Package()
        {
            byte[] result = new byte[10];

            var b = BitConverter.GetBytes(SerialNumber);

            for (int i = 0; i < 8; i++)
                result[i] = b[i];

            result[8] += (byte) (Fire ? 1 : 0);
            result[8] += (byte) (Relay ? 2 : 0);
            result[8] += (byte) (Test ? 4 : 0);

            result[9] = State;

            return result;
        }
    }
}
