namespace ASP_P22.Models
{
    public class JwtToken
    {
        public Guid     Jti   { get; set; } 
        public Guid?    Sub   { get; set; }  
        public DateTime Iat   { get; set; }  
        public DateTime Exp   { get; set; }
        public String   Name  { get; set; }
        public String   Email { get; set; }
        public String   Slug  { get; set; }
    }
}
/* Д.З. Реалізувати передачу у JWT даних про телефон та аватарку користувача.
 * Відобразити їх на фронтенді.
 */
