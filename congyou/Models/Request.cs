using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace congyou.Models
{
	public class Request
	{
		[Key]
		public int Id { get; set; }
		public int? BlogId { get; set; }
		public string UserName { get; set; }
		public Blog Blog { get; set; }
		public bool IsAccepted { get; set; }
	}
}
