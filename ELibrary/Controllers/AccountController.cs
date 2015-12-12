using System.Web.Mvc;
using ELibrary.Models;
using ELibrary.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ELibrary.Controllers
{
	public class AccountController : Controller
	{

		public ViewResult Login()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				if (!FormsAuthProvider.Authenticate(model.Email, model.Password))
				{
					ModelState.AddModelError("", "Incorrect username or password");
					return View();
				}
				var target = Request.Params["ReturnUrl"] as string;
				if (string.IsNullOrEmpty(target))
				{
					return Redirect(Url.Action("Index", "Home"));
				}
				return Redirect(target);
			}
			else
			{
				return View();
			}
		}

		[HttpGet]
		public ActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Register(User user)
		{
			if (ModelState.IsValid)
			{
				if (user.DateOfBirth > DateTime.Now.AddYears(-10))
				{
					ModelState.AddModelError("DateOfBirth", "User must be at least 10 years old.");
					return View();
				}
				if (user.DateOfBirth < new DateTime(1900, 1, 1))
				{
					ModelState.AddModelError("DateOfBirth", "Sorry, you are too old for this site :)");
					return View();
				}
				if (ELibrary.Models.User.ValidateNewUser(user))
				{
					if (user.ProfilePictureFile != null)
					{
						if (!user.ProfilePictureFile.ContentType.Contains("image"))
						{
							ModelState.AddModelError("ProfilePictureFile", "Specify an image file.");
							return View();
						}
						var path = string.Concat("/Content/Images/", user.ProfilePictureFile.FileName);
						user.ProfilePictureFile.SaveAs(Server.MapPath(path));
						user.ProfilePicturePath = path;
					}
					ELibrary.Models.User.AddNewUser(user);
					return Redirect(Url.Action("Index", "Home"));
				}
				ModelState.AddModelError("Email", "Such email already exists");
			}
			return View();
		}

		public ActionResult SignOut()
		{
			FormsAuthentication.SignOut();
			Session.Abandon();

			// clear authentication cookie
			HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
			cookie1.Expires = DateTime.Now.AddYears(-1);
			Response.Cookies.Add(cookie1);

			// clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
			HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
			cookie2.Expires = DateTime.Now.AddYears(-1);
			Response.Cookies.Add(cookie2);

			return Redirect(Url.Action("Login", "Account"));
		}

		[HttpGet]
		public ActionResult PersonalArea()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("NotFound");
			}
			var user = ELibrary.Models.User.GetByEmail(User.Identity.Name);
			ViewBag.ImageUrl = string.IsNullOrEmpty(user.ProfilePicturePath)
				? ((bool)user.Gender ? "/Content/Images/female.jpg" : "/Content/Images/male.jpg")
				: user.ProfilePicturePath;
			return View(user);
		}
		
		[HttpPost]
		public ActionResult PersonalArea(User user)
		{
			if (ModelState.IsValid)
			{
				if (ELibrary.Models.User.ValidateNewUser(user) || ELibrary.Models.User.GetByEmail(user.Email).Id == user.Id)
				{
					if (user.ProfilePictureFile != null)
					{
						if (!user.ProfilePictureFile.ContentType.Contains("image"))
						{
							ModelState.AddModelError("ProfilePictureFile", "Specify an image file.");
							var userResult = ELibrary.Models.User.GetByEmail(User.Identity.Name);
							ViewBag.ImageUrl = (bool)userResult.Gender ? "/Content/Images/female.jpg" : "/Content/Images/male.jpg";
							return View(user);
						}
						var path = string.Concat("/Content/Images/", user.ProfilePictureFile.FileName);
						user.ProfilePictureFile.SaveAs(Server.MapPath(path));
						user.ProfilePicturePath = path;
					}
					ELibrary.Models.User.UpdateUserInfo(user);
				}
				else
				{
					ModelState.AddModelError("Email", "Such email already exists");
				}
			}
			return PersonalArea();
		}

		[HttpGet]
		public ActionResult PremiumPeriod()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("NotFound");
			}
			var user = Models.User.GetByEmail(User.Identity.Name);

			if (Models.User.IsPremiumPeriod(user.Id))
			{
				return View("NotFound");
			}

			return View();
		}

		[HttpPost]
		public ActionResult PremiumPeriod(PremiumPeriod period)
		{
			if (!User.Identity.IsAuthenticated)
			{
				return View("NotFound");
			}
			var user = Models.User.GetByEmail(User.Identity.Name);

			if (Models.User.IsPremiumPeriod(user.Id))
			{
				return View("NotFound");
			}

			if (ModelState.IsValid)
			{
				if (period.DateFrom > period.DateTo)
				{
					ModelState.AddModelError("DateFrom", "The start date must be earlier than date to!");
					return View();
				}
				var now = DateTime.Now;
				if (period.DateFrom < new DateTime(now.Year, now.Month, now.Day))
				{
					ModelState.AddModelError("DateFrom", "The start date must be later than today");
					return View();
				}
				user.AddPremium(period);
			}
			else
			{
				return View();
			}
			return Redirect(Url.Action("PersonalArea", "Account"));
		}
	}
}