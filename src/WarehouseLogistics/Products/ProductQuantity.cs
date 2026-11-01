namespace WarehouseLogistics.Products;

public sealed record ProductQuantity(Sku Sku, Quantity Quantity)
{
    public Ovh TotalOvh => Sku.Ovh * Quantity;
}
