using System;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static async Task<IModelService> GetModelService(
            this IConnectionManager cm,
            ILocalRepositoryModel repository,
            IModelServiceFactory factory)
        {
            var connection = await cm.GetConnection(repository);
            return connection != null ? await factory.CreateAsync(connection) : null;
        }
    }
}
