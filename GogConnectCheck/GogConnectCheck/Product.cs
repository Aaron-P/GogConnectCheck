using System;
using System.ComponentModel.DataAnnotations;

namespace GogConnectCheck
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Timestamp { get; set; }

        internal DateTime Expiration
        {
            get
            {
                var result = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                result = result.AddSeconds(Timestamp).ToLocalTime();
                return result;
            }
        }
    }
}
