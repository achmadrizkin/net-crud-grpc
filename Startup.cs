using Common.Configs;
using Common.Middleware;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using net_test_generator_svc.Config;
using net_test_generator_svc.Repositories;
using net_test_generator_svc.UseCases;
using net_test_generator_svc.Protos;
using net_test_generator_svc.Repositories.MySql;
using net_test_generator_svc.Services;
using net_test_generator_svc.Repositories.Cache;
using FluentValidation;
using net_test_generator_svc.Models;
using net_test_generator_svc.Validators;
using Common.Email;
using Common.Interfaces;
using Common.Extensions;

namespace net_test_generator_svc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Setting for dapper
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();

            #region Register client api rest / grpc

            var policyConfigs = new HttpClientPolicyConfiguration();
            Configuration.Bind("HttpClientPolicies", policyConfigs);

            //services.AddRestClient<IClassRes, ClassRes>(Configuration["RestSettings:ProductUrl"], policyConfigs);
            //services.AddGrpcClient<ProductProtoService.ProductProtoServiceClient>(Configuration["GrpcSettings:ProductUrl"]);

            #endregion

            #region Redis Configuration
            services.AddStackExchangeRedisCache(options =>
            {
                // Retrieve Redis settings from the configuration
                var connectionString = Configuration.GetValue<string>("CacheSettings:ConnectionString");
                var redisPassword = Configuration.GetValue<string>("CacheSettings:Password");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException("CacheSettings:ConnectionString", "Redis connection string cannot be null or empty.");
                }

                // Build Redis configuration options
                var redisOptions = new StackExchange.Redis.ConfigurationOptions
                {
                    EndPoints = { connectionString }, // Use the specified connection string
                    Password = redisPassword,        // Use the specified password
                    Ssl = false                      // SSL disabled for local development
                };

                // Assign the constructed configuration options
                options.ConfigurationOptions = redisOptions;
            });
            #endregion


            #region IOC Register
            services.AddScoped<IDbConnectionFactory>(_ => new Config.MySql.DbConnectionFactory(Configuration.GetValue<string>("DatabaseSettings:ConnectionString")));

            //ResClient dan GRPC Client IOC tidak perlu ditambahkan

            //services.AddSingleton<IDriverPubSub, UpdateDriverPubSub>();
            services.AddScoped<ISiswaDb, SiswaDb>();
            services.AddScoped<ISiswaCache, SiswaCache>();
            services.AddScoped<ISiswaRepository, SiswaRepository>();
            services.AddScoped<ISiswaUseCase, SiswaUseCase>();
            services.AddScoped<IValidator<Siswa>, SiswaValidator>();



            #endregion

            services.AddAutoMapper(typeof(Startup));

            services.AddGrpc().AddJsonTranscoding();

            services.AddGrpcReflection();

            services.AddControllers();
            services.AddGrpcSwagger();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "net_test_generator_svc service", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.OperationFilter<HeaderFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Input bearer token here",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAllElasticApm(Configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "net_test_generator_svc service v1"));
            app.UseRouting();

            app.UseAuthorization();
            app.UseCustomResponseMiddleware();
            app.UseSSEMiddleware();

            app.UseEndpoints(endpoints =>
            {
                #region Service Register
                endpoints.MapGrpcService<SiswaService>();

                //endpoints.MapGrpcService<OrganizationService>();

                #endregion

                endpoints.MapGrpcReflectionService(); //  Focus!!!
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });

                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}