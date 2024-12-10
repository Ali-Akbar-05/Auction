using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.AuctionController;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{

    private readonly AuctionDbContext _dbCon;
    private readonly IMapper _mapper;
    public AuctionsController(AuctionDbContext dbCon, IMapper mapper)
    {
        _dbCon = dbCon;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuction()
    {
        var auction = await _dbCon.Auctions
        .Include(b => b.Item)
        .OrderBy(b => b.Item.Model)
        .ToListAsync();
        return _mapper.Map<List<AuctionDTO>>(auction);
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
        var result = await _dbCon.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDTO>(auction));
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
        var result = await _dbCon.SaveChangesAsync() > 0;
        if (result) return Ok();
        return BadRequest("Could not updated auction.");
    }
}