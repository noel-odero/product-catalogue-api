using Microsoft.AspNetCore.Identity;

namespace ProductCatalogue.Models;

public class User : IdentityUser<Guid> // email, passwornHash, username and authentication
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}