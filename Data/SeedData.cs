using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Roles
            string[] roleNames = { "Administrator", "Recruiter", "Candidate" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin User
            var adminEmail = "admin@talentacquisition.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                    await userManager.AddToRoleAsync(user, "Recruiter");
                }
            }

            // Force Demo Attributes - Clear existing first for clean test
            if (context.AttributeDefinitions.Any())
            {
                context.AttributeDefinitions.RemoveRange(context.AttributeDefinitions);
                await context.SaveChangesAsync();
            }

            // Add Demo Attributes
            var demoAttributes = new List<AttributeDefinition>
            {
                new AttributeDefinition { Name = "English Level", Type = "Dropdown", Options = "Beginner,Intermediate,Advanced,Fluent", IsRequired = true },
                new AttributeDefinition { Name = "GPA", Type = "Number", IsRequired = true, Description = "Grade Point Average (out of 4.0)" },
                new AttributeDefinition { Name = "Years of Experience", Type = "Number", IsRequired = true },
                new AttributeDefinition { Name = "Current Location", Type = "Text", IsRequired = false },
                new AttributeDefinition { Name = "Skills", Type = "Text", IsRequired = true, Description = "List your key technical and soft skills" },
                new AttributeDefinition { Name = "Available for Immediate Join", Type = "Boolean", IsRequired = true }
            };

            context.AttributeDefinitions.AddRange(demoAttributes);
            await context.SaveChangesAsync();
        }
    }
}