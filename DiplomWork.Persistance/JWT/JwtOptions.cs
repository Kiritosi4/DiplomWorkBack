

namespace DiplomWork.Persistance.JWT
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string SecretKey { get; set; }
        public int ExpiresHours { get; set; }
    }
}
