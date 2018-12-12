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
            LocalRepositoryModel repository,
            IModelServiceFactory factory)
        {
            var connection = await cm.GetConnection(repository);
            return connection?.IsLoggedIn == true ? await factory.CreateAsync(connection) : null;
        }
    }
}
