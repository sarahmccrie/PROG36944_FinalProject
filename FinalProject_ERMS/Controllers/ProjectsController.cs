using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinalProject_ERMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject_ERMS.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProjectsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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

        private async Task LoadManagersAsync(object selectedManager = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/EmployeesApi");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var employees = await response.Content.ReadFromJsonAsync<List<Employee>>();
                var managers = employees.Where(e => e.Role == "Manager").ToList();
                ViewBag.ManagerList = new SelectList(managers, "EmployeeId", "Name", selectedManager);
            }
            else
            {
                ViewBag.ManagerList = new SelectList(new List<Employee>(), "EmployeeId", "Name");
            }
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/ProjectsApi");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/ProjectsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var project = await response.Content.ReadFromJsonAsync<Project>();
            if (project == null) return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            await LoadManagersAsync();
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (!ModelState.IsValid)
            {
                await LoadManagersAsync(project.ManagerId);
                return View(project);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/ProjectsApi")
            {
                Content = JsonContent.Create(project)
            };
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create project via API.");
                await LoadManagersAsync(project.ManagerId);
                return View(project);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/ProjectsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var project = await response.Content.ReadFromJsonAsync<Project>();
            if (project == null) return NotFound();

            await LoadManagersAsync(project.ManagerId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.ProjectId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadManagersAsync(project.ManagerId);
                return View(project);
            }

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/ProjectsApi/{project.ProjectId}")
            {
                Content = JsonContent.Create(project)
            };
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update project via API.");
                await LoadManagersAsync(project.ManagerId);
                return View(project);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/ProjectsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var project = await response.Content.ReadFromJsonAsync<Project>();
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/ProjectsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete project via API.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
