using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class CambioEstadoConfiguration : IEntityTypeConfiguration<CambioEstado>
{
    public void Configure(EntityTypeBuilder<CambioEstado> builder)
    {
        builder.ToTable("CambiosEstado");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.EstadoNuevo).IsRequired();
        builder.Property(c => c.FechaHora).IsRequired();
        builder.Property(c => c.Motivo).HasMaxLength(200);
        builder.Property(c => c.OperacionId).IsRequired();

        builder.HasIndex(c => new { c.PedidoId, c.FechaHora });
        builder.HasIndex(c => c.OperacionId).IsUnique();

        builder.HasOne(c => c.Pedido)
            .WithMany(p => p.CambiosEstado)
            .HasForeignKey(c => c.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.CambiosEstado)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
