using FastMember;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DLogger.Extensions.Logging
{
    internal static class LogRecordCache
    {
		private static readonly List<LogRecord> _logs;
		private static readonly Dictionary<string, string> _columnMappings;
		private static bool _flushingInProgress = false;
		private static object _lockObject = new object();

		public static bool IsEmpty { get { return _flushingInProgress || _logs.Count == 0; } }


		static LogRecordCache()
		{
			_logs = new List<LogRecord>();
			_columnMappings = new Dictionary<string, string>
			{
				{ "EventId", "EventID" },
				{ "EventName", "EventName" },
				{ "LogLevel", "LogLevel" },
				{ "Category", "Category" },
				{ "LogTime", "LogTime" },
				{ "Message", "Message" },
				{ "Exception", "Exception" }
			};
		}


		public static void Add(LogRecord log)
		{
			if (_flushingInProgress)
			{
				lock (_lockObject)
				{
					_logs.Add(log);
				}
			}
			else
			{
				_logs.Add(log);
			}
		}

		public static bool IsFull(int maxCount)
		{
			return !_flushingInProgress && _logs.Count >= maxCount;
		}

		public static void Flush(string connectionString)
		{
			if (!_flushingInProgress)
			{
				_flushingInProgress = true;
				Task.Run(() => WriteLogsToDatabase(connectionString));
			}
		}

		private static void WriteLogsToDatabase(string connectionString)
		{
			var lockTaken = false;
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var transaction = connection.BeginTransaction();

			try
			{
				using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
				{
					bulkCopy.DestinationTableName = "Logging";
					foreach (var mapping in _columnMappings)
						bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);

					Monitor.TryEnter(_lockObject, ref lockTaken);
					if (lockTaken)
					{
						using (var reader = ObjectReader.Create(_logs, _columnMappings.Keys.ToArray()))
						{
							bulkCopy.BatchSize = _logs.Count;
							bulkCopy.WriteToServer(reader);
						}

						transaction.Commit();
					}
				}
			}
			catch
			{
				if (connection.State == ConnectionState.Open)
					transaction.Rollback();
				throw;
			}
			finally
			{
				if (lockTaken)
				{
					_logs.Clear();
					Monitor.Exit(_lockObject);
				}

				transaction.Dispose();
				connection.Close();
				_flushingInProgress = false;
			}
		}
	}
}
