using System.Text.Json;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class CV
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position? Position { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int CandidateProfileId { get; set; }
        public CandidateProfile? CandidateProfile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string AttributeValues { get; set; } = "{}";
        public bool IsPublished { get; set; } = false;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CVLike> Likes { get; set; } = new List<CVLike>();

        /// <summary>
        /// Validates that all required attributes have values.
        /// Returns a tuple (isValid, missingFields)
        /// </summary>
        public (bool isValid, List<string> missingFields) ValidateForPublishing()
        {
            var missingFields = new List<string>();

            // Check if user full name is filled
            if (string.IsNullOrWhiteSpace(User?.FullName))
                missingFields.Add("Full Name");

            // Parse attribute values
            var attributeValues = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(AttributeValues))
                {
                    attributeValues = JsonSerializer.Deserialize<Dictionary<string, string>>(AttributeValues) 
                        ?? new Dictionary<string, string>();
                }
            }
            catch
            {
                // If parsing fails, treat as invalid
                missingFields.Add("Attribute data is corrupted");
                return (false, missingFields);
            }

            // Check all required position attributes have values
            if (Position?.PositionAttributes != null)
            {
                foreach (var posAttr in Position.PositionAttributes)
                {
                    if (posAttr.AttributeDefinition?.IsRequired == true)
                    {
                        var attrKey = posAttr.AttributeDefinitionId.ToString();
                        if (!attributeValues.ContainsKey(attrKey) || 
                            string.IsNullOrWhiteSpace(attributeValues[attrKey]))
                        {
                            missingFields.Add($"{posAttr.AttributeDefinition?.Name} (required)");
                        }
                    }
                }
            }

            return (missingFields.Count == 0, missingFields);
        }

        /// <summary>
        /// Get missing field count for display
        /// </summary>
        public int GetMissingFieldCount()
        {
            var (isValid, missingFields) = ValidateForPublishing();
            return missingFields.Count;
        }

        /// <summary>
        /// Check if CV is complete (all required fields filled)
        /// </summary>
        public bool IsComplete()
        {
            var (isValid, _) = ValidateForPublishing();
            return isValid;
        }
    }
}