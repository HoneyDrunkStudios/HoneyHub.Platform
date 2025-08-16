using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Users;

public class UserMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserEntity>();
        entity.ToTable("User");
        entity.HasKey(x => x.Id);
    }
}
