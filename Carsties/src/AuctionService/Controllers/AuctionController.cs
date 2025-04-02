using AuctionService.Dtos;
using AuctionService.Entities;
using AuctionService.Infrastructure;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{	
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;

    public AuctionController(AuctionDbContext dbContext, IMapper mapper)
    {
        _auctionDbContext = dbContext;
		_mapper = mapper;
    }

	[HttpGet("")]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
	{
		var auctions = await _auctionDbContext.Auctions
		.Include(x => x.Item)
		.OrderBy(x => x.Item.Make)
		.ToListAsync();

		return _mapper.Map<List<AuctionDto>>(auctions);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		var auction = await _auctionDbContext.Auctions
		.Include(x => x.Item)
		.FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null)
		{
			return NotFound();
		}

		return _mapper.Map<AuctionDto>(auction);
	}

	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
	{	
		var auction = _mapper.Map<Auction>(auctionDto);

		// Check User Authentication as seller to create an auction
		auction.Seller = "Test";

		_auctionDbContext.Auctions.Add(auction);

		var result = await _auctionDbContext.SaveChangesAsync() > 0; // This saveChanges returns a number of what changes did
		// one or more rows affected

		if (!result)
		{
			return BadRequest("Could not save changes to the db");
		}

		// First we provide the name of the action where this resource can be found (getbyId)
		// Second we provide the parameter that the action where the resource is needs.
		// Finally we provide the auction Object created but we first need to convert it to a dto to be returned to the client.
		return CreatedAtAction(nameof(GetAuctionById),
		 	new { auction.Id },	_mapper.Map<AuctionDto>(auction));
		
		// This createdAtAction will return in the Headers of the HttpResponse a Key "Location"
		//  Pointing to the url where this resource can be taken from
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		// FindAsync does not support eager loading, so you wouldn’t be able to include Item if you used it.
		var auction = await _auctionDbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null)
		{
			return NotFound();
		}

		// TODO: Seller name matches the User name trying to update
		// check seller == username

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

		var result = await _auctionDbContext.SaveChangesAsync() > 0;

		return result ? Ok() : BadRequest("Problem saving changes!");
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAuction(Guid id)
	{
		// FindAsync is specifically optimized for primary key lookups.
		var auction = await _auctionDbContext.Auctions.FindAsync(id);

		if (auction == null) 
		{
			return NotFound(id);
		}

		// TODO: CHECK SELLER == USERNAME

		_auctionDbContext.Auctions.Remove(auction);

		var result = await _auctionDbContext.SaveChangesAsync() > 0;

		if (result) 
		{
			return Ok();
		}

		return BadRequest("Problem Saving Changes!");
	}

	// FirstOrDefaultAsync is useful for more complex queries that involve additional filtering or sorting.
	// For a simple lookup by primary key, FindAsync is the better choice.
}
