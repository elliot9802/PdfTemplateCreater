using Configuration;
using DbModels;
using Microsoft.EntityFrameworkCore;
using static Configuration.AppConfig;

namespace DbContext
{
    public class csMainDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<TicketTemplateDbM> TicketTemplate { get; set; }
        public DbSet<TicketsDataDto> Vy_ShowTickets { get; set; }

        public csMainDbContext() { }

        public csMainDbContext(DbContextOptions<csMainDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketTemplateDbM>()
                .Property(b => b.TicketsHandlingJson)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<TicketsDataDto>()
                .ToView("Vy_ShowTickets", "dbo")
                .HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        public static csMainDbContext Create(string dbConnectionString)
        {
            var optionsBuilder = DbContextOptions(GetDbLoginDetails(dbConnectionString));
            return new csMainDbContext(optionsBuilder.Options);
        }

        public static DbContextOptionsBuilder<csMainDbContext> DbContextOptions(DbLoginDetail dbLoginDetail)
        {
            var optionsBuilder = new DbContextOptionsBuilder<csMainDbContext>();
            optionsBuilder.UseSqlServer(dbLoginDetail.DbConnectionString,
                        options => options.EnableRetryOnFailure());
            return optionsBuilder;
        }

        public class SqlServerDbContext : csMainDbContext
        {
            public SqlServerDbContext() { }

            public SqlServerDbContext(DbContextOptions<csMainDbContext> options) : base(options) { }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    var connectionString = ConfigurationRoot["DbLogins"];
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("The database connection string is not configured.");
                    }

                    var dbLoginDetail = GetDbLoginDetails(connectionString);
                    optionsBuilder.UseSqlServer(dbLoginDetail.DbConnectionString,
                        options => options.EnableRetryOnFailure());
                }
                base.OnConfiguring(optionsBuilder);
            }

            protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            {
                configurationBuilder.Properties<decimal>().HaveColumnType("money");
                configurationBuilder.Properties<string>().HaveColumnType("nvarchar(200)");
                base.ConfigureConventions(configurationBuilder);
            }
        }
    }
}
