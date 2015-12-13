using System;
using System.Collections.Generic;
using ELibrary.Models;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ELibrary.Models
{
	/// <summary>
	/// Book description
	/// </summary>
	public class Book
	{
		/// <summary>
		/// Order number
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Book title
		/// </summary>
		[DataType(DataType.Text)]
		[Required(ErrorMessage = "Please enter a book title")]
		public string Title { get; set; }

		/// <summary>
		/// Book description
		/// </summary>
		[DataType(DataType.MultilineText)]
		[AllowHtml]
		[Required(ErrorMessage = "Please enter a book description")]
		public string Description { get; set; }

		/// <summary>
		/// Author name
		/// </summary>
		[Required(ErrorMessage = "Please enter author's name")]
		public string Author { get; set; }

		/// <summary>
		/// Image url
		/// </summary>
		[Required(ErrorMessage = "Please enter the url of book image")]
		[DataType(DataType.ImageUrl)]
		public string Image { get; set; }

		/// <summary>
		/// Published
		/// </summary>
		public bool Published { get; set; }

		/// <summary>
		/// Only for authenticated
		/// </summary>
		[Required(ErrorMessage = "Please enter accessibility type")]
		public BookAccountType MinAccountType { get; set; }

		/// <summary>
		/// Content url
		/// </summary>
		[Required(ErrorMessage = "Please enter the url of content")]
		[DataType(DataType.Url)]
		public string ContentUrl { get; set; }

		[DataType(DataType.Currency)]
		public float Price { get; set; }


		/// <summary>
		/// Get book by order
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Book GetBook(int id)
		{
			Book result = null;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select BOOK.Id, BOOK.Title, BOOK.Description, BOOK.ImageUrl, BOOK.Author, BOOK.Published, BOOK.MinAccountTypeId, BOOK.ContentUrl, BOOK.Price from BOOK where BOOK.Id = @idValue";
					cmd.Parameters.AddWithValue("idValue", id);
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							result = new Book()
							{
								Id = reader.GetInt32(0),
								Title = reader.GetString(1),
								Description = reader.GetString(2),
								Image = reader.GetString(3),
								Author = reader.GetString(4),
								Published = reader.GetBoolean(5),
								MinAccountType = (BookAccountType)reader.GetInt32(6),
								ContentUrl = reader.GetString(7),
								Price = (float)reader.GetDouble(8)
							};
						}
					}
				}
				connection.Close();
			}
			return result;
		}

		/// <summary>
		/// Retrieve all the books
		/// </summary>
		/// <returns></returns>
		public static List<Book> GetAllBooks(string orderBy, bool desc)
		{
			var resultList = new List<Book>();
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText = string.Format(@"select BOOK.Id, BOOK.Title, BOOK.Description, BOOK.ImageUrl, BOOK.Author, BOOK.Published, BOOK.MinAccountTypeId, BOOK.ContentUrl, BOOK.Price from BOOK order by BOOK.{0} {1}",
							string.IsNullOrEmpty(orderBy) ? "Id" : orderBy, desc ? "desc" : "asc");
						cmd.Connection = connection;
						var reader = cmd.ExecuteReader();
						while (reader.Read())
						{
							try
							{
								resultList.Add(new Book()
								{
									Id = reader.GetInt32(0),
									Title = reader.GetString(1),
									Description = reader.GetString(2),
									Image = reader.GetString(3),
									Author = reader.GetString(4),
									Published = reader.GetBoolean(5),
									MinAccountType = (BookAccountType)reader.GetInt32(6),
									ContentUrl = reader.GetString(7),
									Price = (float)reader.GetDouble(8)
								});
							}
							catch (Exception)
							{
							}
						}
					}
					connection.Close();
				}
			}
			return resultList;
		}

		public static List<Book> GetPublishedBooks()
		{
			var result = new List<Book>();
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = string.Format(@"select BOOK.Id, BOOK.Title, BOOK.Description, BOOK.ImageUrl, BOOK.Author, BOOK.Published, BOOK.MinAccountTypeId, BOOK.ContentUrl, BOOK.Price from BOOK where BOOK.PUBLISHED = 'True' order by Id");
					cmd.Connection = connection;
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						result.Add(new Book()
						{
							Id = reader.GetInt32(0),
							Title = reader.GetString(1),
							Description = reader.GetString(2),
							Image = reader.GetString(3),
							Author = reader.GetString(4),
							Published = reader.GetBoolean(5),
							MinAccountType = (BookAccountType)reader.GetInt32(6),
							ContentUrl = reader.GetString(7),
							Price = (float)reader.GetDouble(8)
						});
					}
				}
				connection.Close();
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="order"></param>
		/// <param name="newItem"></param>
		public static void UpdateBook(int order, Book newItem)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText = @"update BOOK set BOOK.Title = @title, BOOK.Description = @description, BOOK.ImageUrl = @imageUrl, BOOK.Author = @author, BOOK.Published = @published, BOOK.MinAccountTypeId = @minAcc, BOOK.ContentUrl = @content, BOOK.Price = @price where BOOK.Id = @idValue";
						cmd.Connection = connection;
						cmd.Parameters.AddWithValue("title", newItem.Title);
						cmd.Parameters.AddWithValue("description", newItem.Description);
						cmd.Parameters.AddWithValue("imageUrl", newItem.Image);
						cmd.Parameters.AddWithValue("author", newItem.Author);
						cmd.Parameters.AddWithValue("idValue", newItem.Id);
						cmd.Parameters.AddWithValue("published", newItem.Published);
						cmd.Parameters.AddWithValue("minAcc", (int)newItem.MinAccountType);
						cmd.Parameters.AddWithValue("content", newItem.ContentUrl);
						cmd.Parameters.AddWithValue("price", newItem.Price);
						cmd.ExecuteNonQuery();
					}
					connection.Close();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public static void CreateBook(Book item)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText = @"insert BOOK (Title, Description, ImageUrl, Author, Published, MinAccountTypeId, ContentUrl, Price) values (@title, @description, @imageUrl, @author, @published, @minAcc, @content, @price)";
						cmd.Connection = connection;
						cmd.Parameters.AddWithValue("title", item.Title);
						cmd.Parameters.AddWithValue("description", item.Description);
						cmd.Parameters.AddWithValue("imageUrl", item.Image);
						cmd.Parameters.AddWithValue("author", item.Author);
						cmd.Parameters.AddWithValue("published", item.Published);
						cmd.Parameters.AddWithValue("minAcc", (int)item.MinAccountType);
						cmd.Parameters.AddWithValue("content", item.ContentUrl);
						cmd.Parameters.AddWithValue("price", item.Price);
						cmd.ExecuteNonQuery();
					}
					connection.Close();
				}
			}
		}

		/// <summary>
		/// publish book
		/// </summary>
		/// <param name="mode"></param>
		public static void PublishBooks(bool mode, int[] ids)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.Connection = connection;
						var parameters = new string[ids.Length];
						for (int i = 0; i < parameters.Length; i++)
						{
							parameters[i] = string.Format("@order{0}", i);
							cmd.Parameters.AddWithValue(parameters[i], ids[i]);
						}
						cmd.CommandText = string.Format(@"update BOOK set BOOK.Published = @mode where BOOK.Id in ({0})", string.Join(", ", parameters));
						cmd.Parameters.AddWithValue("mode", mode);
						cmd.ExecuteNonQuery();
					}
					connection.Close();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool DeleteBook(int id)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			bool result;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"delete Book where Book.Id = @idValue";
					cmd.Parameters.AddWithValue("idValue", id);
					cmd.Connection = connection;
					result = (cmd.ExecuteNonQuery() > 0);
				}
				connection.Close();
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		public static void DeleteBooks(int[] ids)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					var parameters = new string[ids.Length];
					for (int i = 0; i < parameters.Length; i++)
					{
						parameters[i] = string.Format("@order{0}", i);
						cmd.Parameters.AddWithValue(parameters[i], ids[i]);
					}
					cmd.CommandText = string.Format(@"delete Book where Book.Id in ({0})", string.Join(", ", parameters));
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Is purchased
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool IsPurchased(int userId)
		{
			bool result;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select 1 from Purchase where UserId = @userId and BookId = @bookId";
					cmd.Parameters.AddWithValue("userId", userId);
					cmd.Parameters.AddWithValue("bookId", Id);
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

		public bool Purchase(int userId)
		{
			bool result;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"insert Purchase (UserId, BookId, DateStamp) values (@userId, @bookId, @dateStamp)";
					cmd.Parameters.AddWithValue("userId", userId);
					cmd.Parameters.AddWithValue("bookId", Id);
					cmd.Parameters.AddWithValue("dateStamp", DateTime.Now);
					cmd.Connection = connection;
					result = cmd.ExecuteNonQuery() > 0;
				}
				connection.Close();
			}
			return result;
		}

		public static List<Book> GetAccessibleBooks(User user)
		{
			var list = GetPublishedBooks();
			List<Book> result = new List<Book>();
			foreach (var b in list)
			{
				if (b.MinAccountType == BookAccountType.Regular || User.IsPremiumPeriod(user.Id) || b.IsPurchased(user.Id))
				{
					result.Add(b);
				}
			}
			return result;
		}


		/// <summary>
		/// Groups of users
		/// </summary>
		public enum BookAccountType
		{
			[Display(Name = "For regular users")]
			Regular = 1,
			[Display(Name = "Only for premium users or after purchase")]
			Premium = 2
		}
	}
}