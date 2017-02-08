using Microsoft.Extensions.Logging;
using System;

namespace DLogger.Extensions.Logging.Contracts
{
	/// <summary>
	/// Entity for keeping single log entry data
	/// </summary>
	public class LogRecord
    {
		/// <summary>
		/// Default constructor
		/// </summary>
		public LogRecord()
		{
			LogTime = DateTime.Now;
		}

		/// <summary>
		/// Recommended constructor
		/// </summary>
		/// <param name="eventId">Event ID</param>
		/// <param name="eventName">Event name</param>
		/// <param name="logLevel">Severity level</param>
		/// <param name="categoryName">Category name</param>
		/// <param name="scope">Logging scope</param>
		/// <param name="message">Log record message</param>
		/// <param name="exception">Caught exception</param>
		public LogRecord(int eventId, string eventName, LogLevel logLevel, string categoryName, string scope, string message, Exception exception = null)
		{
			LogTime = DateTime.Now;
			EventId = eventId;
			EventName = eventName;
			LogLevel = logLevel;
			Category = categoryName;
			Scope = scope;
			Message = message;
			Exception = exception;
		}

		/// <summary>
		/// Log record time stamp
		/// </summary>
		public DateTime LogTime { get; }

		/// <summary>
		/// Event ID
		/// </summary>
		public int EventId { get; set; }

		/// <summary>
		/// Event name
		/// </summary>
		public string EventName { get; set; }

		/// <summary>
		/// Severity level
		/// </summary>
		public LogLevel LogLevel { get; set; }

		/// <summary>
		/// Category name
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// Logging scope
		/// </summary>
		public string Scope { get; set; }

		/// <summary>
		/// Log record message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Caught exception
		/// </summary>
		public Exception Exception { get; set; }
	}
}
