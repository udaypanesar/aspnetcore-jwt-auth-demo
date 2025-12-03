using AuthSystemDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystemDemo.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        
        DbSet<User> Users { get; set; }
    }
}
