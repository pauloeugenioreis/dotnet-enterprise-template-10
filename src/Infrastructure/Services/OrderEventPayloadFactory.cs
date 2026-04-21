using System.Collections.Generic;
using System.Linq;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Events;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Services;

public class OrderEventPayloadFactory : IEventPayloadFactory<Order>
{
    public object CreateCreatedEvent(Order entity)
    {
        return new OrderCreatedEvent
        {
            OrderId = entity.Id,
            OrderNumber = entity.OrderNumber,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            CustomerPhone = entity.CustomerPhone,
            ShippingAddress = entity.ShippingAddress,
            Subtotal = entity.Subtotal,
            Tax = entity.Tax,
            ShippingCost = entity.ShippingCost,
            Total = entity.Total,
            Notes = entity.Notes,
            Items = entity.Items?.Select(i => new OrderItemData
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList() ?? new List<OrderItemData>()
        };
    }

    public object CreateUpdatedEvent(Order entity, Dictionary<string, object> changes)
    {
        return new OrderUpdatedEvent
        {
            OrderId = entity.Id,
            Changes = changes
        };
    }

    public object CreateDeletedEvent(Order entity)
    {
        return new OrderDeletedEvent { OrderId = entity.Id };
    }
}
