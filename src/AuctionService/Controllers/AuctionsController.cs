﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("Api/Auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(DateTime? fromDate)
    {
        var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

        if (fromDate is not null)
        {
            query = query.Where(a => a.UpdateAt.CompareTo(fromDate.Value.ToUniversalTime()) > 0);
        }

        return await query
            .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpGet("{id:required:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null)
            return NotFound();

        return Ok(_mapper.Map<AuctionDto>(auction));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        auction.Seller = User.Identity.Name;

        await _context.Auctions.AddAsync(auction);

        var newAuctionDto = _mapper.Map<AuctionDto>(auction);
        AuctionCreated message = _mapper.Map<AuctionCreated>(newAuctionDto);
        await _publishEndpoint.Publish(message);

        var success = await _context.SaveChangesAsync() > 0;

        return success ?
            CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuctionDto) :
            BadRequest("The save failed");
    }

    [Authorize]
    [HttpPut("{id:required:guid}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller!= User.Identity.Name)
        {
            return Forbid();
        }

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

        var message = _mapper.Map<AuctionUpdated>(auction);
        await _publishEndpoint.Publish(message);

        var success = await _context.SaveChangesAsync() > 0;

        return success
            ? Ok()
            : BadRequest("The update failed");
    }

    [Authorize]
    [HttpDelete("{id:required:guid}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
            .FindAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }

        _context.Auctions.Remove(auction);

        AuctionDeleted message = new() { Id = auction.Id };
        await _publishEndpoint.Publish(message);

        var success = await _context.SaveChangesAsync() > 0;

        return success
            ? Ok()
            : BadRequest("The delete failed");
    }
}
