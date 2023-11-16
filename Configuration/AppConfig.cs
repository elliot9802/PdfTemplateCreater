using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Configuration
{
    public class AppConfig
    {
#if DEBUG
        public const string Appsettingfile = "appsettings.Development.json";
#else
        public const string Appsettingfile = "appsettings.json";
#endif

        #region Singleton design pattern
        private static readonly object instanceLock = new();

        private static AppConfig _instance = null;
        private static IConfigurationRoot _configuration = null;
        #endregion

        //All the DB Connections in the appsetting file
        private static DbLoginDetail _dbLogin;
        public static ImagePaths ImgPaths { get; private set; }

        private AppConfig()
        {
            //Lets get the credentials access Azure KV and set them as Environment variables
            //During Development this will come from User Secrets,
            //After Deployment it will come from appsettings.json

            string s = Directory.GetCurrentDirectory();

            //Create final ConfigurationRoot which includes also AzureKeyVault
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile(Appsettingfile, optional: true, reloadOnChange: true)
                                .AddUserSecrets("78db810f-e26c-48b1-828b-b474ce98e9f6", reloadOnChange: true);

            _configuration = builder.Build();

            //get DbSet details
            
            ImgPaths = new ImagePaths();
            _configuration.GetSection("ImagePaths").Bind(ImgPaths);

            var syncfusionLicenseKey = _configuration["SyncfusionLicenseKey"];
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);


        }

        public static IConfigurationRoot ConfigurationRoot
        {
            get
            {
                lock (instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppConfig();
                    }
                    return _configuration;
                }
            }
        }

        public static DbLoginDetail GetDbLoginDetails(string dbLoginKey)
        {
            if (string.IsNullOrEmpty(dbLoginKey))
                throw new ArgumentNullException(nameof(dbLoginKey), "Database login key is not provided.");

            lock (instanceLock)
            {
                // If _dbLogin has not been initialized, attempt to initialize it with the connection string
                // associated with the dbLoginKey in the configuration (user secrets or appsettings.json).
                if (_dbLogin == null)
                {
                    // Retrieve the connection string using the dbLoginKey.
                    var connectionString = ConfigurationRoot["DbLogins"]; // This should match your user secrets key for the connection string
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException($"Connection string for key '{dbLoginKey}' is missing in the configuration.");
                    }

                    // Initialize the _dbLogin with the retrieved connection string.
                    _dbLogin = new DbLoginDetail { DbConnection = connectionString };
                }

                // Return the initialized _dbLogin instance.
                return _dbLogin;
            }
        }

        public static ImagePaths GetImagePaths()
        {
            return ImgPaths;
        }

        public class DbLoginDetail
        {
            //set after reading in the active DbSet

            public string DbConnection { get; set; }
            public string DbConnectionString => DbConnection;
        }

        public class ImagePaths
        {
            public string BackgroundImagePath { get; set; }
            public string ScissorsLineImagePath { get; set; }
            public string AdImagePath { get; set; }
        }
    }
}