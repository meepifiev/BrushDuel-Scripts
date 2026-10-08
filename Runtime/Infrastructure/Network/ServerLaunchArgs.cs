using System;
using System.Collections.Generic;

namespace _Project.Infrastructure.Network
{
    public static class ServerLaunchArgs
    {
        private const string DevTicketSecret = "brushduel-dev-secret";

        public static ServerRole ResolveRole(ServerRole fallback)
        {
            string value = ReadValue("-role");

            if (string.IsNullOrEmpty(value))
            {
                return fallback;
            }

            if (value.Equals("hub", StringComparison.OrdinalIgnoreCase))
            {
                return ServerRole.Hub;
            }

            if (value.Equals("session", StringComparison.OrdinalIgnoreCase))
            {
                return ServerRole.Session;
            }

            return ServerRole.All;
        }

        public static bool TryResolvePort(out ushort port)
        {
            port = 0;
            string value = ReadValue("-port");

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return ushort.TryParse(value, out port);
        }

        public static string ResolveTicketSecret()
        {
            string value = Environment.GetEnvironmentVariable("BRUSHDUEL_TICKET_SECRET");
            return string.IsNullOrEmpty(value) ? DevTicketSecret : value;
        }

        public static List<string> ResolveSessions()
        {
            List<string> result = new List<string>();
            string raw = Environment.GetEnvironmentVariable("BRUSHDUEL_SESSIONS");

            if (string.IsNullOrEmpty(raw))
            {
                return result;
            }

            string[] entries = raw.Split(',');

            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i].Trim();

                if (entry.Length == 0)
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }

        private static string ReadValue(string key)
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
