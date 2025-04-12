using AuctionService.Dtos;
using AuctionService.Entities;
using AuctionService.Infrastructure;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{	
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(AuctionDbContext dbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = dbContext;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
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

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
	{	
		var auction = _mapper.Map<Auction>(auctionDto);

		// Check User Authentication as seller to create an auction
		auction.Seller = User.Identity.Name;

		_auctionDbContext.Auctions.Add(auction);

		// Publish the auction created to the message broker
		var newAuction = _mapper.Map<AuctionDto>(auction);
		await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

		// Now the add and publish are done in the same transaction
		// This is important because if the publish fails, we don't want to save the auction to the database.
		// If the publish fails, we can roll back the transaction and not save the auction to the database.

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
		 	new { auction.Id },	newAuction);
		// This createdAtAction will return in the Headers of the HttpResponse a Key "Location"
		//  Pointing to the url where this resource can be taken from
	}

	[Authorize]
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		// FindAsync does not support eager loading, so you wouldn’t be able to include Item if you used it.
		var auction = await _auctionDbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null)
		{
			return NotFound();
		}

		// Seller name matches the User name trying to update
		if (auction.Seller != User.Identity.Name) return Forbid();

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

		var updatedAuction = _mapper.Map<AuctionUpdated>(auction);

		await _publishEndpoint.Publish(updatedAuction);

		var result = await _auctionDbContext.SaveChangesAsync() > 0;

		return result ? Ok() : BadRequest("Problem saving changes!"); 
	}

	[Authorize]
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
		if (auction.Seller != User.Identity.Name) return Forbid();

		_auctionDbContext.Auctions.Remove(auction);

		// Publish the auction deleted to the message broker
		await _publishEndpoint.Publish(new AuctionDeleted
		{
			Id = auction.Id.ToString()
		});

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
