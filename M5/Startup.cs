using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using M5.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace M5.Main
{
    public class Startup
    {
        static WebService service = new WebService();
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            var mvcBuilders =  services.AddMvc();
             mvcBuilders.ConfigureApplicationPartManager(apm =>
            {
             //   apm.ApplicationParts.Add(new AssemblyPart(Assembly.LoadFile(@"F:\web\my\M5_core\WebApplication1\bin\Debug\netcoreapp2.0\WebApplication1.dll")));
                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            PageContext._contextAccessor=app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/plugin")),
                RequestPath = "/manage/app"
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "default2",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });



            MWMS_Init();
            app.Run(async (context) =>
            {
                // WebService.HttpContext = context;
                //await 
                service.BeginRequest(context);
            });
        }
        public static   void MWMS_Init()
        {
            Sql.connectionString = @"server="+ ConfigurationManager.AppSettings["ServerIP"]
                + ";uid=" + ConfigurationManager.AppSettings["Username"] 
                + ";pwd=" + ConfigurationManager.AppSettings["Password"]
                + ";database=" + ConfigurationManager.AppSettings["DataBaseName"] + ";";
            /*
            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath = Tools.MapPath(@"/cache/"),// Tools.MapPath("assembly/"),
                Namespaces = new HashSet<string>
                             {
                                 "System",
                                 "MWMS",
                                 "MWMS.Helper",
                                 "System.Collections.Generic",
                                 "System.Linq"
                             }
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            RazorEngine.Razor.Compile("kkkkkak@{Raw(\"1111\");}kkkak", typeof(object[]), "test1", true);
            string html = RazorEngine.Razor.Run("test1", new object[] { "","" });
            context.Response.WriteAsync( html);*/
            /*
            UrlMapping u1 = new UrlMapping();
            u1.Path = "2222";
            Uri url = new Uri(context.Request.Scheme+"://"+context.Request.Host+context.Request.Path);
            WebUri u = new WebUri()
            {
                MainMobileUrl="",
            };
            u.AddMapping(new UrlMapping()
            {
                Path = "/acf/",
                PcDomain = "/pc2/",
                MobileDomain = "/m/"
            });
            u.AddMapping(new UrlMapping() {
                    Path="/",
                    PcDomain="/",
                    MobileDomain="/m/"
            });
            u.AddMapping(new UrlMapping()
            {
                Path = "mwms",
                PcDomain = "www.mwms4.com",
                MobileDomain = "m.mwms4.com"
            });
            RequestUrl requestUrl =u.Build(url, context.Request.Headers["User-Agent"]);
            if(requestUrl.IsMobileBrowser && !requestUrl.IsMobileUrl)
            {
                //跳转至手机地址
            } */
            // context.Response.WriteAsync("");
            //  context.Response.Clear
            // SystemData.HttpContext.Response.WriteAsync

        }
    }

}
