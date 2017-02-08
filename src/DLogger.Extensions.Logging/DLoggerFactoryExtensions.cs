using DLogger.Extensions.Logging.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DLogger.Extensions.Logging
{
	/// <summary>
	/// Extension methods for initializing <see cref="DLogger"/>
	/// </summary>
	public static class DLoggerFactoryExtensions
    {
		/// <summary>
		/// Adds a custom logging provider
		/// </summary>
		/// <param name="factory">This <see cref="ILoggerFactory"/> instance</param>
		/// <param name="settings">Logger settings as a <see cref="ILoggerSettings"/> implementation</param>
		/// <param name="writer"><see cref="ILogWriter"/> implementation</param>
		/// <returns></returns>
		public static ILoggerFactory AddDLogger(this ILoggerFactory factory, ILoggerSettings settings, ILogWriter writer)
		{
			factory.AddProvider(new DLoggerProvider(settings, writer));
			return factory;
		}

		/// <summary>
		/// Adds a custom logging provider
		/// </summary>
		/// <param name="factory">This <see cref="ILoggerFactory"/> instance</param>
		/// <param name="loggingConfiguration">Appropriate configuration section</param>
		/// <param name="writer"><see cref="ILogWriter"/> implementation</param>
		/// <returns></returns>
		public static ILoggerFactory AddDLogger(this ILoggerFactory factory, IConfiguration loggingConfiguration, ILogWriter writer)
		{
			return factory.AddDLogger(new DLoggerSettings(loggingConfiguration), writer);
		}
	}
}
