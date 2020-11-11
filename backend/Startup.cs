using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBusMessaging;

namespace backend
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
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumer>();
            services.AddTransient<IProcessData, ProcessData>();
            services.AddSingleton<IDataService, DataService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var bus = app.ApplicationServices.GetService<IServiceBusConsumer>();
            bus.RegisterOnMessageHandlerAndReceiveMessages();
        }
    }

    public class ProcessData : IProcessData
    {
        private readonly IDataService _dataService;
        public ProcessData(IDataService dataService) {
            _dataService = dataService;
        }

        public void Process(MyPayload myPayload)
        {
            Console.WriteLine("Received message: " + myPayload.action);
            _dataService.Add(myPayload.action);
        }
    }
}
