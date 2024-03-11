using DbModels;
using Microsoft.EntityFrameworkCore;
using static Configuration.AppConfig;

namespace DbContext
{
    public class TicketTemplateDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<TicketTemplateDbM> TicketTemplate { get; set; }
        public DbSet<TicketsDataView> Vy_ShowTickets { get; set; }

        public TicketTemplateDbContext()
        { }

        public TicketTemplateDbContext(DbContextOptions<TicketTemplateDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketTemplateDbM>()
                .Property(b => b.TicketsHandlingJson)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<TicketsDataView>()
                .ToView("Vy_ShowTickets", "dbo")
                .HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        public static TicketTemplateDbContext Create(string dbConnectionString)
        {
            var optionsBuilder = DbContextOptions(DbLoginDetails(dbConnectionString));
            return new TicketTemplateDbContext(optionsBuilder.Options);
        }

        public static DbContextOptionsBuilder<TicketTemplateDbContext> DbContextOptions(DbLoginDetail dbLoginDetail)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TicketTemplateDbContext>();
            optionsBuilder.UseSqlServer(dbLoginDetail.DbConnectionString,
                        options => options.EnableRetryOnFailure());
            return optionsBuilder;
        }

        public class SqlServerDbContext : TicketTemplateDbContext
        {
            public SqlServerDbContext()
            { }

            public SqlServerDbContext(DbContextOptions<TicketTemplateDbContext> options) : base(options)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    var connectionString = ConfigurationRoot["DbLogins"];
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("The database connection string is not configured.");
                    }

                    var dbLoginDetail = DbLoginDetails(connectionString);
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