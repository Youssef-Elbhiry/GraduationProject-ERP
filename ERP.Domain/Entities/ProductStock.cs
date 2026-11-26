namespace ERP.Domain.Entities;

public class ProductStock
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal OnHandQty { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
