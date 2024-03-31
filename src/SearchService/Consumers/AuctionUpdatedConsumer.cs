using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        var itemUpdated = _mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
            .MatchID(itemUpdated.ID)
            .ModifyOnly(i => new
            {
                i.Color,
                i.Make,
                i.Model,
                i.Year,
                i.Mileage
            }, itemUpdated)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), "Problem update mongodb.");
        }
    }
}
