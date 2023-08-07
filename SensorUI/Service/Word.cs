using System;

namespace SensorUI.Service
{
    [Flags]
    public enum Word
    {
        None    = 0b_0000_0000,  // 0   Отсутствие какого либо флага
        Fire    = 0b_0000_0001,  // 1   Флаг пожара (true - есть пожар, false - нет пожара )
        Relay   = 0b_0000_0010,  // 2   Флаг реле   (true - реле включено, false - реле отключено)
        Test    = 0b_0000_0100,  // 4   Флаг теста  (true - прибор в режиме тестирования, false - прибор в рабочем режиме)
    }
}
