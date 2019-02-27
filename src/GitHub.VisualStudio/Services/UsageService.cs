using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public sealed class UsageService : IUsageService, IDisposable
    {
        const string StoreFileName = "metrics.json";
        const string UserStoreFileName = "user.json";
        static readonly ILogger log = LogManager.ForContext<UsageService>();

        readonly IGitHubServiceProvider serviceProvider;
        readonly IEnvironment environment;
        readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        string storePath;
        string userStorePath;
        Guid? userGuid;

        [ImportingConstructor]
        public UsageService(IGitHubServiceProvider serviceProvider, IEnvironment environment)
        {
            this.serviceProvider = serviceProvider;
            this.environment = environment;
        }

        public void Dispose()
        {
            semaphoreSlim.Dispose();
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
                    log.Error(ex, "Failed writing user metrics GUID");
                }
            }

            return userGuid.Value;
        }

        public IDisposable StartTimer(Func<Task> callback, TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                async _ =>
                {
                    try { await callback(); }
                    catch (Exception ex) { log.Warning(ex, "Failed submitting usage data"); }
                },
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                log.Error(ex, "Failed to write usage data");
            }
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
            // Avoid IOException when metrics updated multiple times in quick succession
            await semaphoreSlim.WaitAsync();
            try
            {
                using (var s = File.OpenRead(path))
                using (var r = new StreamReader(s, Encoding.UTF8))
                {
                    return await r.ReadToEndAsync();
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        async Task WriteAllTextAsync(string path, string text)
        {
            // Avoid IOException when metrics updated multiple times in quick succession
            await semaphoreSlim.WaitAsync();
            try
            {
                using (var s = new FileStream(path, FileMode.Create))
                using (var w = new StreamWriter(s, Encoding.UTF8))
                {
                    await w.WriteAsync(text);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        class UserData
        {
            public Guid UserGuid { get; set; }
        }
    }
}
