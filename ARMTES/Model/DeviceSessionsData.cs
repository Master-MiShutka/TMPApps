using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.ARMTES.Model
{
    /// <summary>
    /// История сеансов связи с устройством
    /// </summary>
    public class DeviceSessionsData
    {
        /// <summary>
        /// Время сеанса
        /// </summary>
        public string HandshakeTime { get; set; }
        /// <summary>
        /// Продолжительность сеанса
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// Число перезапросов
        /// </summary>
        public string AttemptsCount { get; set; }
        /// <summary>
        /// Номер цикла опроса
        /// </summary>
        public string RepeatCycleNumber { get; set; }
        /// <summary>
        /// Результат сеанса
        /// </summary>
        public string DeviceScanningSessionResult { get; set; }
    }
}
