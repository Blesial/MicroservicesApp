using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
		CreateMap<Item, AuctionDto>();		
		CreateMap<CreateAuctionDto, Auction>()
			.ForMember(d => d.Item, o => o.MapFrom(s => s)); // S is the source itself.
		CreateMap<CreateAuctionDto, Item>();
	}
}
