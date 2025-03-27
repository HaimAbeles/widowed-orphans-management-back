using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WidowedOrphansManagement.Data;
using WidowedOrphansManagement.Models;
using System.Linq;
using System.Threading.Tasks;
using WidowedOrphansManagement.Data.Contexts;

namespace WidowedOrphansManagement.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly WidowedOrphansContext _context;

        public StatusController(WidowedOrphansContext context)
        {
            _context = context;
        }

        // GET: api/Status
        [HttpGet]
        public async Task<IActionResult> GetStatuses()
        {
            try
            {
                var statuses = await _context.Statuses.ToListAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהבאת הסטטוסים: {ex.Message}");
            }
        }

        // GET: api/Status/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatus(int id)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);
                if (status == null)
                {
                    return NotFound($"סטטוס לא נמצא עם מזהה {id}");
                }
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהבאת הסטטוס: {ex.Message}");
            }
        }

        // POST: api/Status
        [HttpPost]
        public async Task<IActionResult> CreateStatus([FromBody] Status status)
        {
            try
            {
                if (status == null || string.IsNullOrEmpty(status.StatusName))
                {
                    return BadRequest("הסטטוס לא תקין, שם הסטטוס חייב להיות מלא.");
                }

                if (_context.Statuses.Any(x => x.StatusName == status.StatusName))
                {
                    return BadRequest("סוג הסטטוס כבר קיים");
                }

                _context.Statuses.Add(status);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetStatus", new { id = status.Id }, status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהוספת הסטטוס: {ex.Message}");
            }
        }

        // PUT: api/Status/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] Status status)
        {
            try
            {
                if (id != status.Id)
                {
                    return BadRequest("מזהה הסטטוס לא תואם.");
                }

                _context.Entry(status).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, $"שגיאה בעדכון הסטטוס: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בעדכון הסטטוס: {ex.Message}");
            }
        }

        // DELETE: api/Status/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);
                if (status == null)
                {
                    return NotFound($"סטטוס לא נמצא עם מזהה {id}");
                }

                _context.Statuses.Remove(status);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהסרת הסטטוס: {ex.Message}");
            }
        }
    }
}
