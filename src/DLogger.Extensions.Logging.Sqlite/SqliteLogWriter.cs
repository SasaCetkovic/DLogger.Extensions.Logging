using DLogger.Extensions.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DLogger.Extensions.Logging.Sqlite
{
    public class SqliteLogWriter : ILogWriter
    {
        public SqliteLogWriter()
        {
        }

		public void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress)
		{
			throw new NotImplementedException();
		}

		public void WriteLog(LogRecord log)
		{
			throw new NotImplementedException();
		}
	}
}
