namespace DeliverySac.Api.Contracts.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
}
