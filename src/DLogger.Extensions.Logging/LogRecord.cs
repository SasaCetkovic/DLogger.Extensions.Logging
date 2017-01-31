using Microsoft.Extensions.Logging;
using System;

namespace DLogger.Extensions.Logging
{
    public class LogRecord
    {
		public LogRecord(int eventId, string eventName, LogLevel logLevel, string categoryName, string scope, string message, Exception exception = null)
		{
			EventId = eventId;
			EventName = eventName;
			LogLevel = logLevel;
			Category = categoryName;
			Scope = scope;
			Message = message;
			Exception = exception;
			LogTime = DateTime.Now;
		}

		public int EventId { get; }

		public string EventName { get; }

		public LogLevel LogLevel { get; }

		public string Category { get; }

		public DateTime LogTime { get; }

		public string Scope { get; }

		public string Message { get; }

		public Exception Exception { get; }
	}
}
