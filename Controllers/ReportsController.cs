using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public IActionResult Index()
    {
        return View();
    }
}
