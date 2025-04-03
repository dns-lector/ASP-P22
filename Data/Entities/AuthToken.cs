using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
    public class AuthToken
    {
        public Guid      Jti { get; set; }   // Token ID - унікальний ідентифікатор токену
        public String?   Iss { get; set; }   // Issuer - видавець токену
        public Guid?     Sub { get; set; }   // Subject - той, кому видається токен (UserId)
        public String?   Aud { get; set; }   // Audience - аудиторія, наприклад, ролі
        public DateTime  Iat { get; set; }   // Issued At - час видачі токену
        public DateTime  Exp { get; set; }   // Expiration - термін дії токену
        public DateTime? Nbf { get; set; }   // Not Before - токен не дійсний до цього часу

        [JsonIgnore]
        public UserAccess UserAccess { get; set; }
    }
}
