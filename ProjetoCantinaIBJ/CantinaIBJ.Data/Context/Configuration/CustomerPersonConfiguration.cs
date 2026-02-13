using CantinaIBJ.Model.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CantinaIBJ.Data.Context.Configuration;

internal class CustomerPersonConfiguration : IEntityTypeConfiguration<CustomerPerson>
{
    public void Configure(EntityTypeBuilder<CustomerPerson> builder)
    {
        builder.ToTable(nameof(CustomerPerson));

        builder
            .HasKey(k => k.Id);

        builder
            .HasIndex(p => p.Name)
            .IsUnique();
    }
}