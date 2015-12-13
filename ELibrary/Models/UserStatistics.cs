using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;

namespace ELibrary.Models
{
	public class UserStatistics
	{
		public User User { get; set; }

		public Dictionary<int, Info> MonthInfo { get; set; }


		public static List<UserStatistics> GetAllStatistics()
		{
			var result = new List<UserStatistics>();

			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				// start transaction
				connection.Open();

				// read data
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText = @"select ID, NAME, SURNAME, EMAIL, PASSWORD, PROFILEPICTURE, DATEOFBIRTH, ACCOUNTTYPEID, GENDER from ""USER""";
					cmd.Connection = connection;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var temp = new UserStatistics()
							{
								User = new User()
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
								},
								MonthInfo = new Dictionary<int, Info>()
							};
							for (int i = 1; i <= 12; i++)
							{
								temp.MonthInfo.Add(i, new Info() { PurchasesCount = 0, ReviewsCount = 0 });
							}
							result.Add(temp);
						}
					}

				}

				foreach (var r in result)
				{
					using (SqlCommand cmd = getMonthValues("Purchase", r.User.Id))
					{
						cmd.Connection = connection;
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								r.MonthInfo[reader.GetInt32(0)].PurchasesCount = reader.GetInt32(1);
							}
						}
					}
					using (SqlCommand cmd = getMonthValues("Review", r.User.Id))
					{
						cmd.Connection = connection;
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								r.MonthInfo[reader.GetInt32(0)].ReviewsCount = reader.GetInt32(1);
							}
						}
					}
				}

				// commit transaction
				connection.Close();
			}

			return result;
		}

		private static SqlCommand getMonthValues(string table, int userId)
		{
			var result = new SqlCommand();
			result.CommandText = string.Format(@"select month(r.DateStamp) as MonthValue, count(*) from {0} r
										where year(r.DateStamp) = year(getdate()) and UserId = @userId group by month(r.DateStamp)", table);
			result.Parameters.AddWithValue("userId", userId);
			return result;
		}


		public class Info
		{
			public int ReviewsCount { get; set; }
			public int PurchasesCount { get; set; }
		}
	}
}