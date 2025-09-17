using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.DataAccess.Wizard.Native;
using DevExpress.Utils;
using DevExpress.XtraReports.Security;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vanigam.CRM.Reports
{
       internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var host = CreateHostBuilder().Build();
            ScriptPermissionManager.GlobalInstance = new ScriptPermissionManager(ExecutionMode.Unrestricted);
            DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(typeof(DbContext).Assembly);
            Application.Run(host.Services.GetRequiredService<frmReports>());
        }
        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddDbContext<VanigamAccountingDbContext>(options =>
                    {
                        options.UseNpgsql(context.Configuration.GetConnectionString("EntitiesConnection"));
                    });
                    services.AddScoped<ICurrentUserService, TestCurrentUserService>();
                    services.AddTransient<frmReports>();
                    services.AddTransient<frmReportDesigner>();
                    services.AddSingleton<IConfiguration>(provider => context.Configuration);
                });
        }

    }
}

