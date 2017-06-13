using DLogger.Extensions.Logging.Contracts;
using FastMember;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace DLogger.Extensions.Logging
{
	/// <summary>
	/// <see cref="ILogWriter"/> implementation for storing log records to SQL Server
	/// </summary>
	public class SqlServerLogWriter : ILogWriter
	{
		private readonly Dictionary<string, string> _columnMappings;
		private readonly string _connectionString;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlServerLogWriter"/> class
		/// </summary>
		/// <param name="connectionString">Connection string for the logging database</param>
		public SqlServerLogWriter(string connectionString)
		{
			_connectionString = connectionString;
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

		/// <summary>
		/// Writes a collection of log records to database at once
		/// </summary>
		/// <param name="logs">List of <see cref="LogRecord"/> objects</param>
		/// <param name="lockObject">Locking object</param>
		/// <param name="flushingInProgress">Indicates if writing process is still in progress; set to false at the end of this method</param>
		public void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress)
		{
			var lockTaken = false;
			var exceptionThrown = false;
			var connection = new SqlConnection(_connectionString);
			connection.Open();
			var transaction = connection.BeginTransaction();

			try
			{
				using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
				{
					bulkCopy.DestinationTableName = "Logging";
					foreach (var mapping in _columnMappings)
					{
						bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
					}

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
				{
					transaction.Rollback();
				}

				exceptionThrown = true;
			}
			finally
			{
				if (lockTaken)
				{
					if (!exceptionThrown)
					{
						logs.Clear();
					}
					else
					{
						// Drop the older half of log records to prevent OutOfMemoryException
						logs.RemoveRange(0, logs.Count / 2);
					}

					Monitor.Exit(lockObject);
				}

				transaction.Dispose();
				connection.Close();
				flushingInProgress = false;
			}
		}

		/// <summary>
		/// Writes a single log record to database
		/// </summary>
		/// <param name="log"><see cref="LogRecord"/> instance to be written</param>
		public void WriteLog(LogRecord log)
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
				command.Parameters.AddWithValue("@scope", log.Scope);
				command.Parameters.AddWithValue("@message", log.Message);
				command.Parameters.AddWithValue("@logTime", log.LogTime);
				command.Parameters.AddWithValue("@exception", log.Exception?.ToString());
				command.ExecuteNonQuery();
			}
		}
	}
}
