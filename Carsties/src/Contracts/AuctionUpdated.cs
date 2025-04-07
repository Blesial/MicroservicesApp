namespace Contracts;

public class AuctionUpdated
{
	public string Id { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
	public int Mileage { get; set; }
}

// AuctionCreated: Uses a Guid because it’s about generating a new, unique identity for an auction.
//AuctionUpdated: Uses a string because it’s referring to an existing auction. The id is represented as a string either for consistency with other parts of your system (like a database or API) or due to specific design requirements (e.g., human-readable or legacy format).