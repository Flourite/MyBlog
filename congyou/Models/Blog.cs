using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace congyou.Models
{
	public class Blog
	{
		public int BlogId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }

		public ICollection<Comment> Comments { get; set; }
	}

	public class Comment
	{
		public int CommentId { get; set; }
		public string CommenterName { get; set; }
		public string Content { get; set; }

		public int? BlogId { get; set; }
		public Blog Blog { get; set; }
	}
}
