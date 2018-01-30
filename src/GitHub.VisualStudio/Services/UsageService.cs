using System;
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
        const string StoreFileName = "ghfvs.usage";
        const string UserStoreFileName = "user.json";
        private static readonly ILogger log = LogManager.ForContext<UsageService>();
        static readonly Calendar cal = CultureInfo.InvariantCulture.Calendar;
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

        public bool IsSameDay(DateTimeOffset lastUpdated)
        {
            return lastUpdated.Date == DateTimeOffset.Now.Date;
        }

        public bool IsSameWeek(DateTimeOffset lastUpdated)
        {
            return GetIso8601WeekOfYear(lastUpdated) == GetIso8601WeekOfYear(DateTimeOffset.Now);
        }

        public bool IsSameMonth(DateTimeOffset lastUpdated)
        {
            return lastUpdated.Month == DateTimeOffset.Now.Month;
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
                    new UsageData { Model = new UsageModel() };
            }
            catch
            {
                return new UsageData { Model = new UsageModel() };
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
                log.Warning(ex, "Failed to write usage data");
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

        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        static int GetIso8601WeekOfYear(DateTimeOffset time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time.UtcDateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time.UtcDateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        class UserData
        {
            public Guid UserGuid { get; set; }
        }
    }
}
