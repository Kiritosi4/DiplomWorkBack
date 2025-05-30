﻿using DiplomWork.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DiplomWork.Persistance.Configs
{
    internal class BudgetConfig : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Expenses)
                .WithOne(x => x.Budget)
                .HasForeignKey(x => x.BudgetId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

}
