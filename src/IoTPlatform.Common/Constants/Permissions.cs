namespace IoTPlatform.Common.Constants;

/// <summary>
/// Permission codes follow the convention "{Module}.{Action}" and map to
/// authorization policies via the <c>HasPermission</c> attribute.
/// </summary>
public static class Permissions
{
    public static class Device
    {
        public const string View = "Device.View";
        public const string Create = "Device.Create";
        public const string Edit = "Device.Edit";
        public const string Delete = "Device.Delete";
        public const string Control = "Device.Control";
        public const string Export = "Device.Export";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Edit = "Settings.Edit";
    }
}
