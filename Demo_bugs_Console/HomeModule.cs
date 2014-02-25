using System.Collections.Generic;
using Demo_bugs_Console.Web;
using Microsoft.Owin;
using Nancy;

[assembly: OwinStartup(typeof(Demo_bugs_Console.Startup))]

namespace Demo_bugs_Console.Web
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ =>
            {
                var model = new { title = "We've Got Issues..." };
                return View["home", model];
            };
        }
    }
}