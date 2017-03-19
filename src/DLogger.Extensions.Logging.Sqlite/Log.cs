using System;

namespace DLogger.Extensions.Logging.Sqlite
{
    public class Log
    {
		public long Id { get; set; }
		public DateTime LogTime { get; }
		public int EventId { get; set; }
		public string EventName { get; set; }
		public string LogLevel { get; set; }
		public string Category { get; set; }
		public string Scope { get; set; }
		public string Message { get; set; }
		public string Exception { get; set; }
	}
}
