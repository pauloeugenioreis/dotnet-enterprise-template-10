namespace BlazorApp.Components.UI;

public class DropdownOption
{
    public DropdownOption() { }
    public DropdownOption(string label, object? value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; set; } = "";
    public object? Value { get; set; }
}
