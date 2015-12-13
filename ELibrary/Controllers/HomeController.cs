using ELibrary.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using ELibrary.Infrastructure;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ELibrary.Controllers
{
	/// <summary>
	/// Main controller
	/// </summary>
	public class HomeController : Controller
	{
		/// <summary>
		/// Home page
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Index()
		{
			List<Book> list;
			try
			{
				//list = ELibrary.Models.Book.GetPublishedBooks((User != null && User.Identity.IsAuthenticated) ? "1=1" : "BOOK.ONLYAUTH = 'False'");
				list = ELibrary.Models.Book.GetPublishedBooks();
			}
			catch (Exception)
			{
				return View(new List<Book>());
			}
			return View(list);
		}

		/// <summary>
		/// Home page
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Comment()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account", new { ReturnUrl = "/Home/Comment/" });
			}
			return View();
		}

		/// <summary>
		/// Home page
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Comment(Comment comment)
		{
			var currentUserInfo = ELibrary.Models.User.GetByEmail(User.Identity.Name);
			if (ModelState.IsValid)
			{
				comment.UserId = currentUserInfo.Id;
				comment.DateStamp = DateTime.Now;
				if (comment.LoadFeedbackToDb())
				{
					var name = currentUserInfo.Name;
					var surname = currentUserInfo.Surname;
					ViewBag.Message = string.Format("Thank you, {0} {1}, for your feedback!",
					name.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper() + name.Substring(1, name.Length - 1),
					surname.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper() + surname.Substring(1, surname.Length - 1));
				}
				else
				{
					ViewBag.Message = "Unfortunately, a problem occured... Maybe same email already exists. Try one more time, please.";
				}
				return View("FeedbackResult");
			}
			return View(comment);
		}

		/// <summary>
		/// Book description page
		/// </summary>
		/// <returns></returns>
		public ActionResult Book()
		{
			var bookNumberStr = RouteData.Values["id"] as string;
			Book result = null;
			if (bookNumberStr != null)
			{
				int bookNumber;
				if (!int.TryParse(bookNumberStr, out bookNumber))
				{
					return View("NotFound");
				}
				result = ELibrary.Models.Book.GetBook(bookNumber);
			}
			if (result == null)
			{
				return View("NotFound");
			}

			ViewBag.Access = isAccessed(result);
			
			return View(result);
		}

		/// <summary>
		/// Not found page for 404, 403 errors
		/// </summary>
		/// <returns></returns>
		public ActionResult NotFound()
		{
			return View();
		}

		/// <summary>
		/// Not found page for 404, 403 errors
		/// </summary>
		/// <returns></returns>
		public ActionResult Error()
		{
			return View();
		}

		/// <summary>
		/// User reviews
		/// </summary>
		/// <param name="page">Page number</param>
		/// <returns></returns>
		public ActionResult Reviews(int page)
		{
			if (page < 1)
			{
				page = 1;
			}

			const int rowsPerPage = 3;
			int rowNumber;
			int lastRows;
			var list = new List<Comment>();
			//var condition = (User.Identity.IsAuthenticated && ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType == ELibrary.Models.AccountType.Admin) ? "1=1" : "AdminReply IS NOT NULL AND NOT AdminReply = ' '";
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText =
						string.Format(@"select count(*) as NUMBER from COMMENT");
						cmd.Connection = connection;
						rowNumber = (int)cmd.ExecuteScalar();
						if (page > Math.Ceiling(rowNumber / (double)rowsPerPage))
						{
							page = (int)Math.Ceiling(rowNumber / (double)rowsPerPage);
						}
						lastRows = Math.Min(rowNumber - (page - 1) * rowsPerPage, rowsPerPage);
					}
					connection.Close();
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText = string.Format(
						@"select * from (
							select top(@top2) ID, TEXT, DATESTAMP, MARK, ADMINREPLY, USERID
							from
								(select top(@top1) COMMENT.*
								from COMMENT
								order by DATESTAMP desc) as T
						order by DATESTAMP) Res order by DATESTAMP DESC");
						cmd.Parameters.AddWithValue("top1", page * rowsPerPage);
						cmd.Parameters.AddWithValue("top2", lastRows);
						cmd.Connection = connection;
						var reader = cmd.ExecuteReader();
						while (reader.Read())
						{
							list.Add(new Comment()
							{
								Id = reader.GetInt32(0),
								Text = reader.GetString(1),
								DateStamp = reader.GetDateTime(2),
								Mark = reader.GetInt32(3),
								AdminReply = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
								UserId = reader.GetInt32(5)
							});
						}
					}
					connection.Close();
				}
			}
			catch (Exception)
			{
				return View(new ReviewsListViewModel() { PagingInfo = new PagingInfo() { CurrentPage = 1, ItemsPerPage = rowsPerPage, TotalItems = 0 }, Reviews = new List<Comment>() });
			}
			var result = new ReviewsListViewModel() { PagingInfo = new PagingInfo() { CurrentPage = page, ItemsPerPage = rowsPerPage, TotalItems = rowNumber }, Reviews = list };
			return View(result);
		}

		/// <summary>
		/// Purchase a book
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Authorize]
		public ActionResult Purchase(int id)
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("NotFound");
			}

			var result = ELibrary.Models.Book.GetBook(id);
			if (result == null)
			{
				return View("NotFound");
			}

			var user = ELibrary.Models.User.GetByEmail(User.Identity.Name);

			if (ELibrary.Models.User.IsPremiumPeriod(user.Id))
			{
				return Redirect(Url.Action(string.Format("Book/{0}", id), "Home"));
			}

			return View(result);
		}

		/// <summary>
		/// Purchase a book
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Authorize]
		public ActionResult Purchase(FormCollection values)
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("NotFound");
			}
			int id;
			decimal resultSum;
			Book book;
			try
			{
				var idStr = values["Id"];
				id = int.Parse(idStr);
				book = ELibrary.Models.Book.GetBook(id);
				if (book == null)
				{
					return View("NotFound");
				}
				var sumStr = values["Sum"];
				if (!decimal.TryParse(sumStr, out resultSum))
				{
					TempData["message"] = "Specify the real value for purchase sum";
					return Redirect(Url.Action(string.Format("Purchase/{0}", id), "Home"));
				}
				if (resultSum < 10m)
				{
					TempData["message"] = "Specify the value > 10$";
					return Redirect(Url.Action(string.Format("Purchase/{0}", id), "Home"));
				}
			}
			catch (Exception e)
			{
				ModelState.AddModelError("Id", "Something went wrong. We are sorry...");
				return View();
			}

			var user = Models.User.GetByEmail(User.Identity.Name);
			book.Purchase(user.Id);

			return Redirect(Url.Action(string.Format("Book/{0}", id), "Home"));
		}

		[HttpGet]
		[Authorize]
		public ActionResult AddReview(int id)
		{
			var book = ELibrary.Models.Book.GetBook(id);
			if (book == null)
			{
				return View("NotFound");
			}
			if (!isAccessed(book))
			{
				return Redirect(Url.Action(string.Format("Purchase/{0}", id), "Home"));
			}
			return View(new Review() { BookId = id });
		}

		[HttpPost]
		[Authorize]
		public ActionResult AddReview(Review review)
		{
			var book = ELibrary.Models.Book.GetBook(review.BookId);
			if (book == null)
			{
				return View("NotFound");
			}
			if (!isAccessed(book))
			{
				return Redirect(Url.Action(string.Format("Purchase/{0}", book.Id), "Home"));
			}

			if (ModelState.IsValid)
			{
				review.Write(Models.User.GetByEmail(User.Identity.Name).Id);
				return Redirect(Url.Action(string.Format("Book/{0}", book.Id), "Home"));
			}
			return View(review);
		}

		private bool isAccessed(Book book)
		{
			if (User.Identity.IsAuthenticated)
			{
				var user = Models.User.GetByEmail(User.Identity.Name);
				return book.MinAccountType == Models.Book.BookAccountType.Regular || Models.User.IsPremiumPeriod(user.Id) || book.IsPurchased(user.Id);
			}
			return false;
		}
	}
}