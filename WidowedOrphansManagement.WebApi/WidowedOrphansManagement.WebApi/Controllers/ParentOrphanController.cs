using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // הוספת using ללוגר
using System; // הוספת using ל-Exception ו-DateTime
using System.Collections.Generic; // הוספת using ל-List
using System.Linq; // הוספת using ל-LINQ (AnyAsync, OrderBy, Select)
using System.Threading.Tasks; // הוספת using ל-Task
using WidowedOrphansManagement.Data.Contexts; // Namespace של Context
using WidowedOrphansManagement.Models; // Namespace של המודלים וה-DTOs

namespace WidowedOrphansManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentOrphanController : ControllerBase
    {
        private readonly WidowedOrphansContext _context;
        private readonly ILogger<ParentOrphanController> _logger;

        public ParentOrphanController(WidowedOrphansContext context, ILogger<ParentOrphanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ParentOrphan/get-parents-and-orphans
        [HttpGet("get-parents-and-orphans")]
        public async Task<IActionResult> GetParentsAndOrphans()
        {
            try
            {
                var parents = await _context.Parents
                    .AsNoTracking() // שיפור ביצועים אם רק קוראים נתונים
                    .Include(p => p.Status)
                    .Include(p => p.Orphans)
                        .ThenInclude(o => o.Status)
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Select(p => new ParentOrphanDto
                    {
                        Id = p.Id,
                        IsParent = true,
                        IdentityNumber = p.IdentityNumber,
                        LastName = p.LastName,
                        FirstName = p.FirstName,
                        BirthDate = p.BirthDate,
                        Gender = p.Gender,
                        Status = p.Status != null ? p.Status.StatusName : "N/A",
                        StatusId = p.StatusId,
                        Neighborhood = p.Neighborhood,
                        Street = p.Street,
                        HouseNumber = p.HouseNumber,
                        Floor = p.Floor,
                        HomePhone = p.HomePhone,
                        MobilePhone = p.MobilePhone,
                        WorkPhone = p.WorkPhone,
                        Email = p.Email,
                        Notes = p.Notes,
                        Children = p.Orphans != null ? p.Orphans
                            .OrderBy(o => o.BirthDate)
                            .Select(o => new ParentOrphanDto
                            {
                                Id = o.Id,
                                IsParent = false,
                                IdentityNumber = o.IdentityNumber,
                                LastName = o.LastName,
                                FirstName = o.FirstName,
                                BirthDate = o.BirthDate,
                                Gender = o.Gender,
                                Status = o.Status != null ? o.Status.StatusName : "N/A",
                                StatusId = o.StatusId,
                                Neighborhood = o.Neighborhood,
                                Street = o.Street,
                                HouseNumber = o.HouseNumber,
                                Floor = o.Floor,
                                HomePhone = o.HomePhone,
                                MobilePhone = o.MobilePhone,
                                WorkPhone = o.WorkPhone,
                                Email = o.Email,
                                Notes = o.Notes,
                                Children = null // לילד אין ילדים ב-DTO
                            }).ToList() : new List<ParentOrphanDto>()
                    }).ToListAsync();

                return Ok(parents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching parents and orphans.");
                return StatusCode(500, "שגיאה פנימית בשרת בעת הבאת הנתונים.");
            }
        }

        // GET: api/ParentOrphan/parent/{id}
        [HttpGet("parent/{id}")]
        public async Task<ActionResult<Parent>> GetParent(int id)
        {
            // כולל סטטוס, אבל לא כולל ילדים כברירת מחדל עבור GET יחיד
            var parent = await _context.Parents
                                     .AsNoTracking()
                                     .Include(p => p.Status)
                                     .FirstOrDefaultAsync(p => p.Id == id);
            if (parent == null)
            {
                return NotFound("הורה לא נמצא");
            }
            // מחזיר את ישות ה-Parent המלאה (ללא ילדים)
            // אם הקליינט צריך את ה-DTO, אפשר למפות כאן
            // return Ok(MapToDto(parent));
            return Ok(parent);
        }

        // GET: api/ParentOrphan/orphan/{id}
        [HttpGet("orphan/{id}")]
        public async Task<ActionResult<Orphan>> GetOrphan(int id)
        {
            var orphan = await _context.Orphans
                                    .AsNoTracking()
                                    .Include(o => o.Status)
                                    .FirstOrDefaultAsync(o => o.Id == id);
            if (orphan == null)
            {
                return NotFound("יתום לא נמצא");
            }
            // מחזיר את ישות ה-Orphan המלאה
            // return Ok(MapToDto(orphan));
            return Ok(orphan);
        }

        // POST: api/ParentOrphan/parent
        [HttpPost("parent")]
        public async Task<ActionResult<ParentOrphanDto>> CreateParent([FromBody] ParentCreateUpdateDto parentDto)
        {
            // --- ולידציה ראשונית של המודל ---
            if (!ModelState.IsValid)
            {
                // איסוף כל שגיאות ה-ModelState למחרוזת אחת
                string errors = string.Join("\n", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                return BadRequest(errors); // החזרת המחרוזת
            }

            // --- ולידציות לוגיות נוספות ---
            var validationErrors = new List<string>();
            if (await _context.Parents.AnyAsync(p => p.IdentityNumber == parentDto.IdentityNumber))
            {
                validationErrors.Add("הורה עם תעודת זהות זו כבר קיים.");
            }
            if (!await _context.Statuses.AnyAsync(s => s.Id == parentDto.StatusId))
            {
                validationErrors.Add("סטטוס לא תקין.");
            }

            // אם יש שגיאות לוגיות, החזר אותן כמחרוזת
            if (validationErrors.Any())
            {
                return BadRequest(string.Join("\n", validationErrors));
            }
            // --- סוף ולידציות ---

            // --- Map DTO to Entity ---
            var newParent = new Parent
            {
                IdentityNumber = parentDto.IdentityNumber,
                LastName = parentDto.LastName,
                FirstName = parentDto.FirstName,
                BirthDate = parentDto.BirthDate.Date, // Store only date part if time is irrelevant
                Gender = parentDto.Gender,
                StatusId = parentDto.StatusId,
                Neighborhood = parentDto.Neighborhood,
                Street = parentDto.Street,
                HouseNumber = parentDto.HouseNumber,
                Floor = parentDto.Floor,
                HomePhone = parentDto.HomePhone,
                MobilePhone = parentDto.MobilePhone,
                WorkPhone = parentDto.WorkPhone,
                Email = parentDto.Email,
                Notes = parentDto.Notes,
                CreatedRow = DateTime.UtcNow, // Use UTC for consistency
                UpdatedRow = DateTime.UtcNow
            };
            // --- End Mapping ---

            try
            {
                _context.Parents.Add(newParent);
                await _context.SaveChangesAsync();

                // Load the related Status for the response DTO
                await _context.Entry(newParent).Reference(p => p.Status).LoadAsync();

                // Map the created entity back to a DTO for the response
                var resultDto = MapToDto(newParent);
                return CreatedAtAction(nameof(GetParent), new { id = newParent.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parent.");
                return StatusCode(500, "שגיאה פנימית ביצירת הורה.");
            }
        }

        // POST: api/ParentOrphan/orphan
        [HttpPost("orphan")]
        public async Task<ActionResult<ParentOrphanDto>> CreateOrphan([FromBody] OrphanCreateUpdateDto orphanDto)
        {
            // --- ולידציה ראשונית ---
            if (!ModelState.IsValid)
            {
                string errors = string.Join("\n", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(errors);
            }

            // --- ולידציות לוגיות ---
            var validationErrors = new List<string>();
            if (await _context.Orphans.AnyAsync(o => o.IdentityNumber == orphanDto.IdentityNumber))
            {
                validationErrors.Add("יתום עם תעודת זהות זו כבר קיים.");
            }
            if (!await _context.Parents.AnyAsync(p => p.IdentityNumber == orphanDto.ParentIdentityNumber))
            {
                validationErrors.Add($"הורה עם תעודת זהות '{orphanDto.ParentIdentityNumber}' לא נמצא.");
            }
            if (!await _context.Statuses.AnyAsync(s => s.Id == orphanDto.StatusId))
            {
                validationErrors.Add("סטטוס לא תקין.");
            }

            if (validationErrors.Any())
            {
                return BadRequest(string.Join("\n", validationErrors));
            }
            // --- סוף ולידציות ---

            // --- Map DTO to Entity ---
            var newOrphan = new Orphan
            {
                ParentIdentityNumber = orphanDto.ParentIdentityNumber,
                IdentityNumber = orphanDto.IdentityNumber,
                LastName = orphanDto.LastName,
                FirstName = orphanDto.FirstName,
                BirthDate = orphanDto.BirthDate.Date,
                Gender = orphanDto.Gender,
                StatusId = orphanDto.StatusId,
                Neighborhood = orphanDto.Neighborhood,
                Street = orphanDto.Street,
                HouseNumber = orphanDto.HouseNumber,
                Floor = orphanDto.Floor,
                HomePhone = orphanDto.HomePhone,
                MobilePhone = orphanDto.MobilePhone,
                WorkPhone = orphanDto.WorkPhone,
                Email = orphanDto.Email,
                Notes = orphanDto.Notes,
                CreatedRow = DateTime.UtcNow,
                UpdatedRow = DateTime.UtcNow
            };
            // --- End Mapping ---

            try
            {
                _context.Orphans.Add(newOrphan);
                await _context.SaveChangesAsync();

                // Load related Status for the response DTO
                await _context.Entry(newOrphan).Reference(o => o.Status).LoadAsync();

                // Map created entity to DTO for response
                var resultDto = MapToDto(newOrphan);
                return CreatedAtAction(nameof(GetOrphan), new { id = newOrphan.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating orphan.");
                return StatusCode(500, "שגיאה פנימית ביצירת יתום.");
            }
        }

        // PUT: api/ParentOrphan/parent/{id}
        [HttpPut("parent/{id}")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] ParentCreateUpdateDto parentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingParent = await _context.Parents.FindAsync(id);
            if (existingParent == null)
            {
                return NotFound($"הורה עם מזהה {id} לא נמצא.");
            }

            // --- Business Logic Validations ---
            if (existingParent.IdentityNumber != parentDto.IdentityNumber &&
                await _context.Parents.AnyAsync(p => p.IdentityNumber == parentDto.IdentityNumber))
            {
                ModelState.AddModelError(nameof(parentDto.IdentityNumber), "תעודת הזהות החדשה כבר שייכת להורה אחר.");
                return BadRequest(ModelState);
            }
            if (!await _context.Statuses.AnyAsync(s => s.Id == parentDto.StatusId))
            {
                ModelState.AddModelError(nameof(parentDto.StatusId), "סטטוס לא תקין.");
                return BadRequest(ModelState);
            }
            // --- End Validations ---

            // --- Update existing entity from DTO ---
            existingParent.IdentityNumber = parentDto.IdentityNumber;
            existingParent.LastName = parentDto.LastName;
            existingParent.FirstName = parentDto.FirstName;
            existingParent.BirthDate = parentDto.BirthDate.Date;
            existingParent.Gender = parentDto.Gender;
            existingParent.StatusId = parentDto.StatusId;
            existingParent.Neighborhood = parentDto.Neighborhood;
            existingParent.Street = parentDto.Street;
            existingParent.HouseNumber = parentDto.HouseNumber;
            existingParent.Floor = parentDto.Floor;
            existingParent.HomePhone = parentDto.HomePhone;
            existingParent.MobilePhone = parentDto.MobilePhone;
            existingParent.WorkPhone = parentDto.WorkPhone;
            existingParent.Email = parentDto.Email;
            existingParent.Notes = parentDto.Notes;
            existingParent.UpdatedRow = DateTime.UtcNow;
            // --- End Update ---

            // Mark entity as modified (optional if change tracking is reliable)
            // _context.Entry(existingParent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // Standard success response for PUT
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, $"Concurrency error updating parent {id}.");
                // Check if the entity was deleted concurrently
                if (!await _context.Parents.AnyAsync(p => p.Id == id))
                {
                    return NotFound($"הורה עם מזהה {id} נמחק.");
                }
                else
                {
                    // General conflict message
                    return Conflict("אירעה שגיאת עדכון עקב שינוי במקביל. נסה לרענן ולשמור שוב.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating parent {id}.");
                return StatusCode(500, "שגיאה פנימית בעדכון ההורה.");
            }
        }

        // PUT: api/ParentOrphan/orphan/{id}
        [HttpPut("orphan/{id}")]
        public async Task<IActionResult> UpdateOrphan(int id, [FromBody] OrphanCreateUpdateDto orphanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingOrphan = await _context.Orphans.FindAsync(id);
            if (existingOrphan == null)
            {
                return NotFound($"יתום עם מזהה {id} לא נמצא.");
            }

            // --- Business Logic Validations ---
            if (existingOrphan.IdentityNumber != orphanDto.IdentityNumber &&
                await _context.Orphans.AnyAsync(o => o.IdentityNumber == orphanDto.IdentityNumber))
            {
                ModelState.AddModelError(nameof(orphanDto.IdentityNumber), "תעודת הזהות החדשה כבר שייכת ליתום אחר.");
                return BadRequest(ModelState);
            }
            if (!await _context.Parents.AnyAsync(p => p.IdentityNumber == orphanDto.ParentIdentityNumber))
            {
                ModelState.AddModelError(nameof(orphanDto.ParentIdentityNumber), $"הורה עם תעודת זהות '{orphanDto.ParentIdentityNumber}' לא נמצא.");
                return BadRequest(ModelState);
            }
            if (!await _context.Statuses.AnyAsync(s => s.Id == orphanDto.StatusId))
            {
                ModelState.AddModelError(nameof(orphanDto.StatusId), "סטטוס לא תקין.");
                return BadRequest(ModelState);
            }
            // --- End Validations ---

            // --- Update existing entity from DTO ---
            existingOrphan.ParentIdentityNumber = orphanDto.ParentIdentityNumber;
            existingOrphan.IdentityNumber = orphanDto.IdentityNumber;
            existingOrphan.LastName = orphanDto.LastName;
            existingOrphan.FirstName = orphanDto.FirstName;
            existingOrphan.BirthDate = orphanDto.BirthDate.Date;
            existingOrphan.Gender = orphanDto.Gender;
            existingOrphan.StatusId = orphanDto.StatusId;
            existingOrphan.Neighborhood = orphanDto.Neighborhood;
            existingOrphan.Street = orphanDto.Street;
            existingOrphan.HouseNumber = orphanDto.HouseNumber;
            existingOrphan.Floor = orphanDto.Floor;
            existingOrphan.HomePhone = orphanDto.HomePhone;
            existingOrphan.MobilePhone = orphanDto.MobilePhone;
            existingOrphan.WorkPhone = orphanDto.WorkPhone;
            existingOrphan.Email = orphanDto.Email;
            existingOrphan.Notes = orphanDto.Notes;
            existingOrphan.UpdatedRow = DateTime.UtcNow;
            // --- End Update ---

            // _context.Entry(existingOrphan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, $"Concurrency error updating orphan {id}.");
                if (!await _context.Orphans.AnyAsync(o => o.Id == id))
                {
                    return NotFound($"יתום עם מזהה {id} נמחק.");
                }
                else
                {
                    return Conflict("אירעה שגיאת עדכון עקב שינוי במקביל. נסה לרענן ולשמור שוב.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating orphan {id}.");
                return StatusCode(500, "שגיאה פנימית בעדכון היתום.");
            }
        }

        // DELETE: api/ParentOrphan/parent/{id}
        [HttpDelete("parent/{id}")]
        public async Task<IActionResult> DeleteParent(int id)
        {
            // FindAsync is efficient for finding by primary key
            var parent = await _context.Parents
                .Include(p => p.Orphans)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (parent == null)
            {
                return NotFound($"הורה עם מזהה {id} לא נמצא.");
            }

            // Cascade delete is configured in DbContext, so removing parent removes orphans
            _context.Parents.Remove(parent);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // Standard success response for DELETE
            }
            catch (DbUpdateException ex) // Catch potential DB constraint issues if cascade fails
            {
                _logger.LogError(ex, $"Error deleting parent {id}. Potential constraint violation.");
                // You might return a specific message if needed, e.g., based on InnerException
                return StatusCode(500, "שגיאה במחיקת ההורה. ייתכן שיש נתונים מקושרים שלא ניתנים למחיקה אוטומטית.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting parent {id}.");
                return StatusCode(500, "שגיאה פנימית במחיקת ההורה.");
            }
        }

        // DELETE: api/ParentOrphan/orphan/{id}
        [HttpDelete("orphan/{id}")]
        public async Task<IActionResult> DeleteOrphan(int id)
        {
            var orphan = await _context.Orphans.FindAsync(id);
            if (orphan == null)
            {
                return NotFound($"יתום עם מזהה {id} לא נמצא.");
            }

            _context.Orphans.Remove(orphan);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error deleting orphan {id}. Potential constraint violation.");
                return StatusCode(500, "שגיאה במחיקת היתום.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting orphan {id}.");
                return StatusCode(500, "שגיאה פנימית במחיקת היתום.");
            }
        }

        // --- Private Helper Methods for Mapping ---
        // Maps a Parent entity to ParentOrphanDto (used in CreateParent response and potentially GetParentsAndOrphans)
        private ParentOrphanDto MapToDto(Parent p)
        {
            if (p == null) return null;
            return new ParentOrphanDto
            {
                Id = p.Id,
                IsParent = true,
                IdentityNumber = p.IdentityNumber,
                LastName = p.LastName,
                FirstName = p.FirstName,
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                Status = p.Status?.StatusName, // Use loaded Status navigation property
                StatusId = p.StatusId,
                Neighborhood = p.Neighborhood,
                Street = p.Street,
                HouseNumber = p.HouseNumber,
                Floor = p.Floor,
                HomePhone = p.HomePhone,
                MobilePhone = p.MobilePhone,
                WorkPhone = p.WorkPhone,
                Email = p.Email,
                Notes = p.Notes,
                // Children are typically not included when mapping a single parent after create/update
                // If needed, they would have to be loaded and mapped separately.
                Children = new List<ParentOrphanDto>()
            };
        }

        // Maps an Orphan entity to ParentOrphanDto (used in CreateOrphan response and potentially GetParentsAndOrphans)
        private ParentOrphanDto MapToDto(Orphan o)
        {
            if (o == null) return null;
            return new ParentOrphanDto
            {
                Id = o.Id,
                IsParent = false,
                IdentityNumber = o.IdentityNumber,
                LastName = o.LastName,
                FirstName = o.FirstName,
                BirthDate = o.BirthDate,
                Gender = o.Gender,
                Status = o.Status?.StatusName, // Use loaded Status navigation property
                StatusId = o.StatusId,
                Neighborhood = o.Neighborhood,
                Street = o.Street,
                HouseNumber = o.HouseNumber,
                Floor = o.Floor,
                HomePhone = o.HomePhone,
                MobilePhone = o.MobilePhone,
                WorkPhone = o.WorkPhone,
                Email = o.Email,
                Notes = o.Notes,
                Children = null // Orphans don't have children in this DTO structure
            };
        }
    }
}