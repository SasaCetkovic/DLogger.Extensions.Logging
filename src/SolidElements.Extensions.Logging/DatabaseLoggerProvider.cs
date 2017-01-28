using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SolidElements.Extensions.Logging
{
	public class DatabaseLoggerProvider : ILoggerProvider
	{
		#region Private Fields

		private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new ConcurrentDictionary<string, DatabaseLogger>();

		private string _connectionString;
		private IDatabaseLoggerSettings _settings;

		#endregion


		#region Constructor

		public DatabaseLoggerProvider(IDatabaseLoggerSettings settings, string connectionString)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_settings = settings;
			_connectionString = connectionString;

			if (_settings.ChangeToken != null)
			{
				_settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
			}
		}

		#endregion


		#region ILoggerProvider Implementation

		public ILogger CreateLogger(string category)
		{
			var logger = new DatabaseLogger(category, GetFilter(category), _connectionString, _settings.BulkWrite, _settings.BulkWriteCacheSize);
			return _loggers.GetOrAdd(category, logger);
		}

		public void Dispose()
		{
			if (!LogRecordCache.IsEmpty)
			{
				LogRecordCache.Flush(_connectionString);
			}
		}

		#endregion


		#region Helper Methods

		private void OnConfigurationReload(object state)
		{
			// Creating a new settings object, because the old one is probably holding on to an old change token.
			_settings = _settings.Reload();

			if (!LogRecordCache.IsEmpty)
			{
				LogRecordCache.Flush(_connectionString);
			}

			foreach (var logger in _loggers.Values)
			{
				logger.Filter = GetFilter(logger.Category);
				logger.BulkWrite = _settings.BulkWrite;
				logger.BulkWriteCacheSize = _settings.BulkWriteCacheSize;
			}

			// The token will change each time it reloads, so we need to register again.
			if (_settings?.ChangeToken != null)
			{
				_settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
			}
		}

		private Func<string, LogLevel, bool> GetFilter(string category)
		{
			if (_settings != null)
			{
				foreach (var prefix in GetKeyPrefixes(category))
				{
					LogLevel level;
					if (_settings.TryGetSwitch(prefix, out level))
					{
						return (cat, lev) => lev >= level;
					}
				}
			}

			return (cat, lev) => false;
		}

		private IEnumerable<string> GetKeyPrefixes(string category)
		{
			while (!string.IsNullOrEmpty(category))
			{
				yield return category;
				var lastIndexOfDot = category.LastIndexOf('.');
				if (lastIndexOfDot == -1)
				{
					yield return "Default";
					break;
				}

				category = category.Substring(0, lastIndexOfDot);
			}
		}

		#endregion
	}
}
