using API.Entities;
using API.Entities.OrderAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole {Id = "be88fd5e-8011-4713-bccb-cf7a99cd4248", Name = "Member", NormalizedName = "MEMBER"},
                new IdentityRole {Id = "7faf6464-f46f-410b-99b1-d2d243c79d2a", Name = "Admin", NormalizedName = "ADMIN"}
            );
    }
}
