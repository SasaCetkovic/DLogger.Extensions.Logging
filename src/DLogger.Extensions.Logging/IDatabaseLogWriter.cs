using DLogger.Extensions.Logging;
using System.Collections.Generic;

namespace DLogger.Extensions.Logging
{
    public interface IDatabaseLogWriter
    {
		/// <summary>
		/// Connection string for the logging database
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		/// Writes a collection of log records to database at once
		/// </summary>
		/// <param name="logs">List of <see cref="LogRecord"/> objects</param>
		/// <param name="lockObject">Locking object</param>
		/// <param name="flushingInProgress">Indicates if writing process is still in progress; set to false at the end of this method</param>
		void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress);

		/// <summary>
		/// Writes a single log record to database
		/// </summary>
		/// <param name="logRecord"><see cref="LogRecord"/> instance to be written</param>
		void WriteLog(LogRecord log);
	}
}
