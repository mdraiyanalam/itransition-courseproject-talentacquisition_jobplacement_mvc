using System.Text.Json;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Helpers
{
    public static class AccessRuleEvaluator
    {
        public static bool CanApply(Position position, CandidateProfile profile, Dictionary<int, string> submittedValues)
        {
            if (string.IsNullOrEmpty(position.AccessRules))
                return true; // No rules = open to all

            try
            {
                var rules = JsonSerializer.Deserialize<List<AccessRule>>(position.AccessRules);
                if (rules == null || rules.Count == 0) return true;

                foreach (var rule in rules)
                {
                    if (!EvaluateRule(rule, profile, submittedValues))
                        return false;
                }
                return true;
            }
            catch
            {
                return true; // Fail open if rules are invalid
            }
        }

        private static bool EvaluateRule(AccessRule rule, CandidateProfile profile, Dictionary<int, string> submittedValues)
        {
            string value = submittedValues.TryGetValue(rule.AttributeId, out var v)
                ? v : GetProfileValue(profile, rule.AttributeId);

            return rule.Operator switch
            {
                ">" => double.TryParse(value, out var num) && num > double.Parse(rule.Value),
                "<" => double.TryParse(value, out var num) && num < double.Parse(rule.Value),
                "=" => value.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
                "!=" => !value.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private static string GetProfileValue(CandidateProfile profile, int attributeId)
        {
            // Extend later with CandidateProfileAttribute
            return "";
        }
    }
}