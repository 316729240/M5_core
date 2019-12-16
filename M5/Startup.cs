using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using M5.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using MWMS.Helper;

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
        static   void MWMS_Init()
        {
            Sql.connectionString = @"server="+ ConfigurationManager.AppSettings["ServerIP"]
                + ";uid=" + ConfigurationManager.AppSettings["Username"] 
                + ";pwd=" + ConfigurationManager.AppSettings["Password"]
                + ";database=" + ConfigurationManager.AppSettings["DataBaseName"]
                + ";min pool size=10;max pool size=100;connect timeout = 20;pooling=true;";
            Razor.SetTemplateService(MWMS.Template.BuildCode.TemplateService);
            RazorEngine.Razor.Compile("1", typeof(object[]),"_init_temp_code", true);
          // LoadMetadataReference();
        }
        static void LoadMetadataReference()
        {
            Tools.MapPath("");
            Tools.GetId();
            string replstr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "file:///" : "file://";
            var assemblies = CompilerServicesUtility
                .GetLoadedAssemblies()
                .Where(a => !a.IsDynamic && File.Exists(a.CodeBase.Replace(replstr, "")))
                .Select(a => (a.CodeBase.Replace(replstr, "")));

            int c = assemblies.Count();

            Constant.BaseNamespaces = new MetadataReference[c];

            int i = 0;
            foreach (string item in assemblies)
            {
                Constant.BaseNamespaces[i] = (MetadataReference.CreateFromFile(item));
                i++;
            }
        }
    }

}
