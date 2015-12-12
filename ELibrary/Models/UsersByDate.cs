using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELibrary.Models
{
	public class UsersByDate
	{
		public User UserInstance { get; set; }

		public int Month { get; set; }

		public int PurchaseCount { get; set; }
	}
}