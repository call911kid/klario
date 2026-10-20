using Microsoft.AspNetCore.Components;

namespace Klario.Console.Components.Common;

public partial class TruncatedTextPopover : ComponentBase
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string? Kicker { get; set; }

    [Parameter]
    public bool IsNearBottom { get; set; } = false;

    [Parameter]
    public string TextClass { get; set; } = string.Empty;

    [Parameter]
    public string PopoverWidthClass { get; set; } = "w-full min-w-[220px] max-w-sm";

    protected string PopoverPosition => IsNearBottom ? "bottom-full mb-1.5" : "top-full mt-1.5";
}
