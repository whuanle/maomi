using Microsoft.EntityFrameworkCore;

namespace Demo7.Tran
{
	public class MyContext : DbContext
	{
		public DbSet<AccountEntity> Account { get; set; }

		public MyContext(DbContextOptions<MyContext> dbContext) : base(dbContext)
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(
				@"filename=my.db");
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<AccountEntity>()
				.Property(b => b.Id)
				.ValueGeneratedOnAdd();
		}
	}

	[PrimaryKey(nameof(Id))]
	[Index(nameof(EMail), IsUnique = true)]
	public class AccountEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string EMail { get; set; }
		public bool VerifyEMail { get; set; }
	}
}
