using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using WidowedOrphansManagement.Data.Contexts;
using WidowedOrphansManagement.Models;

namespace WidowedOrphansManagement.Services.ExcelService
{
    public class ExcelService : IExcelService
    {
        private readonly WidowedOrphansContext _context;
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(WidowedOrphansContext context, ILogger<ExcelService> logger)
        {
            _context = context;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<string> ProcessParentsExcelFileAsync(IFormFile file)
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                if (file == null || file.Length <= 0)
                {
                    errorMessages.AppendLine("שגיאה: לא סופק קובץ או שהקובץ ריק.");
                    return errorMessages.ToString();
                }

                // טוענים את כל הסטטוסים (כדי לבדוק אם הסטטוס קיים)
                var validStatuses = await _context.Statuses.ToListAsync();

                // שמירה של תעודות הזהות שבקובץ כדי לבדוק כפילויות
                var allNationalIdsInFile = new HashSet<string>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            errorMessages.AppendLine("שגיאה: לא נמצא גיליון עבודה.");
                            return errorMessages.ToString();
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string nationalId = worksheet.Cells[row, 1].Text.Trim(); // IdentityNumber
                            string lastName = worksheet.Cells[row, 2].Text.Trim();
                            string firstName = worksheet.Cells[row, 3].Text.Trim();
                            string birthDateText = worksheet.Cells[row, 4].Text.Trim();
                            string gender = worksheet.Cells[row, 5].Text.Trim();
                            string statusName = worksheet.Cells[row, 6].Text.Trim();
                            string neighborhood = worksheet.Cells[row, 7].Text.Trim();
                            string street = worksheet.Cells[row, 8].Text.Trim();
                            string houseNumber = worksheet.Cells[row, 9].Text.Trim();
                            string floor = worksheet.Cells[row, 10].Text.Trim();
                            string homePhone = worksheet.Cells[row, 11].Text.Trim();
                            string mobilePhone = worksheet.Cells[row, 12].Text.Trim();
                            string workPhone = worksheet.Cells[row, 13].Text.Trim();
                            string email = worksheet.Cells[row, 14].Text.Trim();
                            string notes = worksheet.Cells[row, 15].Text.Trim();

                            // הולידציה של תאריך הלידה, תעודת זהות וסטטוס
                            DateTime? birthDate = null;
                            if (!string.IsNullOrEmpty(birthDateText) && DateTime.TryParseExact(birthDateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                birthDate = parsedDate;
                            }
                            else
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: תאריך לידה לא תקין.");
                                continue;
                            }

                            // בדיקה אם הסטטוס קיים בטבלה
                            var status = validStatuses.FirstOrDefault(s => s.StatusName == statusName);
                            if (status == null)
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: סטטוס אישי לא קיים.");
                                continue;
                            }

                            // הוספת הורה אם תעודת הזהות לא קיימת
                            var existingParent = await _context.Parents.FirstOrDefaultAsync(p => p.IdentityNumber == nationalId);
                            if (existingParent != null)
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: תעודת זהות הורה כבר קיימת.");
                                continue;
                            }

                            // בדיקה אם תעודת הזהות כבר קיימת בקובץ (כפילות בתעודות הזהות בקובץ עצמו)
                            if (allNationalIdsInFile.Contains(nationalId))
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: תעודת זהות הורה כפולה בקובץ.");
                                continue;
                            }

                            allNationalIdsInFile.Add(nationalId);

                            Parent parent = new Parent
                            {
                                IdentityNumber = nationalId,
                                LastName = lastName,
                                FirstName = firstName,
                                BirthDate = birthDate.Value,
                                Gender = gender,
                                StatusId = status.Id,
                                Neighborhood = neighborhood,
                                Street = street,
                                HouseNumber = houseNumber,
                                Floor = floor,
                                HomePhone = homePhone,
                                MobilePhone = mobilePhone,
                                WorkPhone = workPhone,
                                Email = email,
                                Notes = notes,
                                CreatedRow = DateTime.Now,
                                UpdatedRow = DateTime.Now
                            };

                            _context.Parents.Add(parent);
                        }
                    }
                }

                // אם היו שגיאות יש להחזיר את השגיאות לפני שמירה
                if (errorMessages.Length > 0)
                {
                    return errorMessages.ToString();
                }

                await _context.SaveChangesAsync();
                return "הקובץ נטען בהצלחה";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProcessParentsExcelFileAsync, Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}");
                return "שגיאה בעיבוד הקובץ.";
            }
        }

        public async Task<string> ProcessOrphansExcelFileAsync(IFormFile file)
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                if (file == null || file.Length <= 0)
                {
                    errorMessages.AppendLine("שגיאה: לא סופק קובץ או שהקובץ ריק.");
                    return errorMessages.ToString();
                }

                // טוענים את כל הסטטוסים (כדי לבדוק אם הסטטוס קיים)
                var validStatuses = await _context.Statuses.ToListAsync();
                var allNationalIdsInFile = new HashSet<string>(); // למניעת כפילות תעודות זהות

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            errorMessages.AppendLine("שגיאה: לא נמצא גיליון עבודה.");
                            return errorMessages.ToString();
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string parentNationalId = worksheet.Cells[row, 1].Text.Trim();
                            string nationalId = worksheet.Cells[row, 2].Text.Trim();
                            string lastName = worksheet.Cells[row, 3].Text.Trim();
                            string firstName = worksheet.Cells[row, 4].Text.Trim();
                            string birthDateText = worksheet.Cells[row, 5].Text.Trim();
                            string gender = worksheet.Cells[row, 6].Text.Trim();
                            string statusName = worksheet.Cells[row, 7].Text.Trim();
                            string neighborhood = worksheet.Cells[row, 8].Text.Trim();
                            string street = worksheet.Cells[row, 9].Text.Trim();
                            string houseNumber = worksheet.Cells[row, 10].Text.Trim();
                            string floor = worksheet.Cells[row, 11].Text.Trim();
                            string homePhone = worksheet.Cells[row, 12].Text.Trim();
                            string mobilePhone = worksheet.Cells[row, 13].Text.Trim();
                            string workPhone = worksheet.Cells[row, 14].Text.Trim();
                            string email = worksheet.Cells[row, 15].Text.Trim();
                            string notes = worksheet.Cells[row, 16].Text.Trim();

                            // הולידציה של תאריך הלידה, תעודת זהות, סטטוס וסטטוס הורה
                            DateTime? birthDate = null;
                            if (!string.IsNullOrEmpty(birthDateText) && DateTime.TryParseExact(birthDateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                birthDate = parsedDate;
                            }
                            else
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: תאריך לידה לא תקין.");
                                continue;
                            }

                            // בדיקה אם הסטטוס קיים בטבלה
                            var status = validStatuses.FirstOrDefault(s => s.StatusName == statusName);
                            if (status == null)
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: סטטוס אישי לא קיים.");
                                continue;
                            }

                            // בדיקת קיום הורה
                            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.IdentityNumber == parentNationalId);
                            if (parent == null)
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: לא נמצא הורה עם תעודת זהות '{parentNationalId}'.");
                                continue;
                            }

                            // הוספת ילד אם תעודת הזהות לא קיימת
                            if (allNationalIdsInFile.Contains(nationalId))
                            {
                                errorMessages.AppendLine($"שורה {row} נכשלה, סיבת הכישלון: תעודת זהות ילד כפולה בקובץ.");
                                continue;
                            }

                            allNationalIdsInFile.Add(nationalId);

                            Orphan orphan = new Orphan
                            {
                                ParentIdentityNumber = parentNationalId,
                                IdentityNumber = nationalId,
                                LastName = lastName,
                                FirstName = firstName,
                                BirthDate = birthDate.Value,
                                Gender = gender,
                                StatusId = status.Id,
                                Neighborhood = neighborhood,
                                Street = street,
                                HouseNumber = houseNumber,
                                Floor = floor,
                                HomePhone = homePhone,
                                MobilePhone = mobilePhone,
                                WorkPhone = workPhone,
                                Email = email,
                                Notes = notes,
                                CreatedRow = DateTime.Now,
                                UpdatedRow = DateTime.Now
                            };

                            _context.Orphans.Add(orphan);
                        }
                    }
                }

                // אם היו שגיאות יש להחזיר את השגיאות לפני שמירה
                if (errorMessages.Length > 0)
                {
                    return errorMessages.ToString();
                }

                await _context.SaveChangesAsync();
                return "הקובץ נטען בהצלחה";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProcessOrphansExcelFileAsync, Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}");
                return "שגיאה בעיבוד הקובץ.";
            }
        }

        public byte[] GenerateParentsExampleExcel()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("דוגמת הורים");

                worksheet.Cells[1, 1].Value = "תעודת זהות";
                worksheet.Cells[1, 2].Value = "שם משפחה";
                worksheet.Cells[1, 3].Value = "שם פרטי";
                worksheet.Cells[1, 4].Value = "תאריך לידה";
                worksheet.Cells[1, 5].Value = "מגדר";
                worksheet.Cells[1, 6].Value = "סטטוס אישי";
                worksheet.Cells[1, 7].Value = "שכונה";
                worksheet.Cells[1, 8].Value = "רחוב";
                worksheet.Cells[1, 9].Value = "מספר בית";
                worksheet.Cells[1, 10].Value = "קומה";
                worksheet.Cells[1, 11].Value = "טלפון בית";
                worksheet.Cells[1, 12].Value = "טלפון נייד";
                worksheet.Cells[1, 13].Value = "טלפון עבודה";
                worksheet.Cells[1, 14].Value = "מייל";
                worksheet.Cells[1, 15].Value = "הערות";

                worksheet.Cells[2, 1].Value = "123456789";
                worksheet.Cells[2, 2].Value = "כהן";
                worksheet.Cells[2, 3].Value = "דוד";
                worksheet.Cells[2, 4].Value = "01/01/1970";
                worksheet.Cells[2, 5].Value = "זכר";
                worksheet.Cells[2, 6].Value = "אלמן";
                worksheet.Cells[2, 7].Value = "תל אביב";
                worksheet.Cells[2, 8].Value = "הרסל";
                worksheet.Cells[2, 9].Value = "12";
                worksheet.Cells[2, 10].Value = "2";
                worksheet.Cells[2, 11].Value = "123456789";
                worksheet.Cells[2, 12].Value = "987654321";
                worksheet.Cells[2, 13].Value = "123123123";
                worksheet.Cells[2, 14].Value = "david@mail.com";
                worksheet.Cells[2, 15].Value = "אין הערות";

                return package.GetAsByteArray();
            }
        }

        public byte[] GenerateOrphansExampleExcel()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("דוגמת ילדים");

                worksheet.Cells[1, 1].Value = "תעודת זהות הורה";
                worksheet.Cells[1, 2].Value = "תעודת זהות ילד";
                worksheet.Cells[1, 3].Value = "שם משפחה";
                worksheet.Cells[1, 4].Value = "שם פרטי";
                worksheet.Cells[1, 5].Value = "תאריך לידה";
                worksheet.Cells[1, 6].Value = "מגדר";
                worksheet.Cells[1, 7].Value = "סטטוס אישי";
                worksheet.Cells[1, 8].Value = "שכונה";
                worksheet.Cells[1, 9].Value = "רחוב";
                worksheet.Cells[1, 10].Value = "מספר בית";
                worksheet.Cells[1, 11].Value = "קומה";
                worksheet.Cells[1, 12].Value = "טלפון בית";
                worksheet.Cells[1, 13].Value = "טלפון נייד";
                worksheet.Cells[1, 14].Value = "טלפון עבודה";
                worksheet.Cells[1, 15].Value = "מייל";
                worksheet.Cells[1, 16].Value = "הערות";

                worksheet.Cells[2, 1].Value = "123456789";
                worksheet.Cells[2, 2].Value = "987654321";
                worksheet.Cells[2, 3].Value = "כהן";
                worksheet.Cells[2, 4].Value = "יצחק";
                worksheet.Cells[2, 5].Value = "15/06/2000";
                worksheet.Cells[2, 6].Value = "זכר";
                worksheet.Cells[2, 7].Value = "יתום";
                worksheet.Cells[2, 8].Value = "ירושלים";
                worksheet.Cells[2, 9].Value = "מלך דוד";
                worksheet.Cells[2, 10].Value = "5";
                worksheet.Cells[2, 11].Value = "3";
                worksheet.Cells[2, 12].Value = "123456789";
                worksheet.Cells[2, 13].Value = "987654321";
                worksheet.Cells[2, 14].Value = "123123123";
                worksheet.Cells[2, 15].Value = "yitzhak@mail.com";
                worksheet.Cells[2, 16].Value = "אין הערות";

                return package.GetAsByteArray();
            }
        }
    }
}
