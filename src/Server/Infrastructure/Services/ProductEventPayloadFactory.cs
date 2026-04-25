using System.Collections.Generic;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Events;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Services;

public class ProductEventPayloadFactory : IEventPayloadFactory<Product>
{
    public object CreateCreatedEvent(Product entity)
    {
        return new ProductCreatedEvent
        {
            ProductId = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Stock = entity.Stock,
            Category = entity.Category,
            IsActive = entity.IsActive
        };
    }

    public object CreateUpdatedEvent(Product entity, Dictionary<string, object> changes)
    {
        return new ProductUpdatedEvent
        {
            ProductId = entity.Id,
            Changes = changes
        };
    }

    public object CreateDeletedEvent(Product entity)
    {
        return new ProductDeletedEvent { ProductId = entity.Id };
    }
}
