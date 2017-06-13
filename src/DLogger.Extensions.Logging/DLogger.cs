using DLogger.Extensions.Logging.Contracts;
using DLogger.Extensions.Logging.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace DLogger.Extensions.Logging
{
	public class DLogger : ILogger
	{
		private readonly ILogWriter _writer;
		private Func<string, LogLevel, bool> _filter;

		public DLogger(string category, Func<string, LogLevel, bool> filter, ILogWriter writer, ILoggerSettings settings)
		{
			_writer = writer;
			_filter = filter;
			Category = category;
			Settings = settings;
		}


		#region Properties

		/// <summary>
		/// Gets the log category
		/// </summary>
		public string Category { get; }

		/// <summary>
		/// Gets or sets the <see cref="ILoggerSettings"/> instance
		/// </summary>
		public ILoggerSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the delegate used for filtering whether a log record should be discarded
		/// </summary>
		public Func<string, LogLevel, bool> Filter
		{
			get => _filter;
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


		#region ILogger Implementation

		public IDisposable BeginScope<TState>(TState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return LogScope.Push(state.ToString());
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return Filter(Category, logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var log = new LogRecord(eventId.Id, eventId.Name, logLevel, Category, GetScope(), state.ToString(), exception);

			if (Settings.BulkWrite)
			{
				LogRecordCache.Add(log);

				if (LogRecordCache.IsFull)
				{
					LogRecordCache.Flush(_writer);
				}
			}
			else
			{
				_writer.WriteLog(log);
			}
		}

		#endregion


		#region Helpers

		private string GetScope()
		{
			if (!Settings.IncludeScopes)
			{
				return null;
			}

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
