using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DLogger.Extensions.Logging.Contracts
{
	public interface ILoggerSettings
	{
		/// <summary>
		/// Gets the value indicating if log records should be written to a permanent storage in bulk
		/// </summary>
		bool BulkWrite { get; }

		/// <summary>
		/// Gets the maximum number of log records that should be kept in cache, before flushing them to permanent storage
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
		/// <returns>New <see cref="ILoggerSettings"/> instance</returns>
		ILoggerSettings Reload();

		/// <summary>
		/// Retrieves the configured minimum log level for the specified category
		/// </summary>
		/// <param name="category">Category name</param>
		/// <param name="level">Logging severity level</param>
		/// <returns>Success of the retrieval</returns>
		bool TryGetSwitch(string category, out LogLevel level);
	}
}
