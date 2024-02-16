using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;

namespace Configuration
{
    public class AppConfig
    {
#if DEBUG
        public const string Appsettingfile = "appsettings.Development.json";
#else
        public const string Appsettingfile = "appsettings.json";
#endif

        private static readonly object InstanceLock = new();
        private static AppConfig _instance;
        private static IConfigurationRoot _configuration;
        private static DbLoginDetail _dbLogin;
        public static ImagePaths _imgPaths;

        private AppConfig()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile(Appsettingfile, optional: true, reloadOnChange: true)
                                .AddUserSecrets("78db810f-e26c-48b1-828b-b474ce98e9f6", reloadOnChange: true);

            _configuration = builder.Build();

            _imgPaths = new ImagePaths();
            _configuration.GetSection("ImagePaths").Bind(_imgPaths);

            var syncfusionLicenseKey = _configuration["SyncfusionLicenseKey"];
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
        }

        public static IConfigurationRoot ConfigurationRoot
        {
            get
            {
                lock (InstanceLock)
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

            lock (InstanceLock)
            {
                if (_dbLogin == null)
                {
                    var connectionString = ConfigurationRoot["DbLogins"];
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException($"Connection string for key '{dbLoginKey}' is missing in the configuration.");
                    }

                    _dbLogin = new DbLoginDetail { DbConnection = connectionString };
                }

                return _dbLogin;
            }
        }

        public static ImagePaths GetImagePaths() => _imgPaths;

        public class DbLoginDetail
        {
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