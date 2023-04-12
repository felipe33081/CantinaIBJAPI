using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace CantinaIBJ.Data.Context.Configuration;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(nameof(Order));

        builder
            .HasKey(k => k.Id);

        builder
            .HasOne(o => o.CustomerPerson);

        builder
            .HasMany(c => c.Products);
    }
}