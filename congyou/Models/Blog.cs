using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace congyou.Models
{
	public class Blog
	{
		[Key]
		public int BlogId
		{ get; set; }
		public string Title { get; set; }
		public string Content { get; set; }

		public ICollection<Comment> Comments { get; set; }

		public ICollection<Photo> Photos { get; set; }

		public ICollection<File> Files { get; set; }
	}

	public class Comment
	{
		[Key]
		public int CommentId { get; set; }
		public string CommenterName { get; set; }
		public string Content { get; set; }

		public int? BlogId { get; set; }
		public Blog Blog { get; set; }
	}
}
