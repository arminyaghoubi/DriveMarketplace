using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("Api/Search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> SearchItem([FromQuery] SearchParams request)
    {
        try
        {
            //var query = DB.Find<Item>();
            var query = DB.PagedSearch<Item, Item>();

            if (!string.IsNullOrEmpty(request.Term))
            {
                query.Match(Search.Full, request.Term)
                    .SortByTextScore();
            }

            query = request.OrderBy switch
            {
                "make" => query.Sort(s => s.Ascending(i => i.Make)),
                "new" => query.Sort(s => s.Descending(i => i.CreateAt)),
                _ => query.Sort(s => s.Ascending(i => i.AuctionEnd))
            };

            query = request.FilerBy switch
            {
                "finished" => query.Match(t => t.AuctionEnd < DateTime.UtcNow),
                "endingsoon" => query.Match(t => t.AuctionEnd < DateTime.UtcNow.AddHours(6) && t.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(t => t.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(request.Seller))
            {
                query.Match(t => t.Seller == request.Seller);
            }

            if (!string.IsNullOrEmpty(request.Winner))
            {
                query.Match(t => t.Winner == request.Winner);
            }

            query.PageNumber(request.PageNumber);
            query.PageSize(request.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(
                new
                {
                    result.Results,
                    result.PageCount,
                    result.TotalCount
                });
        }
        catch(Exception ex)
        {
            Console.WriteLine("Armin {0}",ex.Message);
            throw;
        }
    }
}
