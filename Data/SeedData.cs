using Microsoft.AspNetCore.Identity;

using OnlineTicket.Data; 

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Call the actual seeding method
                await SeedAdminUserAndRoles(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }

    private static async Task SeedAdminUserAndRoles(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Create Customer and Organizer roles (optional but good practice)
        string[] roles = { "Admin", "Organizer", "Customer" };
        foreach (var roleName in roles)
        {
            if (await roleManager.FindByNameAsync(roleName) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Role '{roleName}' created.");
            }
        }

        // 2. Initial Admin Account Info
        string adminEmail = "admin@gmail.com";
        string adminPassword = "Asd123!@#";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createAdminResult.Succeeded)
            {
                // Add Admin user to Admin Role
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Admin user '{adminEmail}' created and assigned to 'Admin' role.");
            }
            else
            {
                Console.WriteLine($"Error creating admin user: {string.Join(", ", createAdminResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Optional: Ensure existing admin user has the admin role
            var existingAdminUser = await userManager.FindByEmailAsync(adminEmail);
            if (!await userManager.IsInRoleAsync(existingAdminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdminUser, "Admin");
                Console.WriteLine($"Existing user '{adminEmail}' assigned to 'Admin' role.");
            }
        }
    }
}