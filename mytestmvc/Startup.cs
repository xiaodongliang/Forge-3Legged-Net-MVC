using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(mytestmvc.Startup))]
namespace mytestmvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
