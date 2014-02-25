using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(Demo_bugs_Console.Startup))]

namespace Demo_bugs_Console
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                FileSystem = new PhysicalFileSystem("Scripts"),
                RequestPath = new PathString("/Scripts")
            });

            app.MapSignalR();

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("bugs", "api/{Controller}");
            app.UseWebApi(config);

            //note: NancyFx is a greedy handler - therefore need to either put it last or map a different pipeline for it
            app.UseNancy();
        }
    }
}
