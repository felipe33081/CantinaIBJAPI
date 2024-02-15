namespace CantinaIBJ.Model;

public class DashboardData
{
    public int OrderQuantity { get; set; }
    public int OrdersFinished { get; set; }
    public int OrdersInProgress { get; set; }
    public int OrdersExcluded { get; set; }
    public decimal TotalValueAmount { get; set; }
    public decimal TotalValueFinishedAmount { get; set; }
    public decimal TotalValueInProgressAmount { get; set; }
    public decimal AverageOrderValueAmount { get; set; }
}