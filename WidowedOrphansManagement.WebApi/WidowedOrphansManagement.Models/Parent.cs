namespace WidowedOrphansManagement.Models
{
    public class Parent
    {
        public int Id { get; set; }
        public string IdentityNumber { get; set; } // תעודת זהות של הורה
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public int StatusId { get; set; }  // קשר לסטטוס
        public Status Status { get; set; }
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string Floor { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedRow { get; set; }
        public DateTime UpdatedRow { get; set; }

        public ICollection<Orphan> Orphans { get; set; }  // קשר עם היתומים
    }
}
