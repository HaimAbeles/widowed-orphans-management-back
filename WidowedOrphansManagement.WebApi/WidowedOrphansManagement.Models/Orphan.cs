namespace WidowedOrphansManagement.Models
{
    public class Orphan
    {
        public int Id { get; set; }
        public string ParentIdentityNumber { get; set; }  // תעודת זהות הורה
        public string IdentityNumber { get; set; }
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

        public Parent Parent { get; set; }  // קשר עם ההורה
    }
}
