using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TokenServiceApi.Models;

namespace TokenServiceApi.Data
{
    public class IdentityDbInit
    {
        public static async void Initialize(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            context.Database.Migrate();

            var meMyemailCom = "me@myemail.com";
            if (context.Users.Any(r => r.UserName == meMyemailCom))
                return;
            var pass = "Password";
            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName =  meMyemailCom,
                Email = meMyemailCom,
                EmailConfirmed = true
            },pass);

        }
    }
}