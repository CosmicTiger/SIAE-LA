#nullable enable
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities
{
    /// <summary>
    /// Represents an application user with extended profile information.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Returns a display string for the user's roles.
        /// </summary>
        /// <param name="roles">The collection of role names.</param>
        /// <returns>A comma-separated string of role display names.</returns>
        public string RoleDisplay(IEnumerable<string> roles)
        {
            return roles is null ? string.Empty : string.Join(", ", roles);
        }
    }
}