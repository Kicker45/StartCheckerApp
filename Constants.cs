using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartCheckerApp
{
        public static class Constants
        {
            // URL of REST service
            //public static string RestUrl = "https://dotnetmauitodorest.azurewebsites.net/api/todoitems/{0}";

            // URL of REST service (Android does not use localhost)
            // Use http cleartext for local deployment. Change to https for production
            public static string LocalhostUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
            public static string Scheme = "http"; // or http
            public static string Port = "5034"; //5034 for http
            public static string RestUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/StartList/";
        }
    
}
