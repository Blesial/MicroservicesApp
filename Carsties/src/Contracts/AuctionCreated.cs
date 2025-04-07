namespace Contracts;
// Something to have in mind when using MassTransit
// Is that Events needs to be in the same namespace as the Consumer.
// This is because MassTransit uses the namespace to determine the routing key for the message.
// So if you have a consumer in the namespace "Carsties.AuctionService.Consumers"
// and the event in "Carsties.AuctionService.Events",
// the consumer won't be able to consume the event.
// So to avoid this, we need to have the event in the same namespace as the consumer.
public class AuctionCreated
{
	public Guid Id { get; set; }
	public int ReservePrice { get; set; }
	public string Seller { get; set; } 
	public string Winner { get; set; }
	public int SoldAmount { get; set; }
	public int CurrentHighBid { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public DateTime AuctionEnd { get; set; }
	public string Status { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
	public int Mileage { get; set; }
	public string ImageUrl { get; set; }
}
