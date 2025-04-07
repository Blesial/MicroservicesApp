using AuctionService.Dtos;
using AuctionService.Entities;
using AuctionService.Infrastructure;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
	{
		// Using AsQueryable() allows you to build up a query dynamically.
		// This is useful when you want to apply additional filters or sorting based on user input.
		// based on runtime information.
		var query = _auctionDbContext.Auctions
			.OrderBy(x => x.Item.Make)
			.AsQueryable(); //  To make further queries

		if (!string.IsNullOrEmpty(date))
		{
			// If the parsed date is interpreted as local time (or as an unspecified kind),
			//  and your database stores dates in UTC, the comparison could be off by the difference 
			// between the local time zone and UTC.
			query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
		}


		return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
			.ToListAsync();
		// ProjectTo is a method provided by AutoMapper that allows you to project your query directly into a DTO.
		// This is more efficient than loading the entire entity and then mapping it to a DTO.
		// It generates a SQL query that selects only the fields you need for the DTO,
		//  which can significantly reduce the amount of data transferred from the database.
		// This is especially useful when dealing with large datasets or when you only need a subset of the data.
		// The ToListAsync method executes the query and returns the results as a list.
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
