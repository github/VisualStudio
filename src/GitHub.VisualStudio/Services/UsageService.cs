using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using Serilog;
using Rothko;
using Environment = System.Environment;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [Export(typeof(IUsageService))]
    public class UsageService : IUsageService
    {
        const string StoreFileName = "metrics.json";
        const string UserStoreFileName = "user.json";
        static readonly ILogger log = LogManager.ForContext<UsageService>();

        readonly IGitHubServiceProvider serviceProvider;
        readonly IEnvironment environment;

        string storePath;
        string userStorePath;
        UserData userData;

        [ImportingConstructor]
        public UsageService(IGitHubServiceProvider serviceProvider, IEnvironment environment)
        {
            this.serviceProvider = serviceProvider;
            this.environment = environment;
        }

        public async Task<UserData> GetUserData()
        {
            await Initialize();

            if (userData == null)
            {
                userData = await ReadUserData();
            }

            return userData;
        }

        public IDisposable StartTimer(Func<Task> callback, TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(
                async _ =>
                {
                    try { await callback(); }
                    catch (Exception ex) { log.Warning(ex, "Failed submitting usage data"); }
                },
                null,
                dueTime,
                period);
        }

        public async Task<UsageData> ReadUsageData()
        {
            await Initialize();

            var json = File.Exists(storePath) ? await ReadAllTextAsync(storePath) : null;

            try
            {
                return json?.FromJson<UsageData>() ?? new UsageData { Reports = new List<UsageModel>() };
            }
            catch(Exception ex)
            {
                log.Error(ex, "Error deserializing usage");
                return new UsageData { Reports = new List<UsageModel>() };
            }
        }

        public async Task WriteUsageData(UsageData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(storePath));
                var json = data.ToJson();
                await WriteAllTextAsync(storePath, json);
            }
            catch (Exception ex)
            {
                log.Error(ex,"Failed to write usage data");
            }
        }

        public async Task<UserData> ReadUserData()
        {
            await Initialize();

            var json = File.Exists(userStorePath) ? await ReadAllTextAsync(userStorePath) : null;

            UserData ret = null;
            if (json != null)
            {
                try
                {
                    ret = json?.FromJson<UserData>();
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error deserializing user data");
                }
            }

            if (ret == null)
            {
                ret = new UserData { UserGuid = Guid.NewGuid() };
                await WriteUserData(ret);
            }
            return ret;
        }

        public async Task<bool> WriteUserData(UserData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(userStorePath));
                var json = data.ToJson();
                await WriteAllTextAsync(storePath, json);
            }
            catch (Exception ex)
            {
                log.Error(ex,"Failed to write user data");
                return false;
            }
            return true;
        }

        async Task Initialize()
        {
            if (storePath == null)
            {
                await ThreadingHelper.SwitchToMainThreadAsync();

                var program = serviceProvider.GetService<IProgram>();

                var localApplicationDataPath = environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                storePath = Path.Combine(localApplicationDataPath, program.ApplicationName, StoreFileName);
                userStorePath = Path.Combine(localApplicationDataPath, program.ApplicationName, UserStoreFileName);
            }
        }

        async Task<string> ReadAllTextAsync(string path)
        {
            using (var s = File.OpenRead(path))
            using (var r = new StreamReader(s, Encoding.UTF8))
            {
                return await r.ReadToEndAsync();
            }
        }

        async Task WriteAllTextAsync(string path, string text)
        {
            using (var s = new FileStream(path, FileMode.Create))
            using (var w = new StreamWriter(s, Encoding.UTF8))
            {
                await w.WriteAsync(text);
            }
        }
    }
}
