using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DLogger.Extensions.Logging
{
	public interface IDatabaseLoggerSettings
	{
		/// <summary>
		/// Gets the value indicating if log records should be witten to database in bulk
		/// </summary>
		bool BulkWrite { get; }

		/// <summary>
		/// Gets the maximum number of log records that should be kept in cache, before flushing them to database
		/// </summary>
		int BulkWriteCacheSize { get; }

		/// <summary>
		/// Gets the token that propagates notifications about changes in configuration
		/// </summary>
		IChangeToken ChangeToken { get; }

		/// <summary>
		/// Gets the IncludeScopes setting
		/// </summary>
		bool IncludeScopes { get; }

		/// <summary>
		/// Reloads the configuration
		/// </summary>
		/// <returns>New <see cref="IDatabaseLoggerSettings"/> instance</returns>
		IDatabaseLoggerSettings Reload();

		/// <summary>
		/// Retrieves the configured minimum log level for the specified category
		/// </summary>
		/// <param name="category">Category name</param>
		/// <param name="level">Logging severity level</param>
		/// <returns>Success of the retrieval</returns>
		bool TryGetSwitch(string category, out LogLevel level);
	}
}
