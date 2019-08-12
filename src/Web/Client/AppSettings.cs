using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Client
{
    public class AppSettings
    {
        public string CatalogUrl { get; set; }

        public string CartUrl { get; set; }

        public Logging Logging { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}