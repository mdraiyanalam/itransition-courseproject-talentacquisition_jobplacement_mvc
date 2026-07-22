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

            // === RECRUITER USER ===
            var recruiterEmail = "recruiter@talentacquisition.local";
            var recruiterUser = await userManager.FindByEmailAsync(recruiterEmail);
            if (recruiterUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = recruiterEmail,
                    Email = recruiterEmail,
                    FullName = "Demo Recruiter",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, "Recruiter@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Recruiter");
                }
            }

            // === CANDIDATE USER ===
            var candidateEmail = "candidate@talentacquisition.local";
            var candidateUser = await userManager.FindByEmailAsync(candidateEmail);
            if (candidateUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = candidateEmail,
                    Email = candidateEmail,
                    FullName = "Demo Candidate",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, "Candidate@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Candidate");

                    // create a candidate profile with sample data
                    var profile = new CandidateProfile
                    {
                        UserId = user.Id,
                        Summary = "Experienced software developer with a focus on backend systems.",
                        Experience = "Worked on multiple enterprise projects using .NET and cloud.",
                        Education = "B.Sc. in Computer Science",
                    };

                    context.CandidateProfiles.Add(profile);
                    await context.SaveChangesAsync();

                    // add sample projects
                    var p1 = new Project
                    {
                        CandidateProfileId = profile.Id,
                        Name = "Inventory System",
                        StartDate = DateTime.UtcNow.AddYears(-2),
                        EndDate = DateTime.UtcNow.AddYears(-1),
                        Description = "Developed inventory management system.",
                        TechnologyTags = "C#, ASP.NET, SQL",
                    };
                    var p2 = new Project
                    {
                        CandidateProfileId = profile.Id,
                        Name = "E-commerce Frontend",
                        StartDate = DateTime.UtcNow.AddYears(-1),
                        EndDate = null,
                        Description = "React + TypeScript storefront.",
                        TechnologyTags = "React, TypeScript, Tailwind",
                    };
                    context.Projects.AddRange(p1, p2);

                    // award an achievement
                    profile.Achievements.Add(new UserAchievement
                    {
                        UserId = user.Id,
                        Name = "Starter",
                        Description = "Seeded account achievement",
                        Icon = "🎖"
                    });

                    await context.SaveChangesAsync();
                }
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
                    },
                    new Position
                    {
                        Title = "Business Analyst",
                        Company = "Global Finance",
                        Level = "Mid",
                        Description = "Join our dynamic team to analyze business processes.",
                        ProjectTags = "Requirements, UML, Power BI",
                        MaxProjects = 4,
                    },
                    new Position
                    {
                        Title = "React Frontend Developer",
                        Company = "InnovateX",
                        Level = "Mid",
                        Description = "Build modern web applications with React and TypeScript.",
                        ProjectTags = "React, TypeScript, Tailwind",
                        MaxProjects = 6,
                    },
                    new Position
                    {
                        Title = "QA Engineer",
                        Company = "QualitySoft",
                        Level = "Junior",
                        Description = "Manual and automated testing for enterprise products.",
                        ProjectTags = "Selenium, Postman, Jira",
                        MaxProjects = 3,
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
