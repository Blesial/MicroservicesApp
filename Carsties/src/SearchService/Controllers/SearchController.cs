using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
	// While is true that ApiController binds the query string to the action parameters,
	// When using a complex object as a parameter in an API controller action, it's best practice to explicitly
	// indicate where the data should come from
	[HttpGet]
	public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
	{
		// Extract the search term from the query string
		string searchTerm = searchParams.SearchTerm;
		string filterBy = searchParams.FilterBy;
		string orderBy = searchParams.OrderBy;
		int pageNumber = searchParams.PageNumber;
		int pageSize = searchParams.PageSize;
		string seller = searchParams.Seller;
		string winner = searchParams.Winner;

		// Create a paged search query
		// The PagedSearch method is a generic method that takes the type of the entity to be searched
		{
			var query = DB.PagedSearch<Item, Item>();
			// Check if the search term is null or empty
			if (!string.IsNullOrEmpty(searchTerm))
			{
				query.Match(Search.Full, searchTerm).SortByTextScore();
			}

			// ORDER BY
			query = orderBy switch 
			{
				"make" => query.Sort(x => x.Ascending(a => a.Make)),
				"new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
				_ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
			};

			// FILTER BY
			query = filterBy switch
			{
				"finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
				"endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
				_ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
			};


			// Seller Filter
			if (!string.IsNullOrEmpty(seller))
			{
				query.Match(x => x.Seller == seller);
			}
			// Winner Filter
			if (!string.IsNullOrEmpty(winner))
			{
				query.Match(x => x.Winner == winner);
			}

			// Set the page number and page size
			query.PageNumber(pageNumber);
			query.PageSize(pageSize);

			var result = await query.ExecuteAsync();

			return Ok(new
			{
				results = result.Results,
				pageCount = result.PageCount,
				totalCount = result.TotalCount,
			});
		}
	}
}
