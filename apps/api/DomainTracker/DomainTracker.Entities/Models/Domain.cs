using System;
using System.Collections.Generic;

namespace DomainTracker.Entities.Models;

public partial class Domain
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public DateTime LastCheckedAt { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<FavoriteDomain> FavoriteDomains { get; set; } = new List<FavoriteDomain>();
}
