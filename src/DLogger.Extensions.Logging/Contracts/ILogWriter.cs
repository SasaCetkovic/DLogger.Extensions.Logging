using System.Collections.Generic;

namespace DLogger.Extensions.Logging.Contracts
{
	/// <summary>
	/// Represents a type used to save log records to a permanent storage
	/// </summary>
    public interface ILogWriter
    {
		/// <summary>
		/// Writes a collection of log records to permanent storage at once
		/// </summary>
		/// <param name="logs">List of <see cref="LogRecord"/> objects</param>
		/// <param name="lockObject">Locking object</param>
		/// <param name="flushingInProgress">Indicates if writing process is still in progress; set to false at the end of this method</param>
		/// <remarks>
		/// This method is called asynchronously with 'fire and forget' approach,
		/// the lockObject and flushingInProgress parameters are important for thread safety
		/// </remarks>
		void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress);

		/// <summary>
		/// Writes a single log record to permanent storage
		/// </summary>
		/// <param name="log"><see cref="LogRecord"/> instance to be written</param>
		void WriteLog(LogRecord log);
	}
}
