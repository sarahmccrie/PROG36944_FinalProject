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
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class TaskItemsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskItemsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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

        private async Task LoadDropdownsAsync(object selectedProject = null, object selectedEmployee = null)
        {
            // Projects
            var projectRequest = new HttpRequestMessage(HttpMethod.Get, "/api/ProjectsApi");
            CopyCookiesToRequest(projectRequest);
            var projectResponse = await _httpClient.SendAsync(projectRequest);
            var projects = projectResponse.IsSuccessStatusCode
                ? await projectResponse.Content.ReadFromJsonAsync<List<Project>>()
                : new List<Project>();
            ViewBag.ProjectList = new SelectList(projects, "ProjectId", "Name", selectedProject);

            // Employees
            var employeeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/EmployeesApi");
            CopyCookiesToRequest(employeeRequest);
            var employeeResponse = await _httpClient.SendAsync(employeeRequest);
            var employees = employeeResponse.IsSuccessStatusCode
                ? await employeeResponse.Content.ReadFromJsonAsync<List<Employee>>()
                : new List<Employee>();
            ViewBag.EmployeeList = new SelectList(employees, "EmployeeId", "Name", selectedEmployee);

            // Priority
            ViewBag.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "High", Text = "High" },
                new SelectListItem { Value = "Medium", Text = "Medium" },
                new SelectListItem { Value = "Low", Text = "Low" }
            };

            // Status
            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Not Started", Text = "Not Started" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Blocked", Text = "Blocked" }
            };
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/TaskItemsApi");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var taskItems = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
            return View(taskItems);
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/TaskItemsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var taskItem = await response.Content.ReadFromJsonAsync<TaskItem>();
            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // GET: TaskItems/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(taskItem.ProjectId, taskItem.AssignedEmployeeId);
                return View(taskItem);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/TaskItemsApi")
            {
                Content = JsonContent.Create(taskItem)
            };
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create task via API.");
                await LoadDropdownsAsync(taskItem.ProjectId, taskItem.AssignedEmployeeId);
                return View(taskItem);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/TaskItemsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var taskItem = await response.Content.ReadFromJsonAsync<TaskItem>();
            if (taskItem == null) return NotFound();

            await LoadDropdownsAsync(taskItem.ProjectId, taskItem.AssignedEmployeeId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.TaskId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(taskItem.ProjectId, taskItem.AssignedEmployeeId);
                return View(taskItem);
            }

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/TaskItemsApi/{taskItem.TaskId}")
            {
                Content = JsonContent.Create(taskItem)
            };
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update task via API.");
                await LoadDropdownsAsync(taskItem.ProjectId, taskItem.AssignedEmployeeId);
                return View(taskItem);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/TaskItemsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Problem("API call failed.");

            var taskItem = await response.Content.ReadFromJsonAsync<TaskItem>();
            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/TaskItemsApi/{id}");
            CopyCookiesToRequest(request);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete task via API.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
