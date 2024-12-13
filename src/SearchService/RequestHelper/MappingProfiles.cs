using AutoMapper;
using Contracts.Auctions;
using SearchService.Models;

namespace SearchService.RequestHelper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}