using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject_ERMS.Data;
using FinalProject_ERMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject_ERMS.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TaskItemsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
        {
            return await _context.TaskItems
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Project)
                .ToListAsync();
        }

        // GET: api/TaskItemsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
        {
            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return taskItem;
        }

        // PUT: api/TaskItemsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskItem(int id, TaskItem taskItem)
        {
            if (id != taskItem.TaskId)
            {
                return BadRequest();
            }

            _context.Entry(taskItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TaskItemsApi
        [HttpPost]
        public async Task<ActionResult<TaskItem>> PostTaskItem(TaskItem taskItem)
        {
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaskItem", new { id = taskItem.TaskId }, taskItem);
        }

        // DELETE: api/TaskItemsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.TaskId == id);
        }
    }
}
