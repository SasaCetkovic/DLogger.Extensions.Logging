using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DLogger.Extensions.Logging.Sqlite
{
	public class LoggingContext : DbContext
	{
		public DbSet<Log> Logs { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=./Logging.sqlite");
		}
	}
}
