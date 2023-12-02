using ContactManager.Authorization;
using ContactManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// dotnet aspnet-codegenerator razorpage -m Contact -dc ApplicationDbContext -udl -outDir Pages\Contacts --referenceScriptLibraries

namespace ContactManager.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // For sample purposes seed both with the same password.
                // Password is set with the following:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything

                var adminID = await EnsureUser(serviceProvider, testUserPw, "admin@contoso.com");
                await EnsureRole(serviceProvider, adminID, Constants.ContactAdministratorsRole);

                // allowed user can create and edit contacts that they create
                var managerID = await EnsureUser(serviceProvider, testUserPw, "manager@contoso.com");
                await EnsureRole(serviceProvider, managerID, Constants.ContactManagersRole);

                SeedDB(context, adminID);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                    string testUserPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = UserName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var existingUsers = await userManager.Users.ToListAsync();
            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
            {
                throw new Exception("The testUserPw password was probably not strong enough!");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

        public static void SeedDB(ApplicationDbContext context, string adminID)
        {
            if (context.Contact.Any())
            {
                return;   // DB has been seeded
            }

            context.Contact.AddRange(
                new Contact
                {
                    Name = "Steve Richard",
                    Address = "Richmod hill",
                    City = "Scarbrough",
                    State = "ON",
                    Zip = "24192",
                    Email = "Stever@example.com",
                    Status = ContactStatus.Approved,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Elisa dees",
                    Address = "345 Fanfare dr ",
                    City = "Scarbrough",
                    State = "ON",
                    Zip = "24192",
                    Email = "elisa345@yopmail.com",
                    Status = ContactStatus.Submitted,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "renold limsay",
                    Address = "321 Kingston road",
                    City = "Scarbrough",
                    State = "ON",
                    Zip = "24192",
                    Email = "renolddunkie@example.com",
                    Status = ContactStatus.Rejected,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Johanthan adams",
                    Address = "4564 manse St",
                    City = "Oshawa",
                    State = "ON",
                    Zip = "24193",
                    Email = "adammails@example.com",
                    Status = ContactStatus.Submitted,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Dilsy ninan",
                    Address = "7890 2nd Ave E",
                    City = "Ajax",
                    State = "ON",
                    Zip = "24190",
                    Email = "dilsyjohn@example.com",
                    OwnerID = adminID
                }
             );
            context.SaveChanges();
        }

    }
}
