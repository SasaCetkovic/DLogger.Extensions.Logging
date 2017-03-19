using DLogger.Extensions.Logging.Contracts;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace DLogger.Extensions.Logging.Postgres
{
	public class PostgresLogWriter : ILogWriter
    {
		private string _connectionString;

		public PostgresLogWriter(string connectionString)
        {
			_connectionString = connectionString;
		}

		public void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress)
		{
			var lockTaken = false;
			var exceptionThrown = false;
			var connection = new NpgsqlConnection(_connectionString);

			try
			{
				connection.Open();
				var commandFormat = "COPY \"Logging\" (\"LogLevel\",\"LogTime\",\"Message\",\"EventID\",\"EventName\",\"Category\",\"Scope\",\"Exception\") FROM STDIN BINARY";
				using (var writter = connection.BeginBinaryImport(commandFormat))
				{
					Monitor.TryEnter(lockObject, ref lockTaken);
					if (lockTaken)
					{
						foreach (var log in logs)
						{
							writter.StartRow();
							writter.Write(log.LogLevel, NpgsqlDbType.Varchar);
							writter.Write(log.LogTime, NpgsqlDbType.TimestampTZ);
							writter.Write(log.Message, NpgsqlDbType.Varchar);

							if (log.EventId != 0)      writter.WriteNull(); else writter.Write(log.EventId, NpgsqlDbType.Integer);
							if (log.EventName != null) writter.WriteNull(); else writter.Write(log.EventName, NpgsqlDbType.Varchar);
							if (log.Category != null)  writter.WriteNull(); else writter.Write(log.Category, NpgsqlDbType.Varchar);
							if (log.Scope != null)     writter.WriteNull(); else writter.Write(log.Scope, NpgsqlDbType.Varchar);
							if (log.Exception != null) writter.WriteNull(); else writter.Write(log.Exception, NpgsqlDbType.Varchar);
						}
					}
				}
			}
			catch
			{
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

				connection.Close();
				flushingInProgress = false;
			}
		}

		public void WriteLog(LogRecord log)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			using (var command = new NpgsqlCommand("LogRecordInsert", connection))
			{
				connection.Open();
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.AddWithValue("@loglevel", NpgsqlDbType.Varchar,     log.LogLevel.ToString());
				command.Parameters.AddWithValue("@logtime",  NpgsqlDbType.TimestampTZ, log.LogTime);
				command.Parameters.AddWithValue("@message",  NpgsqlDbType.Varchar,     log.Message);

				if (log.EventId != 0)      command.Parameters.AddWithValue("@eventid",   NpgsqlDbType.Integer, log.EventId);
				if (log.EventName != null) command.Parameters.AddWithValue("@eventname", NpgsqlDbType.Varchar, log.EventName);
				if (log.Category != null)  command.Parameters.AddWithValue("@category",  NpgsqlDbType.Varchar, log.Category);
				if (log.Scope != null)     command.Parameters.AddWithValue("@scope",     NpgsqlDbType.Varchar, log.Scope);
				if (log.Exception != null) command.Parameters.AddWithValue("@exception", NpgsqlDbType.Varchar, log.Exception.ToString());

				command.ExecuteNonQuery();
			}
		}
	}
}
