using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ELibrary.Models;

namespace ELibrary.Models
{
	public class ReviewsListViewModel
	{
		public IEnumerable<Comment> Reviews { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}
}