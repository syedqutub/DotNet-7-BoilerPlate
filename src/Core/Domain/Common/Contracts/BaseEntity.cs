using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace Demo.WebApi.Domain.Common.Contracts;

public abstract class BaseEntity : BaseEntity<DefaultIdType>
{
}

public abstract class BaseEntity<TId> : IEntity<TId>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public TId Id { get; protected set; } = default!;

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new();
}