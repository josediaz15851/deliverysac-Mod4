using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(100);
        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Rol).IsRequired();
        builder.Property(u => u.Activo).HasDefaultValue(true);

        builder.HasIndex(u => u.NombreUsuario).IsUnique();
    }
}
