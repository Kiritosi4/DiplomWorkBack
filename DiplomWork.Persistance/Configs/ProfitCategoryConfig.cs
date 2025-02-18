using DiplomWork.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Persistance.Configs
{
    internal class ProfitCategoryConfig : IEntityTypeConfiguration<ProfitCategory>
    {
        public void Configure(EntityTypeBuilder<ProfitCategory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Profits)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
