using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;

namespace ELibrary.Models
{
	public class Review
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Please enter your review text")]
		public string Text { get; set; }

		public int Mark { get; set; }

		public int BookId { get; set; }

		public int UserId { get; set; }

		public DateTime DateStamp { get; set; }


		public void Write(int userId)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = string.Format(@"insert Review (Text, DateStamp, Mark, UserId, BookId) values (@textValue, @dateStamp, @markValue, @userId, @bookId)");
					cmd.Parameters.AddRange(new[] {
						new SqlParameter("textValue", Text),
						new SqlParameter("dateStamp", DateTime.Now),
						new SqlParameter("markValue", Mark),
						new SqlParameter("userId", userId),
						new SqlParameter("bookId", BookId)
					});
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		public static List<Review> GetByBookId(int bookId)
		{
			var result = new List<Review>(); ;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = "select Id, Text, DateStamp, Mark, UserId, BookId from Review where BookId = @bookIdValue";
					cmd.Connection = connection;
					cmd.Parameters.AddWithValue("bookIdValue", bookId);
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						result.Add(new Review()
						{
							Id = reader.GetInt32(0),
							Text = reader.GetString(1),
							DateStamp = reader.GetDateTime(2),
							Mark = reader.GetInt32(3),
							UserId = reader.GetInt32(4),
							BookId = reader.GetInt32(5)
						});
					}
				}
				connection.Close();
			}
			return result;
		}

		public static Review GetById(int id)
		{
			Review result;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = "select Id, Text, DateStamp, Mark, UserId, BookId from Review where id = @idValue";
					cmd.Connection = connection;
					cmd.Parameters.AddWithValue("idValue", id);
					var reader = cmd.ExecuteReader();
					if (!reader.HasRows)
					{
						return null;
					}
					reader.Read();
					result = new Review()
					{
						Id = reader.GetInt32(0),
						Text = reader.GetString(1),
						DateStamp = reader.GetDateTime(2),
						Mark = reader.GetInt32(3),
						UserId = reader.GetInt32(4),
						BookId = reader.GetInt32(5)
					};
				}
				connection.Close();
			}
			return result;
		}

		public static void DeleteReview(int id)
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = "delete Review where Id = @idValue";
					cmd.Connection = connection;
					cmd.Parameters.AddWithValue("idValue", id);
					cmd.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		public string GetMarkColor()
		{
			switch (Mark)
			{
				case 1: return "#ff1200";
				case 2: return "#ff9c00";
				case 3: return "#fcff00";
				case 4: return "#c6ff00";
				case 5: return "#4eff00";
			}
			return "black";
		}
	}
}