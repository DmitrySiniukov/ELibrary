using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;

namespace ELibrary.Models
{
	public class PremiumPeriod
	{
		[Required(ErrorMessage = "Please enter your date from")]
		public DateTime DateFrom { get; set; }

		[Required(ErrorMessage = "Please enter your date to")]
		public DateTime DateTo { get; set; }
	}
}