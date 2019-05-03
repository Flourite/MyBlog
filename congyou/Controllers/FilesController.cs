using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using congyou.Models;
using congyou.Data;
using System.IO;

namespace congyou.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly ApplicationDbContext context_;
		private readonly IHostingEnvironment hostingEnvironment_;
		private string webRootPath = null;
		private string filePath = null;
		private string photoPath = null;
		private string otherPath = null;
		public FilesController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
		{
			hostingEnvironment_ = hostingEnvironment;
			webRootPath = hostingEnvironment_.WebRootPath;
			filePath = Path.Combine(webRootPath, "FileStorage");
			context_ = context;
		}


		[HttpGet]
		public IEnumerable<string> Get()
		{
			List<string> files = null;
			try
			{
				files = Directory.GetFiles(filePath).ToList<string>();
				for (int i = 0; i < files.Count; ++i)
					files[i] = Path.GetFileName(files[i]);
			}
			catch
			{
				files = new List<string>();
				files.Add("404 - Not Found");
			}
			return files;
		}


		
		[HttpGet("{id}")]
		public async Task<IActionResult> Download(int id)
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

		
		[HttpPost("{id}")]
		public async Task<IActionResult> Upload(int id)
		{
			var request = HttpContext.Request;
			foreach (var file in request.Form.Files)
			{
				if (file.Length > 0)
				{
					var path = Path.Combine(filePath, file.FileName);
					using (var fileStream = new FileStream(path, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}
				}
				else
				{
					return BadRequest();
				}
				Models.File f = new Models.File();
				f.BlogId = id;
				f.Name = file.FileName;
				f.Path = Path.Combine(photoPath, file.FileName).ToString();
				context_.Files.Add(f);
				Blog blog = context_.Blogs.Find(id);
				blog.Files.Add(f);
				context_.SaveChanges();
			}

			return Ok();
		}

	

		// DELETE api/<controller>/5
		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			List<string> files = null;
			string file = "";
			try
			{
				files = Directory.GetFiles(filePath).ToList<string>();
				if (0 <= id && id < files.Count)
				{
					file = Path.GetFileName(files[id]);
					System.IO.File.Delete(file);
				}
				else
					return NotFound();
			}
			catch
			{
				return NotFound();
			}
			var f = context_.Files.Find(id);
			context_.Files.Remove(f);
			var blog = context_.Blogs.Find(f.BlogId);
			blog.Files.Remove(f);
			context_.SaveChanges();
			return Ok();
		}
	}
	
}