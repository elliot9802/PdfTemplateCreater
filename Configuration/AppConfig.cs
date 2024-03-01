using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;

namespace Configuration
{
    public class AppConfig
    {
#if DEBUG
        private const string Appsettingfile = "appsettings.Development.json";
#else
        private const string Appsettingfile = "appsettings.json";
#endif

        private static readonly object InstanceLock = new();
        private static AppConfig? _instance;
        private readonly IConfigurationRoot _configuration;
        private readonly ImagePaths _imgPaths;

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

        public static AppConfig Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ??= new AppConfig();
                }
            }
        }

        public static IConfigurationRoot ConfigurationRoot => Instance._configuration;

        public static DbLoginDetail DbLoginDetails(string dbLoginKey)
        {
            var connectionString = ConfigurationRoot["DbLogins"]
                ?? throw new InvalidOperationException($"Connection string for key '{dbLoginKey}' is missing in the configuration.");

            return new DbLoginDetail { DbConnection = connectionString };
        }

        public static ImagePaths ImagePathSettings => Instance._imgPaths;

        public class DbLoginDetail
        {
            public string? DbConnection { get; set; }
            public string? DbConnectionString => DbConnection;
        }

        public class ImagePaths
        {
            public string? BackgroundImagePath { get; set; }
            public string? ScissorsLineImagePath { get; set; }
            public string? AdImagePath { get; set; }
        }
    }
}