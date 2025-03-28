// WidowedOrphansManagement.Models/ParentOrphanDto.cs
namespace WidowedOrphansManagement.Models
{
    public class ParentOrphanDto
    {
        public int Id { get; set; } // הוספת מזהה מסד הנתונים
        public bool IsParent { get; set; }
        public string IdentityNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Status { get; set; } // שם הסטטוס
        public int? StatusId { get; set; } // ID של הסטטוס (יכול להיות שימושי בעריכה)
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string Floor { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public List<ParentOrphanDto>? Children { get; set; } // ה ? הופך את זה ל-nullable

        // הוספנו בנאי ריק למקרה הצורך
        public ParentOrphanDto()
        {
            Children = new List<ParentOrphanDto>();
        }
    }
}