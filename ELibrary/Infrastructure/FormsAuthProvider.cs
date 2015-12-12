using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using ELibrary.Infrastructure;
using System.Security.Principal;
using ELibrary.Models;

namespace ELibrary.Infrastructure
{
	public class FormsAuthProvider
	{
		public static bool Authenticate(string email, string password)
		{
			if (User.ValidateUser(email, password))
			{
				FormsAuthentication.RedirectFromLoginPage(email, false);
				FormsAuthentication.SetAuthCookie(email, false);
				return true;
			}
			return false;
		}
	}
}