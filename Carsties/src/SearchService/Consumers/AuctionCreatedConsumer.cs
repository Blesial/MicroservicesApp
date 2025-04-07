using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

// MassTransit is convention based
// The consumer name should be the same as the event name
// The consumer should be in the same namespace as the event AND use Consumer suffix
// The consumer should implement the IConsumer interface
// The consumer should have a constructor that takes the event as a parameter
// The consumer should have a method that takes the event as a parameter

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
	private readonly IMapper _mapper;
	public AuctionCreatedConsumer(IMapper mapper)
	{
		_mapper = mapper;
	}
	public async Task Consume(ConsumeContext<AuctionCreated> context)
	{
		Console.WriteLine("--> Consuming AuctionCreated Event " + context.Message.Id);
		// We need to map the AuctionCreated event to the Item entity
		var item = _mapper.Map<Item>(context.Message);

		if (item.Model == "Foo") 
		{
			throw new ArgumentException("Cannot sell cars with name Foo");
		}

		await item.SaveAsync();
	}	
}
