using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using Newtonsoft.Json;
using Rothko;
using Serilog;
using SQLite;

namespace GitHub.Services
{
    /// <summary>
    /// Stores drafts of messages in an SQL database.
    /// </summary>
    [Export(typeof(IMessageDraftStore))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SqliteMessageDraftStore : IMessageDraftStore
    {
        static readonly ILogger log = LogManager.ForContext<SqliteMessageDraftStore>();
        readonly IOperatingSystem os;
        SQLiteAsyncConnection connection;
        bool initialized;

        [ImportingConstructor]
        public SqliteMessageDraftStore(IOperatingSystem os)
        {
            this.os = os;
        }

        public async Task<T> GetDraft<T>(string key, string secondaryKey) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotNull(secondaryKey, nameof(secondaryKey));

            if (await Initialize().ConfigureAwait(false))
            {
                try
                {
                    var result = await connection.Table<Draft>().Where(
                        x => x.Key == key && x.SecondaryKey == secondaryKey)
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

                    if (result != null)
                    {
                        return JsonConvert.DeserializeObject<T>(result.Data);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to load message draft into {Type}", typeof(T));
                }
            }

            return null;
        }

        public async Task<IEnumerable<(string secondaryKey, T data)>> GetDrafts<T>(string key) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));

            if (await Initialize().ConfigureAwait(false))
            {
                try
                {
                    var result = await connection.Table<Draft>().Where(x => x.Key == key)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    return result.Select(x => (x.SecondaryKey, JsonConvert.DeserializeObject<T>(x.Data)));
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to load message drafts into {Type}", typeof(T));
                }
            }

            return null;
        }

        public async Task UpdateDraft<T>(string key, string secondaryKey, T data) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotNull(secondaryKey, nameof(secondaryKey));

            if (!await Initialize().ConfigureAwait(false))
            {
                return;
            }

            try
            {
                var row = new Draft
                {
                    Key = key,
                    SecondaryKey = secondaryKey,
                    Data = JsonConvert.SerializeObject(data),
                };

                await connection.InsertOrReplaceAsync(row).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to update message draft");
            }
        }

        public async Task DeleteDraft(string key, string secondaryKey)
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotNull(secondaryKey, nameof(secondaryKey));

            if (!await Initialize().ConfigureAwait(false))
            {
                return;
            }

            try
            {
                await connection.ExecuteAsync(
                    "DELETE FROM Drafts WHERE Key=? AND SecondaryKey=?",
                    key,
                    secondaryKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to update message draft");
            }
        }

        async Task<bool> Initialize()
        {
            if (!initialized)
            {
                var path = Path.Combine(os.Environment.GetApplicationDataPath(), "drafts.db");

                try
                {
                    connection = new SQLiteAsyncConnection(path);

                    var draftsTable = await connection.GetTableInfoAsync("Drafts").ConfigureAwait(false);

                    if (draftsTable.Count == 0)
                    {
                        await connection.ExecuteAsync(@"
                            CREATE TABLE `Drafts` (
	                            `Key`	TEXT,
	                            `SecondaryKey`	TEXT,
	                            `Data`	TEXT,
	                            UNIQUE(`Key`,`SecondaryKey`)
                            );").ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error opening drafts from {Path}.", path);
                }
                finally
                {
                    initialized = true;
                }
            }

            return connection != null;
        }

        [Table("Drafts")]
        private class Draft
        {
            public string Key { get; set; }
            public string SecondaryKey { get; set; }
            public string Data { get; set; }
        }
    }
}
