using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DLogger.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;
using FastMember;
using System.Threading;

namespace DLogger.Extensions.Logging
{
	public class SqlServerLogWriter : IDatabaseLogWriter
	{
		private Dictionary<string, string> _columnMappings;

		public string ConnectionString { get; set; }

		public SqlServerLogWriter(string connectionString)
		{
			ConnectionString = connectionString;
			_columnMappings = new Dictionary<string, string>
			{
				{ "EventId", "EventID" },
				{ "EventName", "EventName" },
				{ "LogLevel", "LogLevel" },
				{ "Category", "Category" },
				{ "LogTime", "LogTime" },
				{ "Scope", "Scope" },
				{ "Message", "Message" },
				{ "Exception", "Exception" }
			};
		}

		public void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress)
		{
			var lockTaken = false;
			var connection = new SqlConnection(ConnectionString);
			connection.Open();
			var transaction = connection.BeginTransaction();

			try
			{
				using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
				{
					bulkCopy.DestinationTableName = "Logging";
					foreach (var mapping in _columnMappings)
						bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);

					Monitor.TryEnter(lockObject, ref lockTaken);
					if (lockTaken)
					{
						using (var reader = ObjectReader.Create(logs, _columnMappings.Keys.ToArray()))
						{
							bulkCopy.BatchSize = logs.Count;
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
					logs.Clear();
					Monitor.Exit(lockObject);
				}

				transaction.Dispose();
				connection.Close();
				flushingInProgress = false;
			}
		}

		public void WriteLog(LogRecord log)
		{
			using (var connection = new SqlConnection(ConnectionString))
			using (var command = new SqlCommand("LogRecordInsert", connection))
			{
				connection.Open();
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@eventID", log.EventId);
				command.Parameters.AddWithValue("@eventName", log.EventName);
				command.Parameters.AddWithValue("@logLevel", log.LogLevel.ToString());
				command.Parameters.AddWithValue("@category", log.Category);
				command.Parameters.AddWithValue("@scope", log.Scope);
				command.Parameters.AddWithValue("@message", log.Message);
				command.Parameters.AddWithValue("@logTime", log.LogTime);
				command.Parameters.AddWithValue("@exception", log.Exception?.ToString());
				command.ExecuteNonQuery();
			}
		}
	}
}
