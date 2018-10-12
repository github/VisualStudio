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
        readonly SQLiteAsyncConnection connection;

        [ImportingConstructor]
        public SqliteMessageDraftStore(IOperatingSystem os)
        {
            var path = Path.Combine(
                os.Environment.GetApplicationDataPath(),
                "drafts.db");
            connection = new SQLiteAsyncConnection(path);
        }

        public async Task<T> GetDraft<T>(string key, string secondaryKey) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotNull(secondaryKey, nameof(secondaryKey));

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

            return null;
        }

        public async Task<IEnumerable<(string secondaryKey, T data)>> GetDrafts<T>(string key) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));

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

            return null;
        }

        public async Task UpdateDraft<T>(string key, string secondaryKey, T data) where T : class
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotNull(secondaryKey, nameof(secondaryKey));

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

        [Table("Drafts")]
        private class Draft
        {
            public string Key { get; set; }
            public string SecondaryKey { get; set; }
            public string Data { get; set; }
        }
    }
}
