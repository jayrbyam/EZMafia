using System;
using System.Web;
using System.Web.Http;

namespace EZMafia
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("Default", "api/{controller}/{action}", new { action = "DefaultAction" });
        }
    }
}
