using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WidowedOrphansManagement.Services.ExcelService
{
    public interface IExcelService
    {
        Task<string> ProcessParentsExcelFileAsync(IFormFile file);
        Task<string> ProcessOrphansExcelFileAsync(IFormFile file);
        byte[] GenerateParentsExampleExcel();
        byte[] GenerateOrphansExampleExcel();
    }
}
