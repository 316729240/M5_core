
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using MWMS;
using System;
namespace M5
{

    namespace Hosting
    {
        public static class HostingEnvironment
        {
            public static bool m_IsHosted;

            static HostingEnvironment()
            {
                m_IsHosted = false;
            }

            public static bool IsHosted
            {
                get
                {
                    return m_IsHosted;
                }
            }
        }
    }


    public static class PageContext
    {
        public static IHttpContextAccessor _contextAccessor;

        public static IServiceProvider ServiceProvider;

        static PageContext()
        { }


        public static Microsoft.AspNetCore.Http.HttpContext Current
        {
            get
            {
                Microsoft.AspNetCore.Http.HttpContext context = _contextAccessor.HttpContext;
                // context.Response.WriteAsync("Test");

                return context;
            }
        }


    } // End Class HttpContext 
}