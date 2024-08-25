using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;

namespace ProjectAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskCommentController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;
		public TaskCommentController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost]
		[Route("task-comments")]
		public async Task<IActionResult> AddTaskComment([FromBody] TaskComment taskComment)
		{
			await _authContext.TaskComments.AddAsync(taskComment);
			await _authContext.SaveChangesAsync();

			return Ok(new { TaskCommentId = taskComment.CommentId, Message = "Comment Added!" });

		}

		[HttpGet("{taskId}")]
		public async Task<ActionResult<IEnumerable<TaskComment>>> GetAllTaskComments(Guid taskId)
		{
			var task = await _authContext.Tasks.FindAsync(taskId);

			if (task == null)
			{
				return NotFound("Task not found.");
			}

			var comments = await _authContext.TaskComments
				.Where(comment => comment.TaskId == taskId)
				.ToListAsync();

			if (comments.Count == 0)
			{
				return Ok("There are no comments for this task.");
			}

			return Ok(comments);
		}

		[HttpPut("{commentId}")]
		public async Task<IActionResult> EditTaskComment(int commentId, [FromBody] TaskComment updatedComment)
		{
			var existingComment = await _authContext.TaskComments.FindAsync(commentId);

			if (existingComment == null)
			{
				return NotFound("Comment not found.");
			}

			existingComment.Comment = updatedComment.Comment;
			existingComment.UserId = updatedComment.UserId;
			existingComment.TaskId = updatedComment.TaskId;

			_authContext.TaskComments.Update(existingComment);
			await _authContext.SaveChangesAsync();

			return Ok(new { TaskCommentId = existingComment.CommentId, Message = "Comment updated successfully!" });
		}

		[HttpDelete("{commentId}")]
		public async Task<IActionResult> DeleteTaskComment(int commentId)
		{
			var comment = await _authContext.TaskComments.FindAsync(commentId);

			if (comment == null)
			{
				return NotFound("Comment not found");
			}

			_authContext.TaskComments.Remove(comment);

			await _authContext.SaveChangesAsync();

			return Ok("Comment has been removed.");
		}

	}
}
