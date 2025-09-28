namespace GearHawk.Core.Data
{
    public sealed class ConnectionUtils
    {
        private const string ENV_VAR_NAME = "ConnectionStrings__GearHawk";
        private const string DEFAULT_CONN_STRING = "";

        /// <summary>
        /// Retrieve a connection string to the current database using environment variable
        /// injected by the host or app configuration. Falls back to empty string by default.
        /// </summary>
        /// <returns>connection string to the current sql database</returns>
        public static string GetConnString()
        {
            var fromEnv = System.Environment.GetEnvironmentVariable(ENV_VAR_NAME);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv!;

            return DEFAULT_CONN_STRING;
        }
    }
}
