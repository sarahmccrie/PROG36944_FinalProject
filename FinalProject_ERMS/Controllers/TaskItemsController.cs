using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinalProject_ERMS.Data;
using FinalProject_ERMS.Models;

namespace FinalProject_ERMS.Controllers
{
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
            ViewData["AssignedEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");
            return View();
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,Name,Description,ProjectId,AssignedEmployeeId,Priority,Status")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email", taskItem.AssignedEmployeeId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "Name", taskItem.ProjectId);
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
            ViewData["AssignedEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email", taskItem.AssignedEmployeeId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "Name", taskItem.ProjectId);
            ViewBag.ProjectList = new SelectList(_context.Projects, "ProjectId", "Name");
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");
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
            ViewData["AssignedEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "Email", taskItem.AssignedEmployeeId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "Name", taskItem.ProjectId);
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
