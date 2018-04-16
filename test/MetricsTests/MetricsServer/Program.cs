using GitHub.Models;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Nancy.Extensions;

namespace MetricsServer
{
    public class Server
    {
        readonly string host;
        readonly int port;
        NancyHost server;
        public Server(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Start()
        {
            var conf = new HostConfiguration { RewriteLocalhost = false };
            server = new NancyHost(conf, new Uri($"http://{host}:{port}"));
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }
    }

    public class UsageModule : NancyModule
    {
        public UsageModule()
        {
            Post["/api/usage/visualstudio"] = p =>
            {
                Console.WriteLine(Request.Body.AsString());
                var errors = new List<string>();
                var usage = this.Bind<UsageModel>();
                if (String.IsNullOrEmpty(usage.AppVersion))
                    errors.Add("Empty appVersion");
                if (String.IsNullOrEmpty(usage.Lang))
                    errors.Add("Empty lang");
                if (String.IsNullOrEmpty(usage.VSVersion))
                    errors.Add("Empty vSVersion");
                if (usage.NumberOfStartups == 0)
                    errors.Add("Startups is 0");
                if (errors.Count > 0)
                {
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.BadRequest)
                        .WithAllowedMediaRange(MediaRange.FromString("application/json"))
                        .WithMediaRangeModel(
                              MediaRange.FromString("application/json"),
                              new { result = errors }); // Model for 'application/json';
                }

                return Negotiate
                    .WithAllowedMediaRange(MediaRange.FromString("application/json"))
                    .WithMediaRangeModel(
                            MediaRange.FromString("application/json"),
                            new { result = "Cool usage" }); // Model for 'application/json';
            };
        }
    }
}
