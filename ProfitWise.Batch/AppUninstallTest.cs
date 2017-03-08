using System;
using System.IO;
using System.Net;

namespace ProfitWise.Batch
{
    public class AppUninstallTest
    {
        public static void Execute()
        {
            Console.WriteLine("Simulating App/Uninstallation Webhook...");

            var url = "https://gracie2/profitwise/shopifyauth/uninstall";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(
                "X-Shopify-Hmac-Sha256", "vFTuZXXdLflcXPO3o6mAf0naYyuhEe6YpJ6QEWno11U=");

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = System.IO.File.ReadAllText("UninstallRequest.txt");
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            Console.WriteLine("Complete... hit enter to continue");
            Console.ReadLine();
        }
    }
}
