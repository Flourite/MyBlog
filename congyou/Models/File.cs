using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace congyou.Models
{
	public class File
	{
		public int Id { get; set; }
		public string Path { get; set; }
		public string Name { get; set; }
		public int? BlogId { get; set; }
		//public Blog Blog { get; set; }
	}
}
