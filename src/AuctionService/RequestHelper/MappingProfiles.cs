using AuctionService.Dtos;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts.Auctions;

namespace AuctionService.RequestHelper;

public class MappingProfiles : Profile
{

    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDTO>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDTO>();

        CreateMap<CreateAuctionDto, Auction>()
        .ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<AuctionDTO, AuctionCreated>();
        CreateMap<Auction,AuctionUpdated>().IncludeMembers(b=>b.Item);
        CreateMap<Item,AuctionUpdated>();

    }
}