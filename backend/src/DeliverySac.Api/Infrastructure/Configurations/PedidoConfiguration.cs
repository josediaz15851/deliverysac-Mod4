using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySac.Api.Infrastructure.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FechaReparto).IsRequired();
        builder.Property(p => p.Comentario).HasMaxLength(500);
        builder.Property(p => p.Estado).IsRequired();
        builder.Property(p => p.Total).HasPrecision(18, 2);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.FechaReparto, p.Estado });

        builder.HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
