using System;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure;

public class AuctionDbContext : DbContext
{
	public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
	{
	}

	public DbSet<Auction> Auctions { get; set; } // This will create a table named "Auctions"
	// aswell as a table for the Item class, which is a navigation property of Auction

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder); // This is not necessary, but it's a good practice to call the base method

			modelBuilder.AddInboxStateEntity(); // This will create a table named "InboxState" in the database
			modelBuilder.AddOutboxStateEntity(); // This will create a table named "OutboxState" in the database
			modelBuilder.AddOutboxMessageEntity(); // This will create a table named "OutboxMessage" in the database
	}
}
