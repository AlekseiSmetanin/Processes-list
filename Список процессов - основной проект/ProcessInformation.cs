using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace Список_процессов___основной_проект
{
    /// <summary>
    /// Информация о процессе
    /// </summary>
    public class ProcessInformation 
    {
        private Process process;
        private int? parentPID;
        private string parentName;

        public ProcessInformation(Process process)
        {
            this.process = process;
            PID = process.Id;
            Name = process.ProcessName;
        }

        public ProcessInformation(ProcessInformation processInformation)
        {
            process = processInformation.process;
            PID = processInformation.PID;
            Name = processInformation.Name;
        }

        /// <summary>
        /// Идентификатор процесса
        /// </summary>
        public int PID { private set; get; }

        /// <summary>
        /// Имя процесса
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// Путь к исполняемому файлу процесса
        /// </summary>
        public string Path
        {
            get
            {
                return process.MainModule.FileName;
            }
        }

        /// <summary>
        /// Идентификатор родительского процесса
        /// </summary>
        public int ParentPID
        {
            get
            {
                if (parentPID == null)
                {
                    parentPID = ProcessPropertiesWrapper.GetParentPID(PID);

                    if (parentPID < 0)
                        throw new Exception("Ошибка при получении PPID");
                }

                return (int)parentPID;
            }
        }

        /// <summary>
        /// Имя родительского процесса
        /// </summary>
        public string ParentName
        {
            get
            {
                if (parentName==null)
                {
                    parentName = Array.Find<ProcessInformation>(GetProcessesInformation(), pi => pi.PID == ParentPID).Name;
                }

                return parentName;
            }
        }

        /// <summary>
        /// Список DLL, используемых процессом
        /// </summary>
        public ProcessModuleCollection DLL_List
        {
            get
            {
                return process.Modules;
            }
        }

        /// <summary>
        /// Уровень целостности процесса
        /// </summary>
        public string IntegrityLevel
        {
            get
            {
                int integrityLevel=ProcessPropertiesWrapper.GetProcessIntegrityLevel(PID);
                if (integrityLevel <= 0)
                    throw new Exception(String.Format("Уровень целостности процесса с PID = {0} не определён", PID));
                string result="";

                switch (integrityLevel)
                {
                    case 1:
                        result = "Low";
                        break;
                    case 2:
                        result = "Medium";
                        break;
                    case 3:
                        result = "High";
                        break;
                    case 4:
                        result = "System";
                        break;
                    default:
                        break;
                }

                return result;
            }
        }

        /// <summary>
        /// Использование DEP и ASLR процессом
        /// </summary>
        public string DEP_and_ASLR
        {
            get
            {
                String str = "";

                if (ProcessPropertiesWrapper.IsDEP_Using(PID) == 1)
                {
                    str += "DEP";

                    if (ProcessPropertiesWrapper.IsASLR_Using(PID) == 1)
                        str += "; ASLR";
                }
                else if (ProcessPropertiesWrapper.IsASLR_Using(PID) == 1)
                    str += "ASLR";

                return str;
            }
        }

        /// <summary>
        /// Получает информацию обо всех процессах, существующих в данный момент в операционной системе
        /// </summary>
        /// <returns>Массив объектов ProcessInformation, соответствующих существующим процессам</returns>
        public static ProcessInformation[] GetProcessesInformation()
        {
            Process[] processes = Process.GetProcesses();

            ProcessInformation[] processesInformation = new ProcessInformation[processes.Length];
            for (int i = 0; i < processesInformation.Length; i++)
            {
                processesInformation[i] = new ProcessInformation(processes[i]);
            }

            return processesInformation;
        }
    }
}
