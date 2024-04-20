using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlace>
{
    public async Task Consume(ConsumeContext<BidPlace> context)
    {
        Console.WriteLine("====> Consuming bid placed");

        var auction = await DB.Find<Item>()
            .OneAsync(context.Message.AuctionId);

        ArgumentNullException.ThrowIfNull(auction);

        if (auction.CurrentHightBid == null ||
            context.Message.BidStatus.Contains("Accepted") &&
            context.Message.Amount > auction.CurrentHightBid)
        {
            auction.CurrentHightBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}
