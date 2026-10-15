using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> builder)
    {
        builder.ToTable("Asignaciones");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Fecha).IsRequired();
        builder.Property(a => a.Activa).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.PedidoId)
            .IsUnique()
            .HasFilter("[Activa] = 1");

        builder.HasOne(a => a.Pedido)
            .WithMany(p => p.Asignaciones)
            .HasForeignKey(a => a.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Repartidor)
            .WithMany(u => u.Asignaciones)
            .HasForeignKey(a => a.RepartidorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Unidad)
            .WithMany(u => u.Asignaciones)
            .HasForeignKey(a => a.UnidadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
