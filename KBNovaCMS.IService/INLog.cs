using KBNovaCMS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBNovaCMS.IService
{
    public interface INLog : IDisposable
    {
        void Log(NLogType logType, string logMessage, Exception? exception = null, NLogErrorFileName errorFileName = NLogErrorFileName.InfoLog);
        Task LogAsync(NLogType logType, string logMessage, Exception? exception = null, NLogErrorFileName errorFileName = NLogErrorFileName.InfoLog);
    }
}
