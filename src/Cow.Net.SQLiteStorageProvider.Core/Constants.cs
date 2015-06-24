namespace Cow.Net.SQLiteStorageProvider.Core
{
    public class Constants
    {
        public const string UsersTable          = "users";
        public const string ProjectsTable       = "projects";
        public const string SocketserversTable  = "socketservers";
        public const string ItemsTable          = "items";
        public const string GroupsTable         = "groups";

        public static string[] GetStores()
        {
            return new[] { UsersTable, ProjectsTable, SocketserversTable, ItemsTable, GroupsTable };
        }
    }
}
