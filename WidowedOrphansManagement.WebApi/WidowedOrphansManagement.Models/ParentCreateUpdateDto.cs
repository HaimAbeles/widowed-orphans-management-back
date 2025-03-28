using System;
using System.ComponentModel.DataAnnotations; // נוסיף ולידציות

namespace WidowedOrphansManagement.Models
{
    public class ParentCreateUpdateDto
    {
        [Required(ErrorMessage = "תעודת זהות היא שדה חובה.")]
        public string IdentityNumber { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "תאריך לידה הוא שדה חובה.")]
        public DateTime BirthDate { get; set; } // הקליינט ישלח ISO string, ASP.NET ימיר

        [MaxLength(10)]
        public string? Gender { get; set; } // nullable אם לא חובה

        [Required(ErrorMessage = "סטטוס הוא שדה חובה.")]
        public int StatusId { get; set; } // ID של הסטטוס שנבחר

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [MaxLength(100)]
        public string? Street { get; set; }

        [MaxLength(10)]
        public string? HouseNumber { get; set; }

        [MaxLength(10)]
        public string? Floor { get; set; }

        [MaxLength(20)]
        public string? HomePhone { get; set; }

        [MaxLength(20)]
        public string? MobilePhone { get; set; }

        [MaxLength(20)]
        public string? WorkPhone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Notes { get; set; }

        // שימו לב: ה-DTO *לא* כולל את Id, CreatedRow, UpdatedRow, Status, Orphans
    }
}