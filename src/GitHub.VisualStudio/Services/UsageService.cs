using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [Export(typeof(IUsageService))]
    public class UsageService : IUsageService
    {
        const string StoreFileName = "metrics.json";
        const string UserStoreFileName = "user.json";
        private static readonly ILogger log = LogManager.ForContext<UsageService>();
        readonly IGitHubServiceProvider serviceProvider;
        string storePath;
        string userStorePath;
        Guid? userGuid;

        [ImportingConstructor]
        public UsageService(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Guid> GetUserGuid()
        {
            await Initialize();

            if (!userGuid.HasValue)
            {
                try
                {
                    if (File.Exists(userStorePath))
                    {
                        var json = await ReadAllTextAsync(userStorePath);
                        var data = SimpleJson.DeserializeObject<UserData>(json);
                        userGuid = data.UserGuid;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed reading user metrics GUID");
                }
            }

            if (!userGuid.HasValue || userGuid.Value == Guid.Empty)
            {
                userGuid = Guid.NewGuid();

                try
                {
                    var data = new UserData { UserGuid = userGuid.Value };
                    var json = SimpleJson.SerializeObject(data);
                    await WriteAllTextAsync(userStorePath, json);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed reading writing metrics GUID");
                }
            }

            return userGuid.Value;
        }

        public bool IsToday(DateTimeOffset date)
        {
            return date.Date == DateTimeOffset.Now.Date;
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

        public async Task<UsageData> ReadLocalData()
        {
            await Initialize();

            var json = File.Exists(storePath) ? await ReadAllTextAsync(storePath) : null;

            try
            {
                return json != null ?
                    SimpleJson.DeserializeObject<UsageData>(json) :
                    new UsageData { Reports = new List<UsageModel>() };
            }
            catch(Exception ex)
            {
                log.Error(ex, "Error deserializing usage");
                return new UsageData { Reports = new List<UsageModel>() };
            }
        }

        public async Task WriteLocalData(UsageData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(storePath));
                var json = SimpleJson.SerializeObject(data);
                await WriteAllTextAsync(storePath, json);
            }
            catch(Exception ex)
            {
                log.Error(ex,"Failed to write usage data");
            }
        }

        async Task Initialize()
        {
            if (storePath == null)
            {
                await ThreadingHelper.SwitchToMainThreadAsync();

                var program = serviceProvider.GetService<IProgram>();
                storePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    program.ApplicationName,
                    StoreFileName);
                userStorePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    program.ApplicationName,
                    UserStoreFileName);
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

        class UserData
        {
            public Guid UserGuid { get; set; }
        }
    }
}
