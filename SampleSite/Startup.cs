using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuScien.Security;
using Trivial.Security;

namespace NuScien.Web;

/// <summary>
/// The startup.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the Startup class.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        // Setup database and resource access client.
        ResourceAccessClients.Setup(() =>
        {
            return new AccountDbSetProvider(isReadonly => new AccountDbContext(UseSqlServer, "Server=.;Database=NuScien5;Integrated Security=True;"));
        });
    }

    /// <summary>
    /// Gets the configuration instance.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the service.
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    /// <summary>
    /// Configures.
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The web host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseStaticFiles();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static DbContextOptionsBuilder UseSqlServer(DbContextOptionsBuilder builder, string conn) => SqlServerDbContextOptionsExtensions.UseSqlServer(builder, conn);
}
