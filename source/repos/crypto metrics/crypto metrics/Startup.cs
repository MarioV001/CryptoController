using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace crypto_metrics
{
    public class Startup
    {
        public static readonly string CSVFolderName = "CSVFiles";
        public static readonly string[] CryptoTypeStr = new[]
        {
            "btc", "bch", "bnb", "ltc", "bsv", "eth", "xrp"
        };
        private async Task DownloadCSV(string CryptoType)
        {
            using (WebClient wc = new WebClient())
            {
                //wc.DownloadFileCompleted += DownloadFileCompleted;
                await wc.DownloadFileTaskAsync(new System.Uri("https://coinmetrics.io/newdata/" + CryptoType + ".csv"), CSVFolderName + "/"+CryptoType + ".csv");//download the csv form coinmetrics.io
            }

        }
        //private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs args)//once complete
        //{
        //    string CSVHeader = System.IO.File.ReadLines(CryptoType + ".csv").First(); // gets the first line from file. so i can define the names from the csv
        //}
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //
            if (!System.IO.Directory.Exists(CSVFolderName)) System.IO.Directory.CreateDirectory(CSVFolderName);//if folder for stored CVS doesnt exist, creat it

            foreach (string CSV in CryptoTypeStr)//download all CSV files up to date
            {
                if (System.IO.File.Exists(CSVFolderName + "/" + CSV + ".csv"))//file Allready Exist
                {
                    if (System.IO.File.GetCreationTime(CSVFolderName + "/" + CSV + ".csv") > DateTime.Now.AddHours(-23)==false)//check if the files is older then 24hrs
                    {
                        System.IO.File.Delete(CSVFolderName + "/" + CSV + ".csv");//delete old file
                        DownloadCSV(CSV).GetAwaiter().GetResult();//download new file asynchronous
                        //ConvertCSVFile(CSV).GetAwaiter().GetResult();   //Edit the file to leave only neceseery info(will leave most reacent Data Entry)
                                                                        //Comment this line to process all Data!!!
                    }
                }
                else
                {
                    DownloadCSV(CSV).GetAwaiter().GetResult();
                    //ConvertCSVFile(CSV).GetAwaiter().GetResult();
                }
            }
        }
        private async Task ConvertCSVFile(string CryptoType)
        {
            string CSVHeader = System.IO.File.ReadLines(CSVFolderName + "/" + CryptoType + ".csv").First(); // gets the first line from file. so i can define the names from the csv
            string CSVLastEntry = System.IO.File.ReadLines(CSVFolderName + "/" + CryptoType + ".csv").Last();
            string NewCVSFile = CSVHeader + "\n" + CSVLastEntry;
            System.IO.File.Delete(CSVFolderName + "/" + CryptoType + ".csv");//delete old file(save space)
            await System.IO.File.WriteAllTextAsync(CSVFolderName + "/" + CryptoType + ".csv", NewCVSFile);//new file
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddMemoryCache();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "crypto_metrics", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "crypto_metrics v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
