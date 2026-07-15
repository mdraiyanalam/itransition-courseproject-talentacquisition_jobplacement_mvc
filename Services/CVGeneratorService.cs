using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using talentacquisition_jobplacement_mvc.Helpers;
using talentacquisition_jobplacement_mvc.Models;
using System.Text.Json;
using QRCoder;

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
                        .FontSize(32).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().Text("TalentHub Recruitment Platform")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void Content(IContainer container, CV cv, Dictionary<int, string> attributeValues)
        {
            container.Column(column =>
            {
                // Candidate Info
                column.Item().PaddingBottom(20).Text(cv.User?.FullName ?? "Candidate")
                    .FontSize(26).Bold().AlignCenter();

                column.Item().Text(cv.User?.Email ?? "").AlignCenter().FontSize(12);

                if (!string.IsNullOrEmpty(cv.Position?.Title))
                    column.Item().PaddingTop(8).Text($"Applying for: {cv.Position.Title}")
                        .FontSize(16).Bold().AlignCenter().FontColor(Colors.Blue.Darken1);

                // Professional Summary
                if (!string.IsNullOrEmpty(cv.CandidateProfile?.Summary))
                {
                    column.Item().PaddingTop(30).Text("PROFESSIONAL SUMMARY").FontSize(14).Bold();
                    column.Item().LineHorizontal(1);
                    column.Item().Text(cv.CandidateProfile.Summary)
                        .FontSize(11).LineHeight(1.4f);
                }

                // Key Qualifications
                column.Item().PaddingTop(25).Text("KEY QUALIFICATIONS").FontSize(14).Bold();
                column.Item().LineHorizontal(1);

                foreach (var pa in cv.Position?.PositionAttributes.OrderBy(x => x.Order) ?? Enumerable.Empty<PositionAttribute>())
                {
                    var attr = pa.AttributeDefinition;
                    var value = attributeValues.GetValueOrDefault(attr.Id, "");
                    bool isEmpty = string.IsNullOrWhiteSpace(value);

                    column.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem(4).Text(attr.Name + ":").Bold().FontSize(11);
                        row.RelativeItem(6).Text(isEmpty ? "NOT PROVIDED" : value)
                            .FontColor(isEmpty ? Colors.Red.Medium : Colors.Black)
                            .FontSize(11);
                    });
                }

                // Projects
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
                        column.Item().PaddingTop(12).Text(p.Name).FontSize(13).Bold();
                        column.Item().Text($"{p.StartDate:MMM yyyy} - {(p.EndDate?.ToString("MMM yyyy") ?? "Present")}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        if (!string.IsNullOrEmpty(p.Description))
                            column.Item().Text(p.Description).FontSize(11).LineHeight(1.3f);
                    }
                }

                // QR Code
                column.Item().PaddingTop(30).Text("Scan to View Online").FontSize(10).AlignCenter();
                column.Item().PaddingTop(5).AlignCenter().Width(140).Image(GenerateQRCode($"https://yourapp.com/cvs/details/{cv.Id}"));
            });
        }

        private void Footer(IContainer container)
        {
            container.AlignCenter().Text($"Generated on {DateTime.UtcNow:dd MMMM yyyy} | TalentHub Recruitment Platform")
                .FontSize(9).FontColor(Colors.Grey.Medium);
        }

        private byte[] GenerateQRCode(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}