using Microsoft.EntityFrameworkCore;

namespace dynamicUssdProject.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UssdMenu> UssdMenus { get; set; }
        public DbSet<Menu> Menus { get; set; }   // For the Menu table
        public DbSet<SubMenu> SubMenus { get; set; } // For the SubMenu table
        public DbSet<Class> Ussd { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure one-to-one relationship between User and Account
            modelBuilder.Entity<User>()
                .HasOne(u => u.Account)
                .WithOne(a => a.User)
                .HasForeignKey<Account>(a => a.UserId);

            // Ensure PhoneNumber is unique in both User and Account
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.PhoneNumber)
                .IsUnique();

            // Configure one-to-many relationship between Account and Transactions
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Transactions) // Account has many Transactions
                .WithOne(t => t.Account) // Each Transaction is related to one Account
                .HasForeignKey(t => t.AccountId); // Foreign key to Account
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
               .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Menu>()
               .HasMany(m => m.SubMenus)
               .WithOne(s => s.ParentMenu) // Each SubMenu has one parent Menu
               .HasForeignKey(s => s.ParentMenuId) // Foreign key property in SubMenu
               .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
