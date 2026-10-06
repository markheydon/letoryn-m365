using Microsoft.FluentUI.AspNetCore.Components;

namespace TenancyHub.Web.Components.Layout;

/// <summary>Fluent token styles for controls rendered on the brand-coloured app header.</summary>
internal static class ShellHeaderStyles
{
    public const string OnBrandForeground = $"color: {SystemColors.Neutral.ForegroundOnBrand};";

    public const string OnBrandOutlineButton =
        $"{OnBrandForeground} border-color: {StylesVariables.Colors.Neutral.StrokeOnBrand2};";

    public const string OnBrandTransparentButton = OnBrandForeground;
}
