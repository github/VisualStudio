using GitHub.Models;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MetricsServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Run();
            Console.Read();
        }

        void Run()
        {
            var uri = new Uri("http://localhost:40000");
            var server = new MetricsServer.Server(uri.Host, uri.Port);

            server.Start();
        }

        async Task Send()
        {
            var uri = new Uri("http://localhost:40000");
            var server = new MetricsServer.Server(uri.Host, uri.Port);

            server.Start();

            var client = new HttpClient();
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "/api/usage/visualstudio"));

            var model = UsageModel.Create(Guid.NewGuid());
            model.Dimensions.AppVersion = "9.9.9";
            model.Dimensions.Lang = "en-us";
            model.Dimensions.VSVersion = "14";
            model.Measures.NumberOfStartups = 1;

            var data = new UsageData();
            data.Reports = new List<UsageModel> { model };

            request.Content = SerializeRequest(model);

            HttpResponseMessage response = null;
            try
            {
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
            var ret = await response.Content.ReadAsStringAsync();
            Console.WriteLine(response.ToString());
            Console.WriteLine(ret);

            server.Stop();
        }

        static StringContent SerializeRequest(UsageModel model)
        {
            var serializer = new SimpleJsonSerializer();
            var dictionary = ToModelDictionary(model);
            return new StringContent(serializer.Serialize(dictionary), Encoding.UTF8, "application/json");
        }

        static Dictionary<string, object> ToModelDictionary(object model)
        {
            var dict = new Dictionary<string, object>();
            var type = model.GetType();

            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
                {
                    dict.Add(ToJsonPropertyName(prop.Name), prop.GetValue(model));
                }
                else
                {
                    var value = prop.GetValue(model);

                    if (value == null)
                    {
                        dict.Add(ToJsonPropertyName(prop.Name), value);
                    }
                    else
                    {
                        dict.Add(ToJsonPropertyName(prop.Name), ToModelDictionary(value));
                    }
                }
            }

            return dict;
        }

        static string ToJsonPropertyName(string propertyName)
        {
            if (propertyName.Length < 2)
            {
                return propertyName.ToLowerInvariant();
            }

            return propertyName.Substring(0, 1).ToLowerInvariant() + propertyName.Substring(1);
        }
    }
}
