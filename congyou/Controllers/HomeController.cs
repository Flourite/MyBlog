using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using congyou.Models;
using congyou.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace congyou.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext context_;
		private const string sessionId_ = "SessionId";
		private readonly IHostingEnvironment hostingEnvironment_;
		private string webRootPath = null;
		private string filePath = null;
		private string photoPath = null;
		private string otherPath = null;

		public HomeController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
		{
			hostingEnvironment_ = hostingEnvironment;
			webRootPath = hostingEnvironment_.WebRootPath;
			filePath = Path.Combine(webRootPath, "FileStorage");
			photoPath = Path.Combine(filePath, "Photos");
			otherPath = Path.Combine(filePath, "Files");
			context_ = context;
		}

		public IActionResult Index()
		{
			var blogs = context_.Blogs.ToList<Blog>();
			return View(blogs);
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
		[Authorize(Roles = "Admin")]
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

		[Authorize(Roles = "Admin")]
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

			if (User.IsInRole("User"))
			{
				var requests = context_.Requests.Where(c => c.UserName == User.Identity.Name && c.BlogId == blog.BlogId);
				if (requests == null) return StatusCode(StatusCodes.Status403Forbidden);
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
		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
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

		[HttpGet]
		public async Task<IActionResult> DownloadFile(int id)
		{
			List<string> files = null;
			string file = "";
			try
			{
				files = Directory.GetFiles(filePath).ToList<string>();
				if (0 <= id && id < files.Count)
					file = Path.GetFileName(files[id]);
				else
					return NotFound();
			}
			catch
			{
				return NotFound();
			}
			var memory = new MemoryStream();
			file = files[id];
			using (var stream = new FileStream(file, FileMode.Open))
			{
				await stream.CopyToAsync(memory);
			}
			memory.Position = 0;
			return File(memory, GetContentType(file), Path.GetFileName(file));
		}

		private string GetContentType(string path)
		{
			var types = GetMimeTypes();
			var ext = Path.GetExtension(path).ToLowerInvariant();
			return types[ext];
		}

		private Dictionary<string, string> GetMimeTypes()
		{
			return new Dictionary<string, string>
			{
				{".cs", "application/C#" },
				{".txt", "text/plain"},
				{".pdf", "application/pdf"},
				{".doc", "application/vnd.ms-word"},
				{".docx", "application/vnd.ms-word"},
				{".xls", "application/vnd.ms-excel"},
				{".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
				{".png", "image/png"},
				{".jpg", "image/jpeg"},
				{".jpeg", "image/jpeg"},
				{".gif", "image/gif"},
				{".csv", "text/csv"}
			};
		}

		[Authorize(Roles = "Admin")]
		public ActionResult EditFiles(int? id)
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

		[Authorize(Roles = "Admin")]
		public IActionResult DeleteFile(int? id)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			var file = context_.Files.Find(id);
			try
			{
				//var file = context_.Files.Find(id);
				if (file != null)
				{
					context_.Remove(file);
					context_.SaveChanges();
				}
			}
			catch (Exception)
			{ }

			var blog = context_.Blogs.Find(file.BlogId);
			foreach (var f in blog.Files)
			{
				if (f.Id == id) blog.Files.Remove(f);
			}
			context_.SaveChanges();
			return RedirectToAction("EditFiles", file.BlogId);
		}

		[Authorize(Roles = "Admin")]
		public IActionResult DeletePhoto(int? id)
		{
			if (id == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest);
			}
			var photo = context_.Photos.Find(id);
			try
			{
				//var file = context_.Files.Find(id);
				if (photo != null)
				{
					context_.Remove(photo);
					context_.SaveChanges();
				}
			}
			catch (Exception)
			{ }

			var blog = context_.Blogs.Find(photo.BlogId);
			foreach (var f in blog.Photos)
			{
				if (f.Id == id) blog.Photos.Remove(f);
			}
			return RedirectToAction("EditFiles", photo.BlogId);
		}

		public ActionResult SendRequest()
		{
			var requests = context_.Requests.Where(c => c.IsAccepted == false);

			return View(requests.ToList<Request>());
		}

		public ActionResult PendingRequests()
		{
			var requests = context_.Requests.Where(c => c.IsAccepted == false && c.UserName == User.Identity.Name);

			return View(requests.ToList<Request>());
		}
		public IActionResult Privacy()
		{
			return View();
		}

		public ActionResult ApproveRequest(int id)
		{
			var request = context_.Requests.Find(id);
			request.IsAccepted = true;
			context_.SaveChanges();
			return RedirectToAction("SendRequest");
		}

		public ActionResult IgnoreRequest(int id)
		{
			var request = context_.Requests.Find(id);
			context_.Remove(request);
			context_.SaveChanges();
			return RedirectToAction("SendRequest");
		}

		public ActionResult CancelRequest(int id)
		{
			var request = context_.Requests.Find(id);
			context_.Remove(request);
			context_.SaveChanges();
			return RedirectToAction("PendingRequest");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
