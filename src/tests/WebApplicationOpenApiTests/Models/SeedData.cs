using System;
using System.Collections.Generic;

namespace WebApplicationOpenApiTests.Models
{
    /// <summary>
    /// In-memory seed data shared across handlers.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// A fixed set of users used by the demo handlers.
        /// </summary>
        public static readonly IReadOnlyList<UserDto> Users = new List<UserDto>
        {
            new UserDto { Id = new Guid("11111111-0000-0000-0000-000000000001"), Name = "Alice" },
            new UserDto { Id = new Guid("22222222-0000-0000-0000-000000000002"), Name = "Bob" },
            new UserDto { Id = new Guid("33333333-0000-0000-0000-000000000003"), Name = "Carol" }
        };
    }
}
