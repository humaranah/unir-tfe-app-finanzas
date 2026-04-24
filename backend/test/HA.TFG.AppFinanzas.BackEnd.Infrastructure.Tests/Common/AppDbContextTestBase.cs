using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

public abstract class AppDbContextTestBase : IDisposable
{
    protected readonly AppDbContext Context;

    protected AppDbContextTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
