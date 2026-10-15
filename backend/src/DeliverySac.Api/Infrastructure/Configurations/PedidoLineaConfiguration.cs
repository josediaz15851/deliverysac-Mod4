using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class PedidoLineaConfiguration : IEntityTypeConfiguration<PedidoLinea>
{
    public void Configure(EntityTypeBuilder<PedidoLinea> builder)
    {
        builder.ToTable("PedidoLineas");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Producto).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Cantidad).IsRequired();
        builder.Property(l => l.PrecioUnitario).HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.Descripcion).IsRequired().HasMaxLength(300);

        builder.HasOne(l => l.Pedido)
            .WithMany(p => p.Lineas)
            .HasForeignKey(l => l.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
