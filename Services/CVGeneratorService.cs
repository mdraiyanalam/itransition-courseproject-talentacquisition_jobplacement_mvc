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
                    page.Margin(30);
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
                row.RelativeItem().Text("Curriculum Vitae")
                    .FontSize(28).Bold().FontColor(Colors.Blue.Darken2);
            });
        }

        private void Content(IContainer container, CV cv, Dictionary<int, string> attributeValues)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(20).Text(cv.User?.FullName ?? "Candidate")
                    .FontSize(22).Bold().AlignCenter();

                column.Item().Text(cv.User?.Email ?? "").AlignCenter().FontSize(12);

                column.Item().PaddingTop(25).Text($"Position: {cv.Position?.Title}")
                    .FontSize(16).Bold().AlignCenter();

                // Attributes
                column.Item().PaddingTop(30).Text("Key Qualifications").FontSize(14).Bold();
                column.Item().LineHorizontal(1);

                foreach (var pa in cv.Position?.PositionAttributes.OrderBy(x => x.Order)
                         ?? Enumerable.Empty<PositionAttribute>())
                {
                    var attr = pa.AttributeDefinition;
                    var value = attributeValues.GetValueOrDefault(attr.Id, "Not Provided");
                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem(4).Text(attr.Name + ":").Bold();
                        row.RelativeItem(6).Text(value);
                    });
                }

                // Summary with Markdown support - ALTERNATE SAFE APPROACH
                if (!string.IsNullOrEmpty(cv.CandidateProfile?.Summary))
                {
                    column.Item().PaddingTop(30).Text("Professional Summary").FontSize(14).Bold();

                    // QuestPDF doesn't support full HTML/Markdown easily.
                    // We use plain text with line breaks for now (safe fallback)
                    var summaryLines = cv.CandidateProfile.Summary
                        .Replace("\r\n", "\n")
                        .Split('\n');

                    foreach (var line in summaryLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            column.Item().Text(line.Trim());
                        }
                        else
                        {
                            column.Item().Text(""); // Empty line
                        }
                    }
                }

                // Projects
                if (cv.CandidateProfile?.Projects.Any() == true)
                {
                    column.Item().PaddingTop(30).Text("Projects").FontSize(14).Bold();
                    foreach (var p in cv.CandidateProfile.Projects)
                    {
                        column.Item().PaddingTop(10).Text(p.Name).FontSize(12).Bold();
                        column.Item().Text($"{p.StartDate:MMM yyyy} - {(p.EndDate?.ToString("MMM yyyy") ?? "Present")}");
                        column.Item().Text(p.Description);
                    }
                }
            });
        }

        private void Footer(IContainer container)
        {
            container.AlignCenter().Text($"Generated on {DateTime.UtcNow:dd MMM yyyy}")
                .FontSize(10).FontColor(Colors.Grey.Medium);
        }
    }
}