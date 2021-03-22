using System.IO;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.EntityFramework.Mappers;

namespace BankOfDotNet.IdentitySvr
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opts => opts.EnableEndpointRouting = false );

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", false)
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");
            var migrationAssembly = typeof(Startup).GetType().Assembly.GetName().Name;

            services.AddIdentityServer()
                //.AddInMemoryClients(Config.GetClients())
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryApiResources(Config.GetAllApiResources())
                //.AddInMemoryApiScopes(Config.GetApiScopes())
                .AddTestUsers(Config.GetUsers())
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(opts =>
                {
                    opts.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("BankOfDotNet.IdentitySvr"));
                })
                //Operationl Store: Token, Consents, codes, etc
                .AddOperationalStore(opts =>
                {
                    opts.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("BankOfDotNet.IdentitySvr"));
                });

            //Cookie policy stuff
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = (SameSiteMode)(-1);
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitialIndentityServerDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
            

            app.UseIdentityServer();
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private void InitialIndentityServerDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                //Seed the data
                //Seed Clients
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }
                //Seed the identity resources
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                //Seed the api reosurces
                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetAllApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        //Se activa la politica de Cookies a (1) pues en el caso de Chrome no redirije
        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = (SameSiteMode)(1);
                }
            }
        }

        private bool DisallowsSameSiteNone(string userAgent)
        {
            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
            userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
    }
}
