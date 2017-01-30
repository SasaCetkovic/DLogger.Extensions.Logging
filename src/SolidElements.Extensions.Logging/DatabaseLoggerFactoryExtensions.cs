using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DLogger.Extensions.Logging
{
    public static class DatabaseLoggerFactoryExtensions
    {
		/// <summary>
		/// Adds custom database logging provider
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="settings"></param>
		/// <param name="connectionString">Database connection string</param>
		/// <returns></returns>
		public static ILoggerFactory AddDatabaseLogger(this ILoggerFactory factory, IDatabaseLoggerSettings settings, string connectionString)
		{
			factory.AddProvider(new DatabaseLoggerProvider(settings, connectionString));
			return factory;
		}

		/// <summary>
		/// Adds custom database logging provider
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="loggingConfiguration">Appropriate configuration section</param>
		/// <param name="connectionString">Database connection string</param>
		/// <returns></returns>
		public static ILoggerFactory AddDatabaseLogger(this ILoggerFactory factory, IConfiguration loggingConfiguration, string connectionString)
		{
			return factory.AddDatabaseLogger(new DatabaseLoggerSettings(loggingConfiguration), connectionString);
		}
	}
}
