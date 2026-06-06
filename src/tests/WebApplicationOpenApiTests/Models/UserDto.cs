using System;

namespace WebApplicationOpenApiTests.Models
{
    /// <summary>
    /// Represents a user returned by the /users endpoints.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string Name { get; set; }
    }
}
