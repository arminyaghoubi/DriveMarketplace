using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"=====> AuctionCreatedConsumer: AuctionCreated(Id={context.Message.Id})");
        var newItem = _mapper.Map<Item>(context.Message);

        if (newItem.Model == "Foo")
        {
            throw new ArgumentException();
        }

        await newItem.SaveAsync();
    }
}
