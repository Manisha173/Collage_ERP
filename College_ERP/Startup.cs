using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using College_ERP.Helpers;
using Microsoft.Owin;
using Owin;

   [assembly: OwinStartup(typeof(College_ERP.Startup))]
namespace College_ERP
{
        public class Startup
        {
        public void Configuration(IAppBuilder app)
        {
            JwtConfig.ConfigureJwt(app);
        }
    }
    
}