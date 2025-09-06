namespace GearHawk.Core.Data
{
    public sealed class ConnectionUtils
    {
        private static string CONN_STRING = @"Server=tcp:ia9479ho1u.database.windows.net,1433;Initial Catalog=GearHawk;Persist Security Info=False;User ID=gormank;Password=Offramp1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=230";

        /// <summary>
        /// The only access point to retrieve a connection string
        /// to the current database.
        /// </summary>
        /// <returns>connection string to the current sql database</returns>
        public static string GetConnString() => CONN_STRING;
    }
}
