using System;
using ELibrary.Models;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELibrary.Controllers
{
	[Authorize]
	public class AdminController : Controller
	{
		public ActionResult Index()
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			var arrows = new Dictionary<string, string>();
			arrows["order"] = arrows["title"] = arrows["author"] = "/Content/Images/space.png";
			var desc = new Dictionary<string, string>();
			desc["order"] = desc["title"] = desc["author"] = "";
			var orderBy = (Request.Params["orderBy"] as string) ?? "Id";
			bool descending = false;
			if (orderBy == "title")
			{
				if (string.IsNullOrEmpty(Request.Params["desc"] as string))
				{
					arrows["title"] = "/Content/Images/arrowDown64.png";
					desc["title"] = "desc=true";
				}
				else
				{
					arrows["title"] = "/Content/Images/arrow64.png";
					descending = true;
				}
			}
			else if (orderBy == "author")
			{
				if (string.IsNullOrEmpty(Request.Params["desc"] as string))
				{
					arrows["author"] = "/Content/Images/arrowDown64.png";
					desc["author"] = "desc=true";
				}
				else
				{
					arrows["author"] = "/Content/Images/arrow64.png";
					descending = true;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(Request.Params["desc"] as string))
				{
					arrows["order"] = "/Content/Images/arrowDown64.png";
					desc["order"] = "desc=true";
				}
				else
				{
					arrows["order"] = "/Content/Images/arrow64.png";
					descending = true;
				}
			}

			ViewBag.Arrows = arrows;
			ViewBag.Desc = desc;

			var books = Book.GetAllBooks(orderBy, descending);
			return View(books);
		}

		[HttpGet]
		public ActionResult Edit()
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			Book result = null;
			var bookNumber = RouteData.Values["id"] as string;

			if (bookNumber != null)
			{
				result = Book.GetBook(int.Parse(bookNumber));
			}
			if (result == null)
			{
				return View("NotFound");
			}
			return View(result);
		}

		[HttpPost]
		public ActionResult Edit(Book book)
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			if (ModelState.IsValid)
			{
				if (book.Price < 0)
				{
					ModelState.AddModelError("Price", "Price can't be lower than 0");
					return View(book);
				}
				if (string.IsNullOrEmpty(book.Image))
				{
					book.Image = "http://www.clipartpal.com/_thumbs/pd/book_blue.png";
				}
				if (book.Id < 0)
				{
					Book.CreateBook(book);
				}
				else
				{
					Book.UpdateBook(book.Id, book);
				}
				TempData["message"] = string.Format("{0} ({1}) has been successfully saved", book.Title, book.Id);
				return RedirectToAction("Index");
			}
			return View(book);
		}

		public ActionResult Create()
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			return View("Edit", new Book() { Id = -1 });
		}

		public ActionResult Delete()
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			var bookNumber = RouteData.Values["id"] as string;
			if (bookNumber == null)
			{
				return View("NotFound");
			}
			var curr = Book.GetBook(int.Parse(bookNumber));
			bool status = Book.DeleteBook(curr.Id);
			if (status)
			{
				TempData["message"] = string.Format("{0} was successfully deleted", curr.Title);
			}
			return RedirectToAction("Index");
		}

		public ActionResult AccessDenied()
		{
			return View();
		}

		public ActionResult PublishRange()
		{
			var modeStr = (Request.Params["mode"] as string) ?? "publish";
			var mode = modeStr == "publish" ? true : false;
			var listStr = (Request.Params["ids"] as string);

			if (string.IsNullOrEmpty(listStr))
			{
				TempData["message"] = "No items were selected";
			}
			else
			{
				try
				{
					var listStrSplited = listStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					var list = Array.ConvertAll(listStrSplited, s => int.Parse(s));
					Book.PublishBooks(mode, list);
				}
				catch(Exception) {
					TempData["message"] = "Error has occured. No items were updated";
				}
			}

			return RedirectToAction("Index");
		}

		public ActionResult RemoveRange()
		{
			var listStr = (Request.Params["ids"] as string);

			if (string.IsNullOrEmpty(listStr))
			{
				TempData["message"] = "No items were selected";
			}
			else
			{
				try
				{
					var listStrSplited = listStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					var list = Array.ConvertAll(listStrSplited, s => int.Parse(s));
					Book.DeleteBooks(list);
				}
				catch (Exception)
				{
					TempData["message"] = "Error has occured. No items were deleted";
				}
			}

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult Reviews()
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			return View(Comment.GetAllFeedbacks());
		}

		[HttpPost]
		public ActionResult Reviews(IEnumerable<Comment> comments)
		{
			try
			{
				Comment.UpdateReplies(comments);
				TempData["message"] = "Replies were successfully saved";
			}
			catch(Exception)
			{
				TempData["message"] = "An error has occured. Replies were not successfully saved";
			}
			return View(Comment.GetAllFeedbacks());
		}

		public ActionResult DeleteReview(int id)
		{
			if (ELibrary.Models.User.GetByEmail(User.Identity.Name).AccountType != AccountType.Admin)
			{
				return RedirectToAction("AccessDenied");
			}

			var res = Review.GetById(id);
			Review.DeleteReview(id);
			return Redirect(Url.Action(res == null ? "Index" : string.Format("Book/{0}", res.BookId), "Home"));
		}

		public ActionResult Statistics()
		{
			return View();
		}
	}
}