using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Data; 

public class ModernyzeWebsiteContext : DbContext {
    public ModernyzeWebsiteContext(DbContextOptions<ModernyzeWebsiteContext> options)
        : base(options) { }

    public DbSet<UserAccount> UserAccount { get; set; }

    public DbSet<UserPermission> UserPermission { get; set; }
}
