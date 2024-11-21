using System;
using Microsoft.EntityFrameworkCore;

namespace TagGame.Api.Persistence;

public interface IDatabase
{
    DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync();
}
