namespace WarehouseLogistics.Products;

public sealed record Sku
{
    public Sku(SkuId id, string name, Ovh ovh)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (ovh.Volume <= 0 || ovh.Weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ovh));
        }

        Id = id;
        Name = name;
        Ovh = ovh;
    }

    public SkuId Id { get; }

    public string Name { get; }

    public Ovh Ovh { get; }
}
