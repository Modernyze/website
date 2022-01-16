#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Data
{
    public class ModernyzeWebsiteContext : DbContext
    {
        public ModernyzeWebsiteContext (DbContextOptions<ModernyzeWebsiteContext> options)
            : base(options)
        {
        }

        public DbSet<ModernyzeWebsite.Models.UserAccount> UserAccount { get; set; }
    }
}
