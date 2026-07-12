using System.Text.Json;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Helpers
{
    public static class AccessRuleEvaluator
    {
        public static bool CanApply(Position position, CandidateProfile profile, Dictionary<int, string> submittedValues)
        {
            if (string.IsNullOrEmpty(position.AccessRules) || position.AccessRules == "[]")
                return true; // No rules = open to all

            try
            {
                var rules = JsonSerializer.Deserialize<List<AccessRule>>(position.AccessRules);
                if (rules == null || rules.Count == 0) return true;

                foreach (var rule in rules.OrderBy(r => r.Order))
                {
                    if (!EvaluateSingleRule(rule, profile, submittedValues))
                        return false;
                }
                return true;
            }
            catch
            {
                return true; // Fail open on error
            }
        }

        private static bool EvaluateSingleRule(AccessRule rule, CandidateProfile profile, Dictionary<int, string> submittedValues)
        {
            string? value = submittedValues.TryGetValue(rule.AttributeDefinitionId, out var submitted)
                ? submitted
                : GetProfileValue(profile, rule.AttributeDefinitionId);

            if (string.IsNullOrWhiteSpace(value))
                return false; // Missing value = cannot apply

            return rule.Operator switch
            {
                "=" => string.Equals(value, rule.Value, StringComparison.OrdinalIgnoreCase),
                "!=" => !string.Equals(value, rule.Value, StringComparison.OrdinalIgnoreCase),
                ">" => double.TryParse(value, out var v1) && double.TryParse(rule.Value, out var v2) && v1 > v2,
                "<" => double.TryParse(value, out var v1) && double.TryParse(rule.Value, out var v2) && v1 < v2,
                ">=" => double.TryParse(value, out var v1) && double.TryParse(rule.Value, out var v2) && v1 >= v2,
                "<=" => double.TryParse(value, out var v1) && double.TryParse(rule.Value, out var v2) && v1 <= v2,
                _ => true
            };
        }

        private static string? GetProfileValue(CandidateProfile profile, int attributeId)
        {
            var attr = profile.ProfileAttributes.FirstOrDefault(pa => pa.AttributeDefinitionId == attributeId);
            return attr?.Value;
        }
    }
}