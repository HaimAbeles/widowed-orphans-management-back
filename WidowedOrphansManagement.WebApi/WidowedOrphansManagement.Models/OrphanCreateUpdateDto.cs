using System;
using System.ComponentModel.DataAnnotations;

namespace WidowedOrphansManagement.Models
{
    public class OrphanCreateUpdateDto
    {
        [Required(ErrorMessage = "תעודת זהות הורה היא שדה חובה.")]
        //[RegularExpression(@"^\d{9}$", ErrorMessage = "תעודת זהות הורה חייבת להכיל 9 ספרות.")]
        public string ParentIdentityNumber { get; set; } // ת.ז. של ההורה המשויך

        [Required(ErrorMessage = "תעודת זהות יתום היא שדה חובה.")]
        //[RegularExpression(@"^\d{9}$", ErrorMessage = "תעודת זהות יתום חייבת להכיל 9 ספרות.")]
        public string IdentityNumber { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "תאריך לידה הוא שדה חובה.")]
        public DateTime BirthDate { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "סטטוס הוא שדה חובה.")]
        public int StatusId { get; set; } // ID של הסטטוס

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [MaxLength(100)]
        public string? Street { get; set; }

        [MaxLength(10)]
        public string? HouseNumber { get; set; }

        [MaxLength(10)]
        public string? Floor { get; set; }

        //[Phone(ErrorMessage = "מספר טלפון בית לא תקין.")]
        [MaxLength(20)]
        public string? HomePhone { get; set; }

        //[Phone(ErrorMessage = "מספר טלפון נייד לא תקין.")]
        [MaxLength(20)]
        public string? MobilePhone { get; set; }

        //[Phone(ErrorMessage = "מספר טלפון עבודה לא תקין.")]
        [MaxLength(20)]
        public string? WorkPhone { get; set; }

        //[EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Notes { get; set; }

        // שימו לב: ה-DTO *לא* כולל את Id, CreatedRow, UpdatedRow, Status, Parent
    }
}