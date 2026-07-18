// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // === ROLES ===
            string[] roleNames = { "Administrator", "Recruiter", "Candidate" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // === ADMIN USER ===
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

            // === DEMO ATTRIBUTES ===
            if (!context.AttributeDefinitions.Any())
            {
                var demoAttributes = new List<AttributeDefinition>
                {
                    new AttributeDefinition { Name = "English Level", Category = "Languages", Type = "Dropdown", Options = "Beginner,Intermediate,Advanced,Fluent", IsRequired = true },
                    new AttributeDefinition { Name = "GPA", Category = "Education", Type = "Number", IsRequired = true, Description = "Grade Point Average (out of 4.0)" },
                    new AttributeDefinition { Name = "Years of Experience", Category = "Experience", Type = "Number", IsRequired = true },
                    new AttributeDefinition { Name = "Current Location", Category = "Personal Information", Type = "Text", IsRequired = false },
                    new AttributeDefinition { Name = "Skills", Category = "Skills", Type = "Text", IsRequired = true },
                    new AttributeDefinition { Name = "Available for Immediate Join", Category = "Availability", Type = "Boolean", IsRequired = true },
                    new AttributeDefinition { Name = "IELTS Score", Category = "Certifications", Type = "Number", IsRequired = false }
                };

                context.AttributeDefinitions.AddRange(demoAttributes);
                await context.SaveChangesAsync();
            }

            // === DEMO POSITIONS ===
            if (!context.Positions.Any())
            {
                var positions = new List<Position>
                {
                    new Position
                    {
                        Title = "Senior .NET Developer",
                        Company = "TechVision LLC",
                        Level = "Senior",
                        Description = "Looking for experienced backend developer with strong .NET skills.",
                        ProjectTags = "C#, ASP.NET, SQL, Azure",
                        MaxProjects = 5,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Position
                    {
                        Title = "Business Analyst",
                        Company = "Global Finance",
                        Level = "Mid",
                        Description = "Join our dynamic team to analyze business processes.",
                        ProjectTags = "Requirements, UML, Power BI",
                        MaxProjects = 4,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Position
                    {
                        Title = "React Frontend Developer",
                        Company = "InnovateX",
                        Level = "Mid",
                        Description = "Build modern web applications with React and TypeScript.",
                        ProjectTags = "React, TypeScript, Tailwind",
                        MaxProjects = 6,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Position
                    {
                        Title = "QA Engineer",
                        Company = "QualitySoft",
                        Level = "Junior",
                        Description = "Manual and automated testing for enterprise products.",
                        ProjectTags = "Selenium, Postman, Jira",
                        MaxProjects = 3,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    }
                };

                context.Positions.AddRange(positions);
                await context.SaveChangesAsync();

                // Assign some attributes to positions
                var allAttributes = await context.AttributeDefinitions.ToListAsync();
                var firstPosition = positions[0];
                var secondPosition = positions[1];

                firstPosition.PositionAttributes.Add(new PositionAttribute { AttributeDefinitionId = allAttributes[0].Id, Order = 0 }); // English Level
                firstPosition.PositionAttributes.Add(new PositionAttribute { AttributeDefinitionId = allAttributes[2].Id, Order = 1 }); // Years of Experience

                secondPosition.PositionAttributes.Add(new PositionAttribute { AttributeDefinitionId = allAttributes[1].Id, Order = 0 }); // GPA
                secondPosition.PositionAttributes.Add(new PositionAttribute { AttributeDefinitionId = allAttributes[5].Id, Order = 1 }); // Available for Join

                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }
    }
}