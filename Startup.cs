using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SB_Blog.Startup))]
namespace SB_Blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
