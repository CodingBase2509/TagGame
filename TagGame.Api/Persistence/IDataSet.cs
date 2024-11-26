using System;
using Microsoft.EntityFrameworkCore;

namespace TagGame.Api.Persistence;

public interface IDataSet
{
    DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync();
}
