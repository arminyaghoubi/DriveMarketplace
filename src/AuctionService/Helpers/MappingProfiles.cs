﻿using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>()
            .IncludeMembers(a => a.Item);

        CreateMap<Item, AuctionDto>();

        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(a => a.Item, m => m.MapFrom(c => c));
        CreateMap<CreateAuctionDto, Item>();
    }
}
