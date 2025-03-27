using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WidowedOrphansManagement.Data;
using WidowedOrphansManagement.Models;
using System.Linq;
using WidowedOrphansManagement.Data.Contexts;

namespace WidowedOrphansManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentOrphanController : ControllerBase
    {
        private readonly WidowedOrphansContext _context;

        public ParentOrphanController(WidowedOrphansContext context)
        {
            _context = context;
        }

        [HttpGet("get-parents-and-orphans")]
        public async Task<IActionResult> GetParentsAndOrphans()
        {
            try
            {
                // שליפת כל ההורים עם ילדים שלהם, כולל הילדים
                var parents = await _context.Parents
                    .Include(p => p.Orphans)  // כולל את הילדים
                    .OrderBy(p => p.LastName)  // מיון לפי שם משפחה
                    .ThenBy(p => p.FirstName)  // מיון לפי שם פרטי
                    .Select(p => new ParentOrphanDto
                    {
                        IsParent = true,
                        IdentityNumber = p.IdentityNumber,
                        LastName = p.LastName,
                        FirstName = p.FirstName,
                        BirthDate = p.BirthDate,
                        Gender = p.Gender,
                        Status = p.Status.StatusName,  // הסטטוס של ההורה
                        Neighborhood = p.Neighborhood,
                        Street = p.Street,
                        HouseNumber = p.HouseNumber,
                        Floor = p.Floor,
                        HomePhone = p.HomePhone,
                        MobilePhone = p.MobilePhone,
                        WorkPhone = p.WorkPhone,
                        Email = p.Email,
                        Notes = p.Notes,
                        Children = p.Orphans.OrderBy(o => o.LastName).ThenBy(o => o.FirstName)  // מיון הילדים לפי א"ב של השם הפרטי והמשפחה
                        .Select(o => new ParentOrphanDto
                        {
                            IsParent = false,
                            IdentityNumber = o.IdentityNumber,
                            LastName = o.LastName,
                            FirstName = o.FirstName,
                            BirthDate = o.BirthDate,
                            Gender = o.Gender,
                            Status = o.Status.StatusName,  // הסטטוס של הילד
                            Neighborhood = o.Neighborhood,
                            Street = o.Street,
                            HouseNumber = o.HouseNumber,
                            Floor = o.Floor,
                            HomePhone = o.HomePhone,
                            MobilePhone = o.MobilePhone,
                            WorkPhone = o.WorkPhone,
                            Email = o.Email,
                            Notes = o.Notes
                        }).ToList()
                    }).ToListAsync();

                return Ok(parents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהבאת הנתונים: {ex.Message}");
            }
        }
    }
}
