using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MtgLibrarian.Startup))]
namespace MtgLibrarian
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
