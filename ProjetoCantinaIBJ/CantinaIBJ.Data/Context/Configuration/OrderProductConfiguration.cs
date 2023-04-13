using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace CantinaIBJ.Data.Context.Configuration;

internal class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {
        builder.ToTable(nameof(OrderProduct));

        builder
            .HasKey(pp => new { pp.OrderId, pp.ProductId });

        builder
            .HasOne(o => o.Order);

        builder
            .HasOne(o => o.Product);
    }
}