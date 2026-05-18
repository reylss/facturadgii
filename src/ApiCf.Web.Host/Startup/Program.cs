using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ApiCf.Entidades.ComprobanteFiscalSecNs;
using System.IO;

namespace ApiCf.Web.Host.Startup
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

            
        }

        public static IWebHost BuildWebHost(string[] args)
        { 

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}




