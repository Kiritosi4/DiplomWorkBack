using Microsoft.EntityFrameworkCore;
using DiplomWork.Models;
using DiplomWork.Persistance.Configs;

namespace DiplomWork.Persistance
{
    public class DiplomWorkDbContext : DbContext
    {
        public DiplomWorkDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Profit> Profits { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ProfitCategory> ProfitCategories { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Target> Targets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Budget>()
                .Property(e => e.PeriodType)
                .HasConversion(
                    v => v.ToString(),
                    v => (Period)Enum.Parse(typeof(Period), v));

            modelBuilder.ApplyConfiguration(new BudgetConfig());
            modelBuilder.ApplyConfiguration(new TargetConfig());
            modelBuilder.ApplyConfiguration(new ExpenseCategoryConfig());
            modelBuilder.ApplyConfiguration(new ProfitCategoryConfig());

            base.OnModelCreating(modelBuilder);
        }
    }
}
