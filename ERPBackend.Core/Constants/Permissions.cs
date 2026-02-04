namespace ERPBackend.Core.Constants
{
    public static class Permissions
    {
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }

        public static class PermissionsManagement
        {
            public const string View = "Permissions.Permissions.View";
            public const string Edit = "Permissions.Permissions.Edit";
        }

        public static List<string> GetAllPermissions()
        {
            return new List<string>
            {
                Users.View, Users.Create, Users.Edit, Users.Delete,
                Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
                PermissionsManagement.View, PermissionsManagement.Edit
            };
        }
    }
}
