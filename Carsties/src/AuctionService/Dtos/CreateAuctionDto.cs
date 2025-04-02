using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dtos;

// Automatic Model Validation:
// With the [ApiController] attribute on your controllers, 
// ASP.NET Core automatically validates the incoming DTO against its Data Annotations. 
// If the validation fails, the framework returns a 400 Bad Request with details about the errors.

// Use Fluent Validation if the validation logic is more complex and we also keep the dtos "cleaner"
public class CreateAuctionDto
{
	[Required]
	public string Make { get; set; }
	
	[Required]
	public string Model { get; set; }

	[Required]
	public int Year { get; set; }

	[Required]
	public string Color { get; set; }

	[Required]
	public int Mileage { get; set; }

	[Required]
	public string ImageUrl { get; set; }

	[Required]
	public int ReservePrice { get; set; }

	[Required]
	public DateTime AuctionEnd { get; set; }
}
