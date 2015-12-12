using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;

namespace ELibrary.Models
{
	public class BookStatistics
	{
		public Book BookInstance { get; set; }

		public int PurchaseCount { get; set; }


		public static List<BookStatistics> GetAllStatistics()
		{
			var result = new List<BookStatistics>();
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				// read data
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = "select b.Id, max(b.Title), max(b.Author), max(b.ImageUrl), max(iif(b.Published = 'True', 1, 0)), max(b.ContentUrl), max(b.MinAccountTypeId), count(*) as CountValue from Book b inner join Purchase p on b.Id = p.BookId group by b.Id order by CountValue desc";
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(new BookStatistics()
							{
								BookInstance = new Book() {
									Id = reader.GetInt32(0),
									Title = reader.GetString(1),
									Author = reader.GetString(2),
									Image = reader.GetString(3),
									Published = reader.GetInt32(4) == 1 ? true : false,
									ContentUrl = reader.GetString(5),
									MinAccountType = (Book.BookAccountType)reader.GetInt32(6)
								},
								PurchaseCount = reader.GetInt32(7)
							});
						}
					}

				}
				
				connection.Close();
			}
			return result;
		}
	}
}