using DiplomWork.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Persistance.Configs
{
    internal class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Budgets)
                .WithOne()
                .HasForeignKey(x => x.OwnerId);

            builder.HasMany(x => x.Expenses)
                .WithOne()
                .HasForeignKey(x => x.OwnerId);

            builder.HasMany(x => x.Targets)
                .WithOne()
                .HasForeignKey(x => x.OwnerId);

            builder.HasMany(x => x.Profits)
                .WithOne()
                .HasForeignKey(x => x.OwnerId);

            builder.HasMany(x => x.ExpenseCategories)
                .WithOne()
                .HasForeignKey(x => x.OwnerID);

            builder.HasMany(x => x.ProfitCategories)
                .WithOne()
                .HasForeignKey(x => x.OwnerID);

        }
    }
}
