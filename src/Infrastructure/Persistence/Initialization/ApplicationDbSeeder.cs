using Demo.WebApi.Infrastructure.Persistence.Context;
using Demo.WebApi.Shared.Authorization;
using Demo.WebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demo.WebApi.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in AppRoles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogInformation("Seeding {role} Role.", roleName);
                role = new ApplicationRole(roleName, $"{roleName} Role");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == AppRoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, AppPermissions.Basic, role);
            }
            else if (roleName == AppRoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, AppPermissions.Admin, role);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyList<AppPermission> permissions, ApplicationRole role)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == AppClaims.Permission && c.Value == permission.Name))
            {
                _logger.LogInformation("Seeding {role} Permission '{permission}'.", role.Name, permission.Name);
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = AppClaims.Permission,
                    ClaimValue = permission.Name,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }

    public static class InitialDataSeedConstants
    {
        public const string AdminEmail = "admin@mailinator.com";
        public const string AdminUserName = "Admin";
        public const string AdminFirstName = "Admin";
        public const string AdminLastName = "Admin";
        public const string DefaultPassword = "Hallo123$";
    }

    private async Task SeedAdminUserAsync()
    {
        if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == InitialDataSeedConstants.AdminEmail)
            is not ApplicationUser adminUser)
        {
            adminUser = new ApplicationUser
            {
                FirstName = InitialDataSeedConstants.AdminFirstName,
                LastName = InitialDataSeedConstants.AdminLastName,
                Email = InitialDataSeedConstants.AdminEmail,
                UserName = InitialDataSeedConstants.AdminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = InitialDataSeedConstants.AdminEmail?.ToUpperInvariant(),
                NormalizedUserName = InitialDataSeedConstants.AdminUserName.ToUpperInvariant(),
                IsActive = true
            };

            _logger.LogInformation("Seeding Default Admin User.");
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, InitialDataSeedConstants.DefaultPassword);
            await _userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            _logger.LogInformation("Assigning Admin Role to Admin User.");
            await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }
    }
}