using System;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure;

public class AuctionDbContext : DbContext
{
	public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
	{
	}

	public DbSet<Auction> Auctions { get; set; } // This will create a table named "Auctions"
	// aswell as a table for the Item class, which is a navigation property of Auction
}
