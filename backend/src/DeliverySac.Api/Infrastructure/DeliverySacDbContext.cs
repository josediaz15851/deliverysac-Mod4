using DeliverySac.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Infrastructure;

public class DeliverySacDbContext : DbContext
{
    public DeliverySacDbContext(DbContextOptions<DeliverySacDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Unidad> Unidades => Set<Unidad>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoLinea> PedidoLineas => Set<PedidoLinea>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<CambioEstado> CambiosEstado => Set<CambioEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliverySacDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
