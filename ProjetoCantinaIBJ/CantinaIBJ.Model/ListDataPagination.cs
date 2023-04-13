namespace CantinaIBJ.Model;

public class ListDataPagination<T>
{
    public int Page { get; set; }
    public int TotalPages { get; set; } = 0;
    public int TotalItems { get; set; }
    public List<T> Data { get; set; }
    public string? PaginationToken { get; set; } = null;
}