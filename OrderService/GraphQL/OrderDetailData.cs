namespace OrderService.GraphQL
{
    public record OrderDetailData
    (
        int? Id,
        int? OrderId,
        int FoodId,
        int Quantity
    );
}
