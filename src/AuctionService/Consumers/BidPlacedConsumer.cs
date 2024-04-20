using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlace>
{
    private readonly AuctionDbContext _context;

    public BidPlacedConsumer(AuctionDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<BidPlace> context)
    {
        await Console.Out.WriteLineAsync("====> Consuming bid placed");

        var auction = await _context.Auctions.FindAsync(context.Message.AuctionId);

        ArgumentNullException.ThrowIfNull(auction);

        if (auction.CurrentHightBid is null ||
            context.Message.BidStatus.Contains("Accepted") &&
            context.Message.Amount > auction.CurrentHightBid)
        {
            auction.CurrentHightBid = context.Message.Amount;
            await _context.SaveChangesAsync();
        }
    }
}
