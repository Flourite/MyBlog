using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using congyou.Models;
using congyou.Data;

namespace congyou.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext context_;
		private const string sessionId_ = "SessionId";

		public HomeController(ApplicationDbContext context)
		{
			context_ = context;
		}

		public IActionResult Index()
		{
			return View(context_.Blogs.ToList<Blog>());
		}

		public IActionResult Comments()
		{
			var cmts = context_.Comments.Include(c => c.Blog);
			var orderedcmts = from c in cmts
												orderby c.CommentId
												select c;
			return View(orderedcmts);
		}

		[HttpGet]
		public IActionResult CreateBlog(int id)
		{
			var model = new Blog();
			return View(model);
		}

		[HttpPost]
		public IActionResult CreateBlog(int id, Blog blg)
		{
			context_.Blogs.Add(blg);
			context_.SaveChanges();
			return RedirectToAction("Index");
		}

		public IActionResult DeleteBlog(int? id)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			try
			{
				var blog = context_.Blogs.Find(id);
				if (blog != null)
				{
					context_.Remove(blog);
					context_.SaveChanges();
				}
			}
			catch (Exception)
			{ }

			return RedirectToAction("Index");
		}

		public ActionResult BlogContents(int? id)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			Blog blog = context_.Blogs.Find(id);

			if (blog == null)
			{
				return StatusCode(StatusCodes.Status404NotFound);
			}

			var cmts = context_.Comments.Where(c => c.Blog == blog);

			blog.Comments = cmts.OrderBy(c => c.CommentId).Select(c => c).ToList<Comment>();

			if (blog.Comments == null)
			{
				blog.Comments = new List<Comment>();
				Comment cmt = new Comment();
				cmt.CommenterName = "none";
				cmt.Content = "none";
				blog.Comments.Add(cmt);
			}

			return View(blog);
		}

		[HttpGet]
		public IActionResult EditBlog(int? id)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			Blog blog = context_.Blogs.Find(id);
			if (blog == null)
			{
				return StatusCode(StatusCodes.Status404NotFound);
			}
			return View(blog);
		}

		[HttpPost]
		public IActionResult EditBlog(int? id, Blog blg)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			var blog = context_.Blogs.Find(id);
			if (blog != null)
			{
				blog.Title = blg.Title;
				blog.Content = blg.Content;
				try
				{
					context_.SaveChanges();
				}
				catch (Exception)
				{ }
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult AddComment(int id)
		{
			HttpContext.Session.SetInt32(sessionId_, id);
			Blog blog = context_.Blogs.Find(id);
			if (blog == null)
			{
				return StatusCode(StatusCodes.Status404NotFound);
			}

			Comment cmt = new Comment();
			return View(cmt);
		}
		
		[HttpPost]
		public IActionResult AddComment(int? id, Comment cmt)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}

			int? blogId_ = HttpContext.Session.GetInt32(sessionId_);

			var blog = context_.Blogs.Find(blogId_);

			if (blog != null)
			{
				if (blog.Comments == null)
				{
					List<Comment> comments = new List<Comment>();
					blog.Comments = comments;
				}
				blog.Comments.Add(cmt);

				try
				{
					context_.SaveChanges();
				}
				catch (Exception)
				{ }
			}

			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult EditComment(int? id)
		{
			if (id == null)
			{
				return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
			}
			Comment comment = context_.Comments.Find(id);
			if (comment == null)
			{
				return StatusCode(StatusCodes.Status404NotFound);
			}
			return View(comment);
		}

		[HttpPost]
		public IActionResult EditComment(int? id, Comment cmt)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			var comment = context_.Comments.Find(id);
			if (comment != null)
			{
				comment.Content = cmt.Content;

				try
				{
					context_.SaveChanges();
				}
				catch (Exception)
				{ }
			}
			return RedirectToAction("Index");
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
