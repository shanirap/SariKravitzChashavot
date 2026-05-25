namespace AccountingProject.Infrastructure
{
    /// <summary>Strength rules applied when admins create accounts or rotate passwords.</summary>
    public static class PasswordRules
    {
        public const int MinimumLength = 12;

        /// <summary>Returns English error message, or null when valid.</summary>
        public static string? ValidationError(string? password)
        {
            if (password == null || string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Trim().Length != password.Length)
                return "Password must not contain leading or trailing whitespace.";

            if (password.Length < MinimumLength)
                return $"Password must be at least {MinimumLength} characters.";

            var hasUpper = false;
            var hasLower = false;
            var hasDigit = false;
            var hasSpecial = false;
            foreach (var ch in password)
            {
                if (char.IsUpper(ch)) hasUpper = true;
                else if (char.IsLower(ch)) hasLower = true;
                else if (char.IsDigit(ch)) hasDigit = true;
                else if (!char.IsWhiteSpace(ch)) hasSpecial = true;
            }

            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                return "Password must include at least one uppercase letter, one lowercase letter, one digit, and one non-alphanumeric symbol.";
            }

            return null;
        }
    }
}
