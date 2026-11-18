using System.Windows.Controls;

public class QOL
{
    public static string? GetStringFromComboBox(ComboBox target)
    {
        return target.SelectedItem?.ToString()?.Split(": ") == null ? null : 
        (target.SelectedItem?.ToString()?.Split(": ").Length > 1 ? target.SelectedItem?.ToString()?.Split(": ")[1] : null);
    }
}