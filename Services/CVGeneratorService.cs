using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using talentacquisition_jobplacement_mvc.Helpers;
using talentacquisition_jobplacement_mvc.Models;
using System.Text.Json;

namespace talentacquisition_jobplacement_mvc.Services
{
    public class CVGeneratorService
    {
        public byte[] GenerateCV(CV cv)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;

            var attributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues)
                  ?? new Dictionary<int, string>();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    page.Header().Element(Header);
                    page.Content().Element(c => Content(c, cv, attributeValues));
                    page.Footer().Element(Footer);
                });
            }).GeneratePdf();
        }

        private void Header(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Curriculum Vitae")
                        .FontSize(28).Bold().FontColor(Colors.Blue.Darken2);
                });
            });
        }

        private void Content(IContainer container, CV cv, Dictionary<int, string> attributeValues)
        {
            container.Column(column =>
            {
                // Header Info
                column.Item().PaddingBottom(20).Text(cv.User?.FullName ?? "Candidate")
                    .FontSize(28).Bold().AlignCenter();

                column.Item().Text(cv.User?.Email ?? "").AlignCenter().FontSize(12);

                if (!string.IsNullOrEmpty(cv.Position?.Title))
                    column.Item().PaddingTop(10).Text($"Applying for: {cv.Position.Title}")
                        .FontSize(16).Bold().AlignCenter();

                // Professional Summary
                if (!string.IsNullOrEmpty(cv.CandidateProfile?.Summary))
                {
                    column.Item().PaddingTop(25).Text("PROFESSIONAL SUMMARY").FontSize(14).Bold();
                    column.Item().LineHorizontal(1);
                    column.Item().Text(cv.CandidateProfile.Summary).FontSize(11).Leading(1.3f);
                }

                // Key Qualifications / Attributes (with red highlight)
                column.Item().PaddingTop(25).Text("KEY QUALIFICATIONS").FontSize(14).Bold();
                column.Item().LineHorizontal(1);

                foreach (var pa in cv.Position?.PositionAttributes.OrderBy(x => x.Order) ?? Enumerable.Empty<PositionAttribute>())
                {
                    var attr = pa.AttributeDefinition;
                    var value = attributeValues.GetValueOrDefault(attr.Id, "");
                    bool isEmpty = string.IsNullOrWhiteSpace(value);

                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem(4).Text(attr.Name + ":").Bold().FontSize(11);
                        row.RelativeItem(6).Text(isEmpty ? "NOT PROVIDED" : value)
                            .FontColor(isEmpty ? Colors.Red.Medium : Colors.Black)
                            .FontSize(11);
                    });
                }

                // Projects Section
                var projectsToShow = cv.CandidateProfile?.Projects
                    .OrderByDescending(p => p.StartDate)
                    .Take(cv.Position?.MaxProjects ?? 4)
                    .ToList();

                if (projectsToShow?.Any() == true)
                {
                    column.Item().PaddingTop(30).Text("SELECTED PROJECTS").FontSize(14).Bold();
                    column.Item().LineHorizontal(1);

                    foreach (var p in projectsToShow)
                    {
                        column.Item().PaddingTop(12).Text(p.Name).FontSize(12.5f).Bold();
                        column.Item().Text($"{p.StartDate:MMM yyyy} - {(p.EndDate?.ToString("MMM yyyy") ?? "Present")}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        if (!string.IsNullOrEmpty(p.Description))
                            column.Item().Text(p.Description).FontSize(11);
                    }
                }
            });
        }

        private void Footer(IContainer container)
        {
            container.AlignCenter().Text($"Generated on {DateTime.UtcNow:dd MMMM yyyy} | TalentHub Recruitment Platform")
                .FontSize(9).FontColor(Colors.Grey.Medium);
        }
    }
}