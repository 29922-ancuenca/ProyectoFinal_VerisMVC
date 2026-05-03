using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(_02_MVC.Startup))]
namespace _02_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
