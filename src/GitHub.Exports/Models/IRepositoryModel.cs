using System;

namespace GitHub.Models
{
    public interface IRepositoryModel : ISimpleRepositoryModel
    {
        IAccount Owner { get; }
    }
}
