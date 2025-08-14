using Hangfire;
using Hangfire.Oracle.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using OracleConn_BGJobs;
using Serilog;

var oracleConnString = "User Id=system;Password=Tonterias00;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=FREE)));";

var builder = Host.CreateDefaultBuilder(args)
   .UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext()
   )
   .ConfigureServices((hostContext, services) => {
       var oracleStorage = new OracleStorage(oracleConnString);

       services.AddSingleton<OracleDataService>(provider =>
           new OracleDataService(
               oracleConnString,
               provider.GetRequiredService<ILogger<OracleDataService>>()
           )
       );

       services.AddHangfire(config =>
           config.UseStorage(oracleStorage)
       );
       services.AddHangfireServer(config => {
           config.WorkerCount = Environment.ProcessorCount * 2; // Adjust worker count based on your needs
           config.ServerName = "vitadi_bgjobs"; // Optional: Set a custom server name
       });
       services.AddHangfireServer();
       services.AddHostedService<Worker>();
   })
   //.ConfigureLogging( logging => {  
   //    logging.ClearProviders();  
   //    logging.AddConsole();  
   //})  
   .ConfigureWebHostDefaults(webBuilder => {
       webBuilder.Configure(app => {
           app.UseRouting();
           app.UseHangfireDashboard("/hangfire");

       });
   });

try {
    Log.Information("Starting application...");
    builder.Build().Run();
} catch (Exception ex) {
    Log.Fatal(ex, "Application start-up failed");
} finally {
    Log.CloseAndFlush();
}
