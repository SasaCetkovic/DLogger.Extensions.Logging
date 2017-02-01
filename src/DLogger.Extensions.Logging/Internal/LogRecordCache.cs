using DLogger.Extensions.Logging.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DLogger.Extensions.Logging.Internal
{
    internal static class LogRecordCache
    {
		private static readonly List<LogRecord> _logs;
		private static bool _flushingInProgress = false;
		private static object _lockObject = new object();

		public static bool IsEmpty { get { return _flushingInProgress || _logs.Count == 0; } }


		static LogRecordCache()
		{
			_logs = new List<LogRecord>();
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

		public static void Flush(ILogWriter writer)
		{
			if (!_flushingInProgress)
			{
				_flushingInProgress = true;
				Task.Run(() => writer.WriteBulk(_logs, _lockObject, ref _flushingInProgress));
			}
		}
	}
}
