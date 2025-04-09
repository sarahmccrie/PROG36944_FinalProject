using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinalProject_ERMS.Data;
using FinalProject_ERMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject_ERMS.Controllers
{
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TaskItems.Include(t => t.AssignedEmployee).Include(t => t.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        public IActionResult Create()
        {
            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");

            ViewBag.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "High", Text = "High" },
                new SelectListItem { Value = "Medium", Text = "Medium" },
                new SelectListItem { Value = "Low", Text = "Low" }
            };

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Not Started", Text = "Not Started" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Blocked", Text = "Blocked" }
            };

            return View();
        }


        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,Name,Description,ProjectId,AssignedEmployeeId,Priority,Status")] TaskItem taskItem)
        {
            Console.WriteLine("POST Create action called for TaskItem");
            Console.WriteLine($"TaskId: {taskItem.TaskId}");
            Console.WriteLine($"Name: {taskItem.Name}");
            Console.WriteLine($"Description: {taskItem.Description}");
            Console.WriteLine($"ProjectId: {taskItem.ProjectId}");
            Console.WriteLine($"AssignedEmployeeId: {taskItem.AssignedEmployeeId}");
            Console.WriteLine($"Priority: {taskItem.Priority}");
            Console.WriteLine($"Status: {taskItem.Status}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is NOT valid");
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    var errors = state.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ModelState error for {key}: {error.ErrorMessage}");
                    }
                }
            }
            else
            {
                Console.WriteLine("ModelState is valid. Proceeding to save.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                Console.WriteLine("TaskItem saved successfully!");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");

            ViewBag.PriorityList = new List<SelectListItem>
    {
        new SelectListItem { Value = "High", Text = "High" },
        new SelectListItem { Value = "Medium", Text = "Medium" },
        new SelectListItem { Value = "Low", Text = "Low" }
    };

            ViewBag.StatusList = new List<SelectListItem>
    {
        new SelectListItem { Value = "Not Started", Text = "Not Started" },
        new SelectListItem { Value = "In Progress", Text = "In Progress" },
        new SelectListItem { Value = "Completed", Text = "Completed" },
        new SelectListItem { Value = "Blocked", Text = "Blocked" }
    };

            return View(taskItem);
        }



        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");

            ViewBag.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "High", Text = "High" },
                new SelectListItem { Value = "Medium", Text = "Medium" },
                new SelectListItem { Value = "Low", Text = "Low" }
            };

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Not Started", Text = "Not Started" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Blocked", Text = "Blocked" }
            };

            return View(taskItem);

        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskId,Name,Description,ProjectId,AssignedEmployeeId,Priority,Status")] TaskItem taskItem)
        {
            if (id != taskItem.TaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.TaskId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");

            ViewBag.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "High", Text = "High" },
                new SelectListItem { Value = "Medium", Text = "Medium" },
                new SelectListItem { Value = "Low", Text = "Low" }
            };

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Not Started", Text = "Not Started" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Blocked", Text = "Blocked" }
            };

            return View(taskItem);

        }

        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
                _context.TaskItems.Remove(taskItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.TaskId == id);
        }
    }
}
