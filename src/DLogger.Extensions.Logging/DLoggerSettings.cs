using DLogger.Extensions.Logging.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;

namespace DLogger.Extensions.Logging
{
	/// <summary>
	/// Default <see cref="ILoggerSettings"/> implementation
	/// </summary>
	public class DLoggerSettings : ILoggerSettings
	{
		private const int _defaultCacheSize = 100;
		private readonly IConfiguration _loggingConfiguration;

		/// <summary>
		/// Initializes a new instance of the <see cref="DLoggerSettings"/> class
		/// </summary>
		/// <param name="loggingConfiguration">The 'Logging' section of the configuration file</param>
		public DLoggerSettings(IConfiguration loggingConfiguration)
		{
			_loggingConfiguration = loggingConfiguration;

			ChangeToken = loggingConfiguration.GetReloadToken();

			// Set bulk write properties
			var bulkWrite = false;
			var value = _loggingConfiguration["BulkWrite"];
			bool.TryParse(value, out bulkWrite);
			BulkWrite = bulkWrite;

			int cacheSize;
			value = _loggingConfiguration["BulkWriteCacheSize"];
			BulkWriteCacheSize = int.TryParse(value, out cacheSize) ? cacheSize : _defaultCacheSize;

			// Retrieve IncludeScopes setting
			var includeScopes = false;
			value = _loggingConfiguration["IncludeScopes"];
			bool.TryParse(value, out includeScopes);
			IncludeScopes = includeScopes;
		}


		#region ILoggerSettings Properties

		/// <summary>
		/// Gets the token that propagates notifications about changes in configuration
		/// </summary>
		public IChangeToken ChangeToken { get; private set; }

		/// <summary>
		/// Gets a value indicating whether log records should be witten in bulk
		/// </summary>
		public bool BulkWrite { get; }

		/// <summary>
		/// Gets the maximum number of log records that should be kept in cache, before flushing them to permanent storage
		/// </summary>
		public int BulkWriteCacheSize { get; }

		/// <summary>
		/// Gets a value indicating whether the logger should include scope information
		/// </summary>
		public bool IncludeScopes { get; }

		#endregion


		#region ILoggerSettings Methods

		/// <summary>
		/// Reloads the configuration
		/// </summary>
		/// <returns>New <see cref="ILoggerSettings"/> instance</returns>
		public ILoggerSettings Reload()
		{
			ChangeToken = null;
			return new DLoggerSettings(_loggingConfiguration);
		}

		/// <summary>
		/// Retrieves the configured minimum log level for the specified category
		/// </summary>
		/// <param name="category">Category name</param>
		/// <param name="level">Logging severity level</param>
		/// <returns>Success of the retrieval</returns>
		public bool TryGetSwitch(string category, out LogLevel level)
		{
			level = LogLevel.None;

			var switches = _loggingConfiguration.GetSection("LogLevel");
			if (switches == null)
			{
				return false;
			}

			var value = switches[category];
			return Enum.TryParse(value, out level);
		}

		#endregion
	}
}
