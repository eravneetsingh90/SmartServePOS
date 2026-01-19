using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartServePOS.Models
{
	public class BrandDto
	{
		public int LocalId { get; set; }
		public int Id { get; set; }              // ServerId
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
