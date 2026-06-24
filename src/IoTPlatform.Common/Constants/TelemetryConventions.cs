using System.Text.RegularExpressions;

namespace IoTPlatform.Common.Constants;

/// <summary>
/// Conventions for device telemetry. Telemetry posts directly at the device level
/// (the sensor layer is intentionally hidden); each reading is identified by a
/// <c>dataname_symbol</c> string, e.g. <c>temperature_C</c>, <c>humidity_percent</c>,
/// <c>pressure_kPa</c>.
/// </summary>
public static partial class TelemetryConventions
{
    /// <summary>
    /// A telemetry data point name: a lowercase data name, an underscore separator,
    /// then a unit symbol. Examples: <c>temperature_C</c>, <c>humidity_percent</c>,
    /// <c>battery_V</c>, <c>flow_Lpm</c>.
    /// </summary>
    public const string DataNameSymbolPattern = "^[a-z][a-z0-9]*_[A-Za-z0-9%]+$";

    public const int DataNameSymbolMaxLength = 128;

    [GeneratedRegex(DataNameSymbolPattern, RegexOptions.CultureInvariant)]
    public static partial Regex DataNameSymbolRegex();

    /// <summary>Returns true when <paramref name="value"/> matches the dataname_symbol convention.</summary>
    public static bool IsValidDataNameSymbol(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= DataNameSymbolMaxLength
        && DataNameSymbolRegex().IsMatch(value);
}
