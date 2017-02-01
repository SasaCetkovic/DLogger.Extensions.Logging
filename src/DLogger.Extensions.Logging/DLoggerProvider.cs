using DLogger.Extensions.Logging.Contracts;
using DLogger.Extensions.Logging.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DLogger.Extensions.Logging
{
	public class DLoggerProvider : ILoggerProvider
	{
		#region Private Fields

		private readonly ConcurrentDictionary<string, DLogger> _loggers = new ConcurrentDictionary<string, DLogger>();

		private ILoggerSettings _settings;
		private ILogWriter _writer;

		#endregion


		public DLoggerProvider(ILoggerSettings settings, ILogWriter writer)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_settings = settings;
			_writer = writer;

			if (_settings.ChangeToken != null)
			{
				_settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
			}
		}


		#region ILoggerProvider Implementation

		public ILogger CreateLogger(string category)
		{
			var logger = new DLogger(category, GetFilter(category), _writer, _settings);
			return _loggers.GetOrAdd(category, logger);
		}

		public void Dispose()
		{
			if (!LogRecordCache.IsEmpty)
			{
				LogRecordCache.Flush(_writer);
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
				LogRecordCache.Flush(_writer);
			}

			foreach (var logger in _loggers.Values)
			{
				logger.Filter = GetFilter(logger.Category);
				logger.Settings = _settings;
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
