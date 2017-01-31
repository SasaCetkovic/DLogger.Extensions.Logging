using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DLogger.Extensions.Logging
{
    public static class DatabaseLoggerFactoryExtensions
    {
		/// <summary>
		/// Adds custom database logging provider
		/// </summary>
		/// <param name="factory">This <see cref="ILoggerFactory"/> instance</param>
		/// <param name="settings">Logger settings as a <see cref="IDatabaseLoggerSettings"/> implementation</param>
		/// <param name="writer"><see cref="IDatabaseLogWriter"/> implementation</param>
		/// <returns></returns>
		public static ILoggerFactory AddDatabaseLogger(this ILoggerFactory factory, IDatabaseLoggerSettings settings, IDatabaseLogWriter writer)
		{
			factory.AddProvider(new DatabaseLoggerProvider(settings, writer));
			return factory;
		}

		/// <summary>
		/// Adds custom database logging provider
		/// </summary>
		/// <param name="factory">This <see cref="ILoggerFactory"/> instance</param>
		/// <param name="loggingConfiguration">Appropriate configuration section</param>
		/// <param name="writer"><see cref="IDatabaseLogWriter"/> implementation</param>
		/// <returns></returns>
		public static ILoggerFactory AddDatabaseLogger(this ILoggerFactory factory, IConfiguration loggingConfiguration, IDatabaseLogWriter writer)
		{
			return factory.AddDatabaseLogger(new DatabaseLoggerSettings(loggingConfiguration), writer);
		}
	}
}
