using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;

namespace ELibrary.Models
{
	/// <summary>
	/// User information
	/// </summary>
	public class User
	{
		/// <summary>
		/// user name backing
		/// </summary>
		private string _name;

		/// <summary>
		/// user surname backing
		/// </summary>
		private string _surname;

		/// <summary>
		/// User id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Email
		/// </summary>
		[Required(ErrorMessage = "Please enter your email address")]
		[RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
		public string Email { get; set; }

		/// <summary>
		/// Password
		/// </summary>
		[Required(ErrorMessage = "Please enter your password")]
		[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$",
			ErrorMessage = "Please enter a valid password (at least 6 chars with letters and numbers)")]
		public string Password { get; set; }

		/// <summary>
		/// User name
		/// </summary>
		[Required(ErrorMessage = "Please enter your name")]
		[RegularExpression("^([a-zA-Zа-яА-ЯіІїЇ' ]){2,50}", ErrorMessage = "Please enter a valid name")]
		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(_name))
				{
					return "Unknown_name";
				}
				return _name.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper() + _name.ToLower().Substring(1, _name.Length - 1);
			}
			set { _name = value; }
		}

		/// <summary>
		/// User surname
		/// </summary>
		[Required(ErrorMessage = "Please enter your surname")]
		[RegularExpression("^([a-zA-Zа-яА-ЯіІїЇ' ]){2,50}", ErrorMessage = "Please enter a valid surname")]
		public string Surname
		{
			get
			{
				if (string.IsNullOrEmpty(_surname))
				{
					return "Unknown_surname";
				}
				return _surname.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper() + _surname.ToLower().Substring(1, _surname.Length - 1);
			}
			set { _surname = value; }
		}

		/// <summary>
		/// User gender
		/// </summary>
		public bool? Gender { get; set; }

		/// <summary>
		/// Profile picture path
		/// </summary>
		public string ProfilePicturePath { get; set; }

		/// <summary>
		/// Profile picture file
		/// </summary>
		public HttpPostedFileBase ProfilePictureFile { get; set; }

		/// <summary>
		/// Date of birth
		/// </summary>
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		/// <summary>
		/// Account type id
		/// </summary>
		public AccountType AccountType { get; set; }


		public static bool ValidateNewUser(User user)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select 1 from ""User"" where ""User"".""Email"" = @email";
					cmd.Parameters.AddWithValue("email", user.Email);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							connection.Close();
							return false;
						}
					}
				}
				connection.Close();
			}
			return true;
		}

		public static void AddNewUser(User user)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"insert ""USER"" (NAME, SURNAME, EMAIL, PASSWORD, PROFILEPICTURE, DATEOFBIRTH, ACCOUNTTYPEID, GENDER) values
						(@name, @surname, @email, @password, @profilePicture, @dateOfBirth, @accountTypeId, @gender)";
					cmd.Parameters.AddRange(new[] {
							new SqlParameter("name", user.Name),
							new SqlParameter("surname", user.Surname),
							new SqlParameter("email", user.Email),
							new SqlParameter("password", user.Password),
							new SqlParameter("profilePicture", string.IsNullOrEmpty(user.ProfilePicturePath) ? (object)DBNull.Value : user.ProfilePicturePath),
							new SqlParameter("dateOfBirth", user.DateOfBirth),
							new SqlParameter("accountTypeId", 1),
							new SqlParameter("gender", user.Gender)
						});
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		public static bool ValidateUser(string email, string password)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select ""User"".""Password"" from ""User"" where ""User"".""Email"" = @email";
					cmd.Parameters.AddWithValue("email", email);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.HasRows)
						{
							connection.Close();
							return false;
						}
						reader.Read();
						if (reader.GetString(0) != password)
						{
							connection.Close();
							return false;
						}
					}
				}
				connection.Close();
			}
			return true;
		}

		public static User GetByEmail(string email)
		{
			User result = null;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select ID, NAME, SURNAME, EMAIL, PASSWORD, PROFILEPICTURE, DATEOFBIRTH, ACCOUNTTYPEID, GENDER from ""USER"" where ""USER"".""EMAIL"" = @email";
					cmd.Parameters.AddWithValue("email", email);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							result = new User() {
								Id = reader.GetInt32(0),
								Name = reader.GetString(1),
								Surname = reader.GetString(2),
								Email = reader.GetString(3),
								Password = reader.GetString(4),
								ProfilePicturePath = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
								DateOfBirth = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
								AccountType = (AccountType)reader.GetInt32(7),
								Gender = reader.GetBoolean(8)
							};
						}
					}
				}
				connection.Close();
			}
			return result;
		}

		public static User GetById(int id)
		{
			User result = null;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select ID, NAME, SURNAME, EMAIL, PASSWORD, PROFILEPICTURE, DATEOFBIRTH, ACCOUNTTYPEID, GENDER from ""USER"" where ""USER"".""ID"" = @id";
					cmd.Parameters.AddWithValue("id", id);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							result = new User()
							{
								Id = reader.GetInt32(0),
								Name = reader.GetString(1),
								Surname = reader.GetString(2),
								Email = reader.GetString(3),
								Password = reader.GetString(4),
								ProfilePicturePath = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
								DateOfBirth = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
								AccountType = (AccountType)reader.GetInt32(7),
								Gender = reader.GetBoolean(8)
							};
						}
					}
				}
				connection.Close();
			}
			return result;
		}

		public static void UpdateUserInfo(User user)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"update ""USER"" set NAME = @name, SURNAME = @surname, EMAIL = @email, PASSWORD = @password, PROFILEPICTURE = @profilePicture, DATEOFBIRTH = @dateOfBirth, GENDER = @gender where Id = @id";
					cmd.Parameters.AddRange(new[] {
							new SqlParameter("name", user.Name),
							new SqlParameter("surname", user.Surname),
							new SqlParameter("email", user.Email),
							new SqlParameter("password", user.Password),
							new SqlParameter("profilePicture", string.IsNullOrEmpty(user.ProfilePicturePath) ? (object)DBNull.Value : user.ProfilePicturePath),
							new SqlParameter("dateOfBirth", user.DateOfBirth),
							new SqlParameter("gender", user.Gender),
							new SqlParameter("id", user.Id)
						});
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Is premium period?
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static bool IsPremiumPeriod(int userId)
		{
			bool result;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select 1 from PremiumPeriod where UserId = @id and getdate() > DateFrom and getdate() < DateTo";
					cmd.Parameters.AddWithValue("id", userId);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						result = reader.HasRows;
					}
				}
				connection.Close();
			}
			return result;
		}

		public void AddPremium(PremiumPeriod period)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"insert PremiumPeriod (UserId, DateFrom, DateTo) values (@userId, @dateFrom, @dateTo)";
					cmd.Parameters.AddWithValue("userId", Id);
					cmd.Parameters.AddWithValue("dateFrom", period.DateFrom);
					cmd.Parameters.AddWithValue("dateTo", period.DateTo);
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}
	}


	/// <summary>
	/// Groups of users
	/// </summary>
	public enum AccountType
	{
		Regular = 1,
		Premium = 2,
		Admin = 3
	}
}