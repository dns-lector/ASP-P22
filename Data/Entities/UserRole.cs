namespace ASP_P22.Data.Entities
{
    public class UserRole
    {
        public Guid   Id          { get; set; }
        public String Name        { get; set; }
        public String Description { get; set; }
        public bool   CanCreate   { get; set; }  // Права доступу
        public bool   CanRead     { get; set; }  // до інформації
        public bool   CanUpdate   { get; set; }  // з обмеженим доступом
        public bool   CanDelete   { get; set; }  // (ІзОД)
        public bool   IsEmployee  { get; set; }

    }
}
