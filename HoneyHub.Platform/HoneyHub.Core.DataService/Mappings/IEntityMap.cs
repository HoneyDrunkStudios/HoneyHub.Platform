using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Core.DataService.Mappings;

public interface IEntityMap
{
    void Configure(ModelBuilder modelBuilder);
}
