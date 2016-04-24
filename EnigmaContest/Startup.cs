using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EnigmaContest.Startup))]
namespace EnigmaContest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
