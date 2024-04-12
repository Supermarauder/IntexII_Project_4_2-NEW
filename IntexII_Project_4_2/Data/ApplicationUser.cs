using Microsoft.AspNetCore.Identity;

namespace IntexII_Project_4_2.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public string? DateOfBirth { get; set; }
        public char? Gender { get; set; }
        public int? CustomerId { get; set; }
    }
}
