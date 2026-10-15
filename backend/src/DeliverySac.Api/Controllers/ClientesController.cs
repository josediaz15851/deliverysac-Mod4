using Dapper;
using DeliverySac.Api.Contracts.Clientes;
using DeliverySac.Api.Contracts.Common;
using DeliverySac.Api.Domain;
using DeliverySac.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = "Administrador")]
public class ClientesController : ControllerBase
{
    private readonly DeliverySacDbContext _db;

    public ClientesController(DeliverySacDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(
        [FromBody] ClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Direccion = request.Direccion.Trim(),
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ClienteDto { Id = cliente.Id, Nombre = cliente.Nombre, Direccion = cliente.Direccion };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, dto);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var currentPage = page < 1 ? 1 : page;
        var pageSize = size is < 1 or > 100 ? 10 : size;
        var offset = (currentPage - 1) * pageSize;

        const string sql = """
            SELECT Id, Nombre, Direccion, COUNT(*) OVER() AS Total
            FROM Clientes
            ORDER BY Nombre
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var connection = _db.Database.GetDbConnection();
        var rows = (await connection.QueryAsync<ClienteRow>(
            sql,
            new { Offset = offset, PageSize = pageSize })).ToList();

        var result = new PagedResult<ClienteDto>
        {
            Items = rows.Select(r => new ClienteDto { Id = r.Id, Nombre = r.Nombre, Direccion = r.Direccion }).ToList(),
            Total = rows.Count > 0 ? rows[0].Total : 0,
        };

        return Ok(result);
    }

    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Buscar(
        [FromQuery] string? q,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (20) Id, Nombre, Direccion
            FROM Clientes
            WHERE @Q IS NULL OR Nombre LIKE '%' + @Q + '%'
            ORDER BY Nombre;
            """;

        var connection = _db.Database.GetDbConnection();
        var rows = await connection.QueryAsync<ClienteDto>(sql, new { Q = string.IsNullOrWhiteSpace(q) ? null : q });

        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var cliente = await _db.Clientes.FindAsync(new object?[] { id }, cancellationToken);
        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(new ClienteDto { Id = cliente.Id, Nombre = cliente.Nombre, Direccion = cliente.Direccion });
    }

    private sealed class ClienteRow
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
