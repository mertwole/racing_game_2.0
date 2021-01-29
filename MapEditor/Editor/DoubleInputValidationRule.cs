using System.Globalization;
using System.Windows.Controls;

namespace Editor
{
    public class DoubleInputValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object _value, CultureInfo cultureInfo)
        {
            string value = _value.ToString();

            bool valid = true;

            if (string.IsNullOrEmpty(value))
                valid = false;

            if (value.Contains(","))
                valid = false;

            CultureInfo culture = new CultureInfo("en-US", false);

            if (!double.TryParse(value, NumberStyles.Float, culture, out _))
                valid = false;

            return new ValidationResult(valid, valid ? null : "Please enter the number");
        }
    }
}
