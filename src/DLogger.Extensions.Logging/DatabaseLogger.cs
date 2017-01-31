using System;
using Microsoft.Extensions.Logging;
using DLogger.Extensions.Logging.Internal;
using System.Text;

namespace DLogger.Extensions.Logging
{
	public class DatabaseLogger : ILogger
	{
		private string _connectionString;
		private Func<string, LogLevel, bool> _filter;
		private IDatabaseLogWriter _writer;


		#region Properties

		/// <summary>
		/// Log category
		/// </summary>
		public string Category { get; }

		public IDatabaseLoggerSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the delegate used for filtering whether a log record should be discarded
		/// </summary>
		public Func<string, LogLevel, bool> Filter
		{
			get { return _filter; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_filter = value;
			}
		}

		#endregion


		public DatabaseLogger(string category, Func<string, LogLevel, bool> filter, IDatabaseLogWriter writer, IDatabaseLoggerSettings settings)
		{
			_writer = writer;
			_filter = filter;
			Category = category;
			Settings = settings;
		}


		#region ILogger Implementation

		public IDisposable BeginScope<TState>(TState state)
		{
			if (state == null)
				throw new ArgumentNullException(nameof(state));

			return LogScope.Push(state.ToString());
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return Filter(Category, logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel)) return;

			var log = new LogRecord(eventId.Id, eventId.Name, logLevel, Category, GetScope(), state.ToString(), exception);

			if (Settings.BulkWrite)
				WriteBulk(log);
			else
				_writer.WriteLog(log);
		}

		#endregion


		#region Helpers

		private void WriteBulk(LogRecord log)
		{
			LogRecordCache.Add(log);

			if (LogRecordCache.IsFull(Settings.BulkWriteCacheSize))
			{
				LogRecordCache.Flush(_writer);
			}
		}

		private string GetScope()
		{
			if (!Settings.IncludeScopes)
				return null;

			var current = LogScope.Current;
			var scope = new StringBuilder();

			while (current != null)
			{
				scope.Append(current);

				if (current.Parent != null)
				{
					scope.AppendLine();
					scope.Append("=> ");
				}

				current = current.Parent;
			}

			return scope.ToString();
		}

		#endregion
	}
}
