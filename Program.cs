using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfOrder
{
	public class Program
	{
		private static ILoggerFactory LogFactory = LoggerFactory.Create(builder => builder.AddConsole());
		private static ILogger<Program> Logger = LogFactory.CreateLogger<Program>();

		private static DbContextOptionsBuilder<AppDbContext> dbOptions = new DbContextOptionsBuilder<AppDbContext>()
			.UseLoggerFactory(LogFactory)
			.UseSqlite("Data Source=app.db");

		public static void Main(string[] args)
		{
			CreateDb();
			RunQuery();
			RunSkipQuery();
		}

		public static void CreateDb()
		{
			using var dbContext = new AppDbContext(dbOptions.Options);
			dbContext.Database.EnsureCreated();
		}

		public static void RunQuery()
		{
			Logger.LogInformation("Running query without skip");

			using var dbContext = new AppDbContext(dbOptions.Options);
			var query = dbContext.Posts;
			var data = query
				.Select(x => x.ThreadId)
				.Distinct()
				.SelectMany(x => query
					.Where(y => y.ThreadId == x)
					.OrderBy(y => y.Message))
				.ToList();
		}

		public static void RunSkipQuery()
		{
			Logger.LogInformation("Running query with skip");

			using var dbContext = new AppDbContext(dbOptions.Options);
			var query = dbContext.Posts;
			var data = query
				.Select(x => x.ThreadId)
				.Distinct()
				.SelectMany(x => query
					.Where(y => y.ThreadId == x)
					.OrderBy(y => y.Message)
					.Skip(0))
				.ToList();
		}
	}

	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<Thread> Threads { get; set; }
		public DbSet<Post> Posts { get; set; }
	}

	public class Thread
	{
		public Guid Id { get; set; }
		public ICollection<Post> Posts { get; set; }
	}

	public class Post 
	{
		public Guid Id { get; set; }
		public string Message { get; set; }
		public Guid ThreadId { get; set; }
		public Thread Thread { get; set; }
	}
}
