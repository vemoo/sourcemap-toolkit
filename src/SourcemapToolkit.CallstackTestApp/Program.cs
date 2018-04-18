using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace SourcemapToolkit.CallstackTestApp
{
    class Program
    {
        private static string GetContentRoot()
        {
            return System.Text.RegularExpressions.Regex.Replace(
                System.Reflection.Assembly.GetExecutingAssembly().Location,
                @"(?<=sourcemap-toolkit/src/SourcemapToolkit\.CallstackTestApp).*",
                ""
            );
        }

        private static IWebHost BuildWebHost()
        {
            var webHost = WebHost.CreateDefaultBuilder()
                .Configure(app =>
                {
                    app.UseStaticFiles();
                    app.UseMvc(router =>
                    {
                        router.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddLogging(l =>
                    {
                        // l.AddFilter(x => true);
                    });
                    services.AddMvc();
                })
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5000);
                })
                .UseContentRoot(GetContentRoot())
                .Build();
            return webHost;
        }

        public static void Main()
        {

            using (var webHost = BuildWebHost())
            {
                // var p = System.Reflection.Assembly.GetEntryAssembly().Location;
                // Console.WriteLine(p);

                var h = webHost.Services.GetService<IHostingEnvironment>();
                Console.WriteLine(h.ContentRootPath);

                webHost.Start();
                Console.WriteLine("<ENTER>");
                Console.ReadLine();
            }
        }
    }
}