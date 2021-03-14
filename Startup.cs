using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using quizter_be.Hubs;
using quizter_be.Services;
using quizter_be.Repository;

namespace quizter_be
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                        builder.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                        builder.WithOrigins("https://quizterr.herokuapp.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                        builder.WithOrigins("http://quizterr.herokuapp.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
            });
            // services.AddScoped<IGameStorage>(storage => new FileGameStorage(@"/Repo/Games/"));
            services.AddScoped<IGameStorage>(storage => new DBGameStorage(Configuration));
            services.AddScoped<IQuestionStorage>(storage => new FileQuestionStorage(@"Questions/", @"/Repo/Games/"));
           
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddControllers();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}
