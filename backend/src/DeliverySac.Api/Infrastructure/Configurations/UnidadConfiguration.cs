using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class UnidadConfiguration : IEntityTypeConfiguration<Unidad>
{
    public void Configure(EntityTypeBuilder<Unidad> builder)
    {
        builder.ToTable("Unidades");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Identificador).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Tipo).IsRequired();
        builder.Property(u => u.Activo).HasDefaultValue(true);

        builder.HasIndex(u => u.Identificador).IsUnique();
    }
}
