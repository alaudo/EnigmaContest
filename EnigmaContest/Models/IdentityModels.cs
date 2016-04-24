using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace EnigmaContest.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public const string DisplayNameClaimType = "DisplayName";
        public const string AgeClaimType = "Age";
        public const string GradeClaimType = "Grade";

        [Required]
        [Display]
        [MaxLength(128)]
        public string DisplayName { get; set; }

        [Required]
        [Range(5,99,ErrorMessage = "Age must be between 5 and 99 years old")]
        public int Age { get; set; }

        [Required]
        public int Grade { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim(DisplayNameClaimType, DisplayName));
            userIdentity.AddClaim(new Claim(AgeClaimType, Age.ToString()));
            userIdentity.AddClaim(new Claim(GradeClaimType, Grade.ToString()));

            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}