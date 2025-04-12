using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinalProject_ERMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject_ERMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeesController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        private void CopyCookiesToRequest(HttpRequestMessage requestMessage)
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Headers["Cookie"];
            if (!string.IsNullOrEmpty(cookie))
            {
                requestMessage.Headers.Add("Cookie", cookie.ToString());
            }
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/EmployeesApi");
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Problem("API call failed.");
            }

            var employees = await response.Content.ReadFromJsonAsync<List<Employee>>();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/EmployeesApi/{id}");
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Problem("API call failed.");
            }

            var employee = await response.Content.ReadFromJsonAsync<Employee>();
            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
                return View(employee);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/EmployeesApi")
            {
                Content = JsonContent.Create(employee)
            };
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create employee via API.");
                ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
                return View(employee);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/EmployeesApi/{id}");
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Problem("API call failed.");
            }

            var employee = await response.Content.ReadFromJsonAsync<Employee>();
            if (employee == null) return NotFound();

            ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
                return View(employee);
            }

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/EmployeesApi/{employee.EmployeeId}")
            {
                Content = JsonContent.Create(employee)
            };
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update employee via API.");
                ViewBag.Roles = new List<string> { "Admin", "Manager", "Employee" };
                return View(employee);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/EmployeesApi/{id}");
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Problem("API call failed.");
            }

            var employee = await response.Content.ReadFromJsonAsync<Employee>();
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/EmployeesApi/{id}");
            CopyCookiesToRequest(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete employee via API.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
