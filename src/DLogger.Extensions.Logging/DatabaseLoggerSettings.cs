using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DLogger.Extensions.Logging
{
    public class DatabaseLoggerSettings : IDatabaseLoggerSettings
	{
		private IConfiguration _loggingConfiguration;
		private const int DefaultCacheSize = 100;


		#region IDatabaseLoggerSettings Properties

		/// <summary>
		/// Gets the token that propagates notifications about changes in configuration
		/// </summary>
		public IChangeToken ChangeToken { get; private set; }

		/// <summary>
		/// Gets the value indicating if log records should be witten to database in bulk
		/// </summary>
		public bool BulkWrite { get; }

		/// <summary>
		/// Gets the maximum number of log records that should be kept in cache, before flushing them to database
		/// </summary>
		public int BulkWriteCacheSize { get; }

		/// <summary>
		/// Gets the IncludeScopes setting
		/// </summary>
		public bool IncludeScopes { get; }

		#endregion


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="loggingConfiguration">The 'Logging' section of the configuration file</param>
		public DatabaseLoggerSettings(IConfiguration loggingConfiguration)
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
			BulkWriteCacheSize = int.TryParse(value, out cacheSize) ? cacheSize : DefaultCacheSize;

			// Retrieve IncludeScopes setting
			var includeScopes = false;
			value = _loggingConfiguration["IncludeScopes"];
			bool.TryParse(value, out includeScopes);
			IncludeScopes = includeScopes;
		}


		#region IDatabaseLoggerSettings Methods

		/// <summary>
		/// Reloads the configuration
		/// </summary>
		/// <returns>New <see cref="IDatabaseLoggerSettings"/> instance</returns>
		public IDatabaseLoggerSettings Reload()
		{
			ChangeToken = null;
			return new DatabaseLoggerSettings(_loggingConfiguration);
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
				return false;

			var value = switches[category];
			return Enum.TryParse(value, out level);
		}

		#endregion
	}
}
