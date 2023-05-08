using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace CantinaIBJ.Data.Context.Configuration;

internal class CustomerPersonConfiguration : IEntityTypeConfiguration<CustomerPerson>
{
    public void Configure(EntityTypeBuilder<CustomerPerson> builder)
    {
        builder.ToTable(nameof(CustomerPerson));

        builder
            .HasKey(k => k.Id);
    }
}