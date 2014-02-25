using System.Collections.Generic;
using Demo.Bugs.Web;
using Microsoft.Owin;
using Nancy;

[assembly: OwinStartup(typeof(Startup))]

namespace Demo.Bugs.Web
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