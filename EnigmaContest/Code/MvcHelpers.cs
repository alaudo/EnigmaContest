using EnigmaContest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnigmaContest.Code
{
    public class MvcHelpers
    {
        public static string GetUsername()
        {
            var claimsIdentity = HttpContext.Current.User.Identity as System.Security.Claims.ClaimsIdentity;
            var displayNameClaim = claimsIdentity != null
                ? claimsIdentity.Claims.SingleOrDefault(x => x.Type == ApplicationUser.DisplayNameClaimType)
                : null;
            return displayNameClaim == null ? HttpContext.Current.User.Identity.Name : displayNameClaim.Value;
        }

        public static User GetUser()
        {
            var user = new User();
            var claimsIdentity = HttpContext.Current.User.Identity as System.Security.Claims.ClaimsIdentity;
            var displayNameClaim = claimsIdentity != null
                ? claimsIdentity.Claims.SingleOrDefault(x => x.Type == ApplicationUser.DisplayNameClaimType)
                : null;

            user.Username = displayNameClaim == null ? HttpContext.Current.User.Identity.Name : displayNameClaim.Value;

            var ageClaim = claimsIdentity != null
                ? claimsIdentity.Claims.SingleOrDefault(x => x.Type == ApplicationUser.AgeClaimType)
                : null;
            user.Age = Convert.ToInt32(ageClaim == null ? "0" : ageClaim.Value);

            return user;
        }

        public static bool IsAdmin()
        {
            return GetUsername() == "alaudo";
        }

    }
}