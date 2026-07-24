// Services/CVGeneratorService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System.Text.Json;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Services
{
    public class CVGeneratorService
    {
        // Generate CV PDF. baseUrl should be the application's base URL (e.g. https://itransition-courseproject-f3vg.onrender.com)
        // includePrivateNote: when true, include a note under the QR that the linked online profile is visible only to Recruiters and Administrators.
        public byte[] GenerateCV(CV cv, string baseUrl, bool includePrivateNote)
        {
            QuestPDF.Settings.License = LicenseType.Evaluation;

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
                    page.Content().Element(c => Content(c, cv, attributeValues, baseUrl, includePrivateNote));
                    page.Footer().Element(Footer);
                });
            }).GeneratePdf();
        }

        private void Header(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Curriculum Vitae")
                        .FontSize(28).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().Text("TalentHub Recruitment Platform")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void Content(IContainer container, CV cv, Dictionary<int, string> attributeValues, string baseUrl, bool includePrivateNote)
        {
            container.Column(column =>
            {
                // Personal Info
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

                    column.Item().PaddingTop(8).Row(row =>
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

                        if (!string.IsNullOrEmpty(p.TechnologyTags))
                            column.Item().Text($"Technologies: {p.TechnologyTags}").FontSize(10).FontColor(Colors.Blue.Medium);
                    }
                }

                // QR Code
                column.Item().PaddingTop(40).AlignCenter().Text("Scan to View Online Profile").FontSize(10);
                column.Item().PaddingTop(8).AlignCenter().Width(160)
                    .Image(GenerateQRCode(GetProfileUrl(baseUrl, cv)));

                // Conditional note about visibility of the online profile (only shown when the requester is a Recruiter or Administrator)
                if (includePrivateNote)
                {
                    column.Item().PaddingTop(8).AlignCenter().Text("Note: The online profile linked by this QR code is viewable only by Recruiters and Administrators on the platform.")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                }
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

        private string GetProfileUrl(string baseUrl, CV cv)
        {
            // Prefer candidate's user id if available, fall back to CV id route
            var userId = cv.CandidateProfile?.UserId ?? cv.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                // Use Admin/ViewProfile route so recruiters/admins can view the profile when authorized
                return $"{baseUrl.TrimEnd('/')}/Admin/ViewProfile/{userId}";
            }

            // fallback to CV details page
            return $"{baseUrl.TrimEnd('/')}/CVs/Details/{cv.Id}";
        }
    }
}