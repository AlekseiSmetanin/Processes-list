using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Список_процессов___основной_проект
{
    /// <summary>
    /// Класс-обёртка для работы с функциями, экспортируемыми из ProcessProperties.dll
    /// </summary>
    static class ProcessPropertiesWrapper
    {
        /// <summary>
        /// Возвращает уровень целостности процесса
        /// </summary>
        /// <param name="PID">PID процесса</param>
        /// <returns>
        /// 1 - низкий уровень целостности
        /// 2 - средний уровень целостности
        /// 3 - высокий уровень целостности
        /// 4 - системный уровень целостности
        /// Неположительное значение свидетельствует об ошибке
        /// </returns>
        //[DllImport("../../../../x64/Debug/ProcessProperties.dll")]
        [DllImport("ProcessProperties.dll")]
        public static extern int GetProcessIntegrityLevel(int PID);

        /// <summary>
        /// Возвращает PID родительского процесса
        /// </summary>
        /// <param name="pid">PID процесса</param>
        /// <returns>
        /// PID родительского процесса
        /// Неположительное значение свидетельствует об ошибке
        /// </returns>
        [DllImport("ProcessProperties.dll")]
        public static extern int GetParentPID(int pid);

        /// <summary>
        /// Определяет, используется ли процессом технология DEP
        /// </summary>
        /// <param name="pid">PID процесса</param>
        /// <returns>
        /// 1 - технология DEP используется процессом
        /// </returns>
        [DllImport("ProcessProperties.dll")]
        public static extern int IsDEP_Using(int pid);

        /// <summary>
        /// Определяет, используется ли процессом технология ASLR
        /// </summary>
        /// <param name="pid">PID процесса</param>
        /// <returns>
        /// 1 - технология ASLR используется процессом
        /// </returns>
        [DllImport("ProcessProperties.dll")]
        public static extern int IsASLR_Using(int pid);
    }
}
