using System.Web.Http;
using Demo.Bugs.Web;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly:OwinStartup(typeof(Startup))]

namespace Demo.Bugs.Web
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