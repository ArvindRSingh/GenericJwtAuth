﻿using AzureTableIdentity;
using AzureTableIdentity.Core;
using AzureTableIdentity.Web.StartupServices;
using GenericJwtAuth.Providers;
using GenericJwtAuth.StartupServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nivra.Localization;

namespace GenericJwtAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConstantsInitializer.Initialize(Configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityCore<AzureTableUser>(options =>
                options.SignIn.RequireConfirmedAccount = true
                )

                //.AddTokenProvider<EmailConfirmationTokenProvider<AzureTableUser>>(TokenOptions.DefaultProvider)
                .AddTokenProvider<PasswordResetTokenProvider<AzureTableUser>>(TokenOptions.DefaultProvider)
                //.AddTokenProvider<ChangePhoneNumberTokenProvider<AzureTableUser>>(TokenOptions.DefaultProvider)
                //.AddTokenProvider<EmailTokenProvider<AzureTableUser>>("ChangePhoneNumber")
                .AddTokenProvider<UserTwoFactorTokenProvider>(TokenOptions.DefaultProvider)
                //.AddTokenProvider<DataProtectorTokenProvider<AzureTableUser>>(TokenOptions.DefaultProvider)

                .AddTokenProvider<AppEmailConfirmationTokenProvider<AzureTableUser>>("EmailConfirmationTokenProvider")
                //.AddTokenProvider<PasswordResetTokenProvider<AzureTableUser>>("PasswordReset")
                //.AddTokenProvider<ChangePhoneNumberTokenProvider<AzureTableUser>>("ChangePhoneNumber")
                ////.AddTokenProvider<EmailTokenProvider<AzureTableUser>>("ChangePhoneNumber")
                //.AddTokenProvider<UserTwoFactorTokenProvider>("TwoFactorTokenProvider")
                // .AddTokenProvider<DataProtectorTokenProvider<AzureTableUser>>(TokenOptions.DefaultProvider)
                .AddDefaultTokenProviders()
                ;


            AzureTableRepo azureTableRepo = new AzureTableRepo();
            Nivra.AzureOperations.Utility utility = new Nivra.AzureOperations.Utility(Configuration["ConnectionStrings:DefaultConnection"], "Auth");

            //Configuring CORS
            services.AddCors(config =>
            {
                config.AddPolicy("AllowAll", builder =>
                {
                    builder.WithOrigins(Configuration["AllowedHosts"])
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });


            //         // Configuring PasswordHasher
            //services.Configure<PasswordHasherOptions>(options =>
            //{

            //	options.HashAlgorithm = PasswordHasherAlgorithms.SHA1;
            //	options.SaltSize = 16;
            //	options.IterationCount = 8192;
            //});

            //// Registering PasswordHasher
            //services.AddPasswordHasher();

            //services.AddIdentity<AzureTableUser, AzureTableRole>();
            JwtTokenConfigurations.Load(Configuration);

            services.AddGenericJwtAuthService();

            var tables = AzureTables.InitializeAzureTables(Configuration["ConnectionStrings:DefaultConnection"], "Auth");
            foreach (var table in tables)
            {
                azureTableRepo.Collection.Add(table.Name, table);
            }

            services.AddSingleton<IAzureTableRepo>(azureTableRepo);
            services.AddSingleton<Nivra.AzureOperations.Utility>(utility);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                 );
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddTransient(typeof(IUserTwoFactorTokenProvider<AzureTableUser>), typeof(UserTwoFactorTokenProvider));
            services.AddSingleton(typeof(TextResourceManager));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });


        }
    }
}

