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
		//private string photoPath = null;
		//private string otherPath = null;
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


		[HttpPost]
		public async Task<IActionResult> Upload()
		{
			var request = HttpContext.Request;
			foreach (var file in request.Form.Files)
			{
				int id;
				string[] s;
				if (file.Length > 0)
				{
					s = file.FileName.Split("+");
					id = Convert.ToInt32(s[0]);
					var path = Path.Combine(filePath, s[1]);
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
				f.Name = s[1];
				f.Path = Path.Combine(filePath, s[1]).ToString();
				//context_.Files.Add(f);
				Blog blog = context_.Blogs.Find(id);
				if (blog.Files == null)
				{
					blog.Files = new List<Models.File>();
				}
				blog.Files.Add(f);
				context_.SaveChanges();
			}

			return Ok();
		}



		// DELETE api/<controller>/5
		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			var f = context_.Files.Find(id);
			if (f == null) return NotFound();
			else
			{
				context_.Remove(f);
				context_.SaveChanges();
			}
			List<string> files = null;
			string file = "";
			try
			{
				files = Directory.GetFiles(filePath).ToList<string>();
				bool flag = false;
				foreach (var fi in files)
				{
					string tmp = fi;
					//string[] s = fi.Split("+");
					//if (s.Count() == 2) tmp = s[1]; else tmp = s[0];  
					if (tmp == f.Path)
					{
						file = Path.GetFileName(tmp);
						System.IO.File.Delete(Path.Combine(filePath, file).ToString());
						flag = true;
					}

				}
				if (!flag) return NotFound();
			}
			catch
			{
				return NotFound();
			}

			var blog = context_.Blogs.Find(f.BlogId);
			var fil = context_.Files.Where(c => c.BlogId == blog.BlogId);

			blog.Files = fil.OrderBy(c => c.Id).Select(c => c).ToList<Models.File>();
			if (blog.Files == null)
			{
				blog.Files = new List<Models.File>();
				context_.SaveChanges();
			}
			foreach (var ff in blog.Files)
			{
				if (ff.Id == id)
				{

					blog.Files.Remove(ff);
				}
			}
			context_.SaveChanges();
			context_.SaveChanges();
			return Ok();
		}

		[HttpPut("{id}")]
		public IActionResult edit(int id, [FromBody] string value)
		{
			//var content = HttpContext.Request;
			Blog blog = context_.Blogs.Find(id);
			//string s;
			//content.Form.TryGetValue(s);
			blog.Content = value;
			context_.SaveChanges();
			return Ok();
		}
	}
	
}