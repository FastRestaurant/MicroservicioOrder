namespace OrderService.Domain.Constants;

public static class ProductTypes
{
    public const string Dish = "Dish";
    public const string Drink = "Drink";

    public static bool IsValid(string productType)
        => productType is Dish or Drink;
}
