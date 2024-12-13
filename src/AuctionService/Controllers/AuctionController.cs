using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.AuctionController;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{

    private readonly AuctionDbContext _dbCon;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext dbCon, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _dbCon = dbCon;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }


    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuction(string date)
    {

        var query = _dbCon.Auctions.OrderBy(b => b.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(b => b.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _dbCon.Auctions
        .Include(b => b.Item)
         .FirstOrDefaultAsync(b => b.Id == id);

        if (auction == null) return NotFound();

        return _mapper.Map<AuctionDTO>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDto auctionDto)
    {

        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO : add current user as seler
        auction.Seller = "test";
        _dbCon.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);
        var auctionCreated = _mapper.Map<AuctionCreated>(newAuction);
        await _publishEndpoint.Publish(auctionCreated);

        var result = await _dbCon.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save");



        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _dbCon.Auctions.Include(b => b.Item)
        .FirstOrDefaultAsync(b => b.Id == id);
        if (auction == null) return NotFound();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
        var result = await _dbCon.SaveChangesAsync() > 0;
        if (result) return Ok();
        return BadRequest("Problem saving change.");
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _dbCon.Auctions.FirstOrDefaultAsync(b => b.Id == id);
        if (auction == null) return NotFound();

        _dbCon.Auctions.Remove(auction);
        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });
        var result = await _dbCon.SaveChangesAsync() > 0;
        if (result) return Ok();
        return BadRequest("Could not updated auction.");
    }
}