using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
	private readonly IMapper _mapper;
	public AuctionUpdatedConsumer(IMapper mapper)
	{
		_mapper = mapper;
	}
	public async Task Consume(ConsumeContext<AuctionUpdated> context)
	{
		Console.WriteLine("--> Consuming AuctionUpdated Event " + context.Message.Id);
	
		var item = _mapper.Map<Item>(context.Message);

		var result = await DB.Update<Item>()
			.MatchID(context.Message.Id)
			.ModifyWith(item)
			.ExecuteAsync();

		if (!result.IsAcknowledged)
		{
			Console.WriteLine($"--> Failed to update auction with id {context.Message.Id}");
			return;
		}
	}	
}
