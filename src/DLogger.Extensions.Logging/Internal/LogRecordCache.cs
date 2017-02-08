using DLogger.Extensions.Logging.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DLogger.Extensions.Logging.Internal
{
    internal static class LogRecordCache
    {
		private static readonly List<LogRecord> _logs;
		private static int _maxCacheSize;
		private static bool _flushingInProgress = false;
		private static object _lockObject = new object();

		public static bool IsEmpty { get { return _flushingInProgress || _logs.Count == 0; } }

		public static bool IsFull { get { return !_flushingInProgress && _logs.Count >= _maxCacheSize; } }


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

		public static void SetCapacity(int cacheSize)
		{
			_maxCacheSize = cacheSize;
			_logs.Capacity = cacheSize * 2;
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
