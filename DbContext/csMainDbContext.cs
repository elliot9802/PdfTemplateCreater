using DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DbContext
{
    public class csMainDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly IConfiguration? _configuration;

        private string? _dblogin;
        public DbSet<TicketsDataDto> Vy_ShowTickets { get; set; }

        #region Handle the context to use 
        public static DbContextOptionsBuilder<csMainDbContext> DbContextOptions(string dbConnectionString)
        {
            var _optionsBuilder = new DbContextOptionsBuilder<csMainDbContext>();

            _optionsBuilder.UseSqlServer(dbConnectionString,
                        options => options.EnableRetryOnFailure());
            return _optionsBuilder;

            //unknown database type
            throw new InvalidDataException($"Database type  does not exist");
        }
        #endregion

        #region constructors
        public csMainDbContext() { }
        public csMainDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _dblogin = _configuration["DbLogins"];
        }

        public csMainDbContext(DbContextOptions options) : base(options)
        { }
        #endregion

        //Here we can modify the migration building
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketsDataDto>().ToView("Vy_ShowTickets", "dbo").HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        #region DbContext for Sql Server database
        public static csMainDbContext DbContext(string dbConnectionString)
        {
            // Create the options builder with the connection string
            var optionsBuilder = DbContextOptions(dbConnectionString);

            // Use the 'Options' property of 'DbContextOptionsBuilder', not the string
            return new csMainDbContext(optionsBuilder.Options);
        }

        public class SqlServerDbContext : csMainDbContext
        {
            public SqlServerDbContext() { }
            public SqlServerDbContext(DbContextOptions options) : base(options)
            { }


            //Used only for CodeFirst Database Migration and database update commands
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    var connectionString = _configuration["DbLogins"];
                    optionsBuilder.UseSqlServer(connectionString,
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

            #region Add your own modelling based on done migrations
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

                base.OnModelCreating(modelBuilder);
            }
            #endregion

        }
        #endregion
    }
}
