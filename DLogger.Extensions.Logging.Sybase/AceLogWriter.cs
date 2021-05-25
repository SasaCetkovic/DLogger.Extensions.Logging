using DLogger.Extensions.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoNetCore.AseClient;


namespace DLogger.Extensions.Logging.Sybase
{
    public class AceLogWriter : ILogWriter
    {
        private readonly string _connectionString;

        public AceLogWriter(string connectionString)
        {
            _connectionString = connectionString;
        }



        public void WriteBulk(List<LogRecord> logs, object lockObject, ref bool flushingInProgress)
        {
            throw new Exception();
		}

        public void WriteLog(LogRecord log)
        {
            using (var connection = new AseConnection(_connectionString))
            using (var command = new AseCommand("LogRecordInsert", connection))
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.StoredProcedure;

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
