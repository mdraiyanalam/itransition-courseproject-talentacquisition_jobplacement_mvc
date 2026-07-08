using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using talentacquisition_jobplacement_mvc.Models;
using System.Text.Json;

namespace talentacquisition_jobplacement_mvc.Services
{
    public class CVGeneratorService
    {
        public byte[] GenerateCV(CV cv)
        {
            var attributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("Curriculum Vitae").FontSize(24).Bold().AlignCenter();

                    page.Content().Column(column =>
                    {
                        // Personal Info
                        column.Item().Text(cv.User.FullName).FontSize(20).Bold();
                        column.Item().Text(cv.User.Email).FontSize(12);

                        // Position Applied
                        column.Item().PaddingTop(20).Text($"Position: {cv.Position.Title}").FontSize(16).Bold();

                        // Attributes
                        column.Item().PaddingTop(15).Text("Key Information").FontSize(14).Bold();

                        foreach (var pa in cv.Position.PositionAttributes.OrderBy(x => x.Order))
                        {
                            var attr = pa.AttributeDefinition;
                            var value = attributeValues.GetValueOrDefault(attr.Id, "Not Provided");

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(attr.Name + ":").Bold();
                                row.RelativeItem().Text(value);
                            });
                        }

                        // Summary
                        if (!string.IsNullOrEmpty(cv.CandidateProfile?.Summary))
                        {
                            column.Item().PaddingTop(20).Text("Professional Summary").FontSize(14).Bold();
                            column.Item().Text(cv.CandidateProfile.Summary);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated on ").FontSize(10);
                        text.Span(DateTime.UtcNow.ToString("dd MMM yyyy")).FontSize(10);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}