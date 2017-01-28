using System;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;

namespace SolidElements.Extensions.Logging
{
	public class DatabaseLogger : ILogger
	{
		private string _connectionString;
		private Func<string, LogLevel, bool> _filter;


		#region Properties

		/// <summary>
		/// Gets or sets the value indicating if log records should be witten to database in bulk
		/// </summary>
		public bool BulkWrite { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of log records that should be kept in cache, before flushing them to database
		/// </summary>
		public int BulkWriteCacheSize { get; set; }

		/// <summary>
		/// Log category
		/// </summary>
		public string Category { get; }

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


		public DatabaseLogger(string category, Func<string, LogLevel, bool> filter, string connectionString, bool bulkWrite, int bulkWriteCacheSize)
		{
			_connectionString = connectionString;
			_filter = filter;
			Category = category;
			BulkWrite = bulkWrite;
			BulkWriteCacheSize = bulkWriteCacheSize;
		}


		#region ILogger Implementation

		public IDisposable BeginScope<TState>(TState state)
		{
			// TODO: Implement scopes; nothing to dispose of for now
            return NullDisposable.Instance;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return Filter(Category, logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel)) return;

			var log = new LogRecord(eventId.Id, eventId.Name, logLevel, Category, state.ToString(), exception);

			if (BulkWrite)
				WriteBulk(log);
			else
				WriteLog(log);
		}

		#endregion


		#region Helpers

		private void WriteBulk(LogRecord log)
		{
			LogRecordCache.Add(log);

			if (LogRecordCache.IsFull(BulkWriteCacheSize))
			{
				LogRecordCache.Flush(_connectionString);
			}
		}

		private void WriteLog(LogRecord log)
		{
			using (var connection = new SqlConnection(_connectionString))
			using (var command = new SqlCommand("LogRecordInsert", connection))
			{
				connection.Open();
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@eventID", log.EventId);
				command.Parameters.AddWithValue("@eventName", log.EventName);
				command.Parameters.AddWithValue("@logLevel", log.LogLevel.ToString());
				command.Parameters.AddWithValue("@category", log.Category);
				command.Parameters.AddWithValue("@message", log.Message);
				command.Parameters.AddWithValue("@logTime", log.LogTime);
				command.Parameters.AddWithValue("@exception", log.Exception?.ToString());
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
