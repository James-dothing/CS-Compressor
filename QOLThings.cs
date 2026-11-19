using System.Windows.Controls;

public static class QOL
{
    public static string? GetStringFromComboBox(ComboBox? target)
    {
        if (target == null || target.SelectedItem == null) return null;

        if (target.SelectedItem is ComboBoxItem item)
        {
            return item.Content?.ToString()?.Trim();
        }

        var text = target.SelectedItem.ToString()?.Trim();
        if (string.IsNullOrEmpty(text)) return null;

        var split = text.Split(new[] { ": " }, StringSplitOptions.None);
        return split.Length > 1 ? split[1].Trim() : text;
    }
}
