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
                row.RelativeItem().Text("Curriculum Vitae")
                    .FontSize(32).Bold().FontColor(Colors.Blue.Darken2);
            });
        }

        private void Content(IContainer container, CV cv, Dictionary<int, string> attributeValues)
        {
            container.Column(column =>
            {
                // Candidate Info
                column.Item().PaddingBottom(20).Text(cv.User?.FullName ?? "Candidate")
                    .FontSize(24).Bold().AlignCenter();

                column.Item().Text(cv.User?.Email ?? "").AlignCenter().FontSize(12);

                column.Item().PaddingTop(10).Text($"Applied for: {cv.Position?.Title}")
                    .FontSize(16).Bold().AlignCenter().FontColor(Colors.Grey.Darken2);

                // Professional Summary
                if (!string.IsNullOrEmpty(cv.CandidateProfile?.Summary))
                {
                    column.Item().PaddingTop(25).Text("Professional Summary").FontSize(14).Bold();
                    column.Item().LineHorizontal(1);

                    var summaryLines = cv.CandidateProfile.Summary
                        .Replace("\r\n", "\n").Split('\n');

                    foreach (var line in summaryLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            column.Item().Text(line.Trim()).FontSize(11);
                    }
                }

                // Attributes
                column.Item().PaddingTop(25).Text("Key Qualifications").FontSize(14).Bold();
                column.Item().LineHorizontal(1);

                foreach (var pa in cv.Position?.PositionAttributes.OrderBy(x => x.Order)
                         ?? Enumerable.Empty<PositionAttribute>())
                {
                    var attr = pa.AttributeDefinition;
                    var value = attributeValues.GetValueOrDefault(attr.Id, "");

                    bool isEmpty = string.IsNullOrWhiteSpace(value);

                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem(4).Text(attr.Name + ":").Bold();
                        row.RelativeItem(6).Text(isEmpty ? "NOT PROVIDED" : value)
                            .FontColor(isEmpty ? Colors.Red.Medium : Colors.Black);
                    });
                }

                // Projects
                if (cv.CandidateProfile?.Projects.Any() == true)
                {
                    column.Item().PaddingTop(30).Text("Selected Projects").FontSize(14).Bold();
                    column.Item().LineHorizontal(1);

                    var projects = cv.CandidateProfile.Projects
                        .OrderByDescending(p => p.StartDate)
                        .Take(cv.Position?.MaxProjects ?? 3);

                    foreach (var p in projects)
                    {
                        column.Item().PaddingTop(12).Text(p.Name).FontSize(12).Bold();
                        column.Item().Text($"{p.StartDate:MMM yyyy} - {(p.EndDate?.ToString("MMM yyyy") ?? "Present")}");

                        if (!string.IsNullOrEmpty(p.Description))
                        {
                            var descLines = p.Description.Replace("\r\n", "\n").Split('\n');
                            foreach (var line in descLines.Take(3))
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                    column.Item().Text("• " + line.Trim());
                            }
                        }
                    }
                }
            });
        }

        private void Footer(IContainer container)
        {
            container.AlignCenter().Text($"Generated on {DateTime.UtcNow:dd MMMM yyyy} | TalentHub")
                .FontSize(10).FontColor(Colors.Grey.Medium);
        }
    }
}