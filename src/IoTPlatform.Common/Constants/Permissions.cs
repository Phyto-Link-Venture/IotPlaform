namespace IoTPlatform.Common.Constants;

/// <summary>
/// Permission codes follow the convention "{Controller}.{actionMethod}" and map directly to
/// controller action methods (not a fixed CRUD enum). Each constant is referenced by the
/// <c>[HasPermission(...)]</c> attribute on the matching action and checked by the dynamic
/// policy provider. To add a new permission you add a method, decorate it, register the code
/// here, and seed a matching <c>permissions</c> row — no enum changes required.
/// </summary>
public static class Permissions
{
    /// <summary>DevicesController.</summary>
    public static class Devices
    {
        public const string List = "Devices.list";
        public const string Get = "Devices.get";
        public const string Create = "Devices.create";
        public const string Update = "Devices.update";
        public const string Delete = "Devices.delete";
        public const string SendCommand = "Devices.sendCommand";
    }

    /// <summary>UsersController.</summary>
    public static class Users
    {
        public const string List = "Users.list";
        public const string CreateUser = "Users.createUser";
        public const string CreateSuperAdmin = "Users.createSuperAdmin";
        public const string UpdateUser = "Users.updateUser";
        public const string DeleteUser = "Users.deleteUser";
    }

    /// <summary>DashboardController.</summary>
    public static class Dashboard
    {
        public const string View = "Dashboard.view";
    }

    /// <summary>AiController.</summary>
    public static class Ai
    {
        public const string Chat = "Ai.chat";
    }
}
