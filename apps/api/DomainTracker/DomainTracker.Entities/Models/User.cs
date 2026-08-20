using System;
using System.Collections.Generic;

namespace DomainTracker.Entities.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<FavoriteDomain> FavoriteDomains { get; set; } = new List<FavoriteDomain>();
}
