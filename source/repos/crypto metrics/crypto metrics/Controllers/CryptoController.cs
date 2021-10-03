using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.ComponentModel;
using Newtonsoft.Json;
using System.IO;
using System.Collections;

namespace crypto_metrics.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class CryptoController : Controller
    {
        private static readonly string[] CryptoTypeNames = new[]
        {
            "Bitcoin", "Bitcoin Cash", "Binance Coin", "Litecoin", "Bitcoin SV", "Ethereum", "Ripple"
        };

        //[HttpGet]
        //public JsonResult GetData(int Id)                                                                                     
        //{                                                                                                                     
        //    var rootObject = new CsvToJson("C:\testFile.csv");                                                                    Old methond i tried to use Json to covnert the .CSV file directly
        //    var MYObj = System.IO.File.ReadAllText(Startup.CSVFolderName + "/bch.csv").Select((x) => x.Split(',') ;               But had little sucess.
        //    var customerFromJson = JsonConvert.SerializeObject(MYObj, Formatting.Indented);                                       So i decided to just write my own converion method (ConvertCsvToJson)...
        //    return Json(customerFromJson);                                                                                    
        //                                                                                                                      
        //}
        [HttpGet]
        public IEnumerable Get(string CryptoToShow= "ALL/bch/bnb/bsv/btc/eth/ltc/xrp")
        {
            CallBacks.NewListT.Clear();
            

            if (CryptoToShow.Contains("ALL",StringComparison.OrdinalIgnoreCase) == true)
            {
                for (int i = 0; i < Startup.CryptoTypeStr.Length; i++)
                {
                    CallBacks.ConvertCsvToJson(Startup.CSVFolderName + "/" + Startup.CryptoTypeStr[i] + ".csv");
                    yield return Json(CallBacks.NewListT);
                }
                yield break;
            }
            else
            {
                CallBacks.ConvertCsvToJson(Startup.CSVFolderName + "/" + CryptoToShow +".csv");
            }
            yield return Json(CallBacks.NewListT);
        }

        
         //return JsonConvert.SerializeObject(listObjResult);
    }
    
    public class CallBacks
    {
        //public static Dictionary<string, List<string>> NewListT = new Dictionary<string, List<string>>();         //for multiple Data entries
        public static Dictionary<string, string> NewListT = new Dictionary<string, string>();
        public static void ConvertCsvToJson(string filePath)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            NewListT.Clear();
            // get header values
            var headers = lines[0].Split(',', StringSplitOptions.None);
            //var NaneConv = new List<string> { GetFullMetricName(Path.GetFileName(filePath))};                     //for multiple Data entries
            NewListT.Add("Name", GetFullMetricName(Path.GetFileName(filePath)));
            foreach (string line in lines.Skip(1))
            {
                var lineSplit = line.Split(',');// split line to get individual values
                for (int i = 0; i < headers.Length; i++)// loop to apply the headers
                {
                    if (!NewListT.ContainsKey(headers[i]))  //make sure value is not there allready (if processing whole CSV file,without "ConvertCSVFile")
                    {
                        //var myList = new List<string> { lineSplit[i] };                                           //for multiple Data entries
                        NewListT.Add(headers[i], lineSplit[i]);
                    }
                    //else NewListT[headers[i]].Add(lineSplit[i]);                                                  //for multiple Data entries
                }
                //NewListT.Add(dictionary);
            }
        }
        public static string GetFullMetricName(string Metric)
        {
            switch (Metric)
            {
                case "btc.csv": return "Bitcoin";
                case "bch.csv": return "Bitcoin Cash";
                case "bnb.csv": return "Binance Coin";
                case "ltc.csv": return "Litecoin";
                case "bsv.csv": return "Bitcoin SV";
                case "eth.csv": return "Ethereum";
                case "xrp.csv": return "Ripple";
            }
            return Metric;//if not found just return the same
        }
        
    }

    public class CryptoJson//was going to use it for the Json Conversion
    {
        public int PriceBTC { get; set; }
        public int PriceUSD { get; set; }
        public int NDF { get; set; } //Network distribution factor

    }
}
