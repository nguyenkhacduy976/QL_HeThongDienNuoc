using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin")]
public class CustomersController : Controller
{
    private readonly IApiClient _apiClient;

    public CustomersController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
            return View(customers ?? new List<CustomerDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
            return View(new List<CustomerDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _apiClient.DeleteAsync($"customers/{id}");
            TempData["Success"] = "Xóa khách hàng thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var customer = await _apiClient.GetAsync<CustomerDto>($"customers/{id}");
            
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(customer);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var customer = await _apiClient.GetAsync<CustomerDto>($"customers/{id}");
            return View(customer);
        }

        try
        {
            await _apiClient.PutAsync<CustomerDto>($"customers/{id}", dto);
            TempData["Success"] = "Cập nhật khách hàng thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            var customer = await _apiClient.GetAsync<CustomerDto>($"customers/{id}");
            return View(customer);
        }
    }
}
