using System.Windows.Controls;

public class QOL
{
    public static string? GetStringFromComboBox(ComboBox target)
    {
        return target.SelectedItem?.ToString()?.Split(": ") == null ? null : 
        (target.SelectedItem?.ToString()?.Split(": ").Length > 1 ? target.SelectedItem?.ToString()?.Split(": ")[1] : null);
    }
    public static bool CheckSyntax(TextBox target)
    {
        if (string.IsNullOrEmpty(target.Text)) {return false;}
        if (target.Text.Length <= 1) {return false;}

        char last = target.Text.ToUpper()[^1];

        if (last == 'M' || last == 'K')
        {
            return target.Text[..^1].All(char.IsDigit);
        }

        return target.Text.All(char.IsDigit);
    }
    public static double? StringToDouble(string target)
    {
        if (double.TryParse(target, out double result))
        {
            return result;
        }
        return null; //
    }
}