using System;
using System.Collections.Generic;

namespace DomainTracker.Entities.Models;

public partial class FavoriteDomain
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DomainId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Domain Domain { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
