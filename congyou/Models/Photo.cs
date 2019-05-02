using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace congyou.Models
{
	public class Photo
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string CompletePath { get; set; }
		public int? BlogId { get; set; }
		public Blog Blog { get; set; }
	}
}
