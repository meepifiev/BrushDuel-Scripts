using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace _Project.Infrastructure.Network
{
    public class MatchTicket
    {
        public string MatchId;
        public int Slot;
        public string PlayerId;
        public string Nickname;
        public string PaintingId;
        public int GameMode;
        public float RoundDurationSeconds;
        public long ExpiresUnix;
    }

    public static class MatchTicketCodec
    {
        public static string Encode(MatchTicket ticket, string secret)
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ticket));
            return Base64Url(payload) + "." + Base64Url(Sign(payload, secret));
        }

        public static bool TryDecode(string token, string secret, out MatchTicket ticket)
        {
            ticket = null;

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            int dot = token.IndexOf('.');

            if (dot <= 0 || dot >= token.Length - 1)
            {
                return false;
            }

            byte[] payload;
            byte[] signature;

            try
            {
                payload = FromBase64Url(token.Substring(0, dot));
                signature = FromBase64Url(token.Substring(dot + 1));
            }
            catch (FormatException)
            {
                return false;
            }

            if (FixedTimeEquals(signature, Sign(payload, secret)) == false)
            {
                return false;
            }

            try
            {
                ticket = JsonConvert.DeserializeObject<MatchTicket>(Encoding.UTF8.GetString(payload));
            }
            catch (JsonException)
            {
                return false;
            }

            return ticket != null;
        }

        private static byte[] Sign(byte[] payload, string secret)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return hmac.ComputeHash(payload);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;

            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }

        private static string Base64Url(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] FromBase64Url(string value)
        {
            string padded = value.Replace('-', '+').Replace('_', '/');

            switch (padded.Length % 4)
            {
                case 2:
                    padded += "==";
                    break;
                case 3:
                    padded += "=";
                    break;
            }

            return Convert.FromBase64String(padded);
        }
    }
}
