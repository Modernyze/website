using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Models;
using ModernyzeWebsite.Models.TimeLog;
using ModernyzeWebsite.Models.User;

namespace ModernyzeWebsite.Data;

public class ModernyzeWebsiteContext : DbContext {
    public ModernyzeWebsiteContext(DbContextOptions<ModernyzeWebsiteContext> options)
        : base(options) { }

    public DbSet<UserAccount> UserAccount { get; set; }

    public DbSet<UserPermission> UserPermission { get; set; }

    public DbSet<TimeLog> TimeLog { get; set; }

    public DbSet<Permissions> Permissions { get; set; }
}
