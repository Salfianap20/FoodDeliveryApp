namespace OrderService.GraphQL
{
    public record TrackingInput
    (
        int UserId,
        int CourierId,
        string Longitude,
        string Latitude
    );
}
