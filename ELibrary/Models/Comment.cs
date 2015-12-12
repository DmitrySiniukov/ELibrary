using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace ELibrary.Models
{
	/// <summary>
	/// User information
	/// </summary>
	public class Comment
	{
		/// <summary>
		/// Comment id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Feedback
		/// </summary>
		[Required(ErrorMessage = "Please enter your recall")]
		public string Text { get; set; }

		/// <summary>
		/// Review date
		/// </summary>
		public DateTime DateStamp { get; set; }

		/// <summary>
		/// Mark
		/// </summary>
		public int Mark { get; set; }

		/// <summary>
		/// reply
		/// </summary>
		public string AdminReply { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		public int UserId { get; set; }


		/// <summary>
		/// Load a feedback instance to database
		/// </summary>
		/// <returns></returns>
		public bool LoadFeedbackToDb()
		{
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand())
					{
						cmd.CommandText = string.Format(@"insert Comment (TEXT, DATESTAMP, MARK, ADMINREPLY, USERID) values
						(@text, @datestamp, @mark, @adminReply, @userId)");
						cmd.Parameters.AddRange(new[] {
							new SqlParameter("text", Text),
							new SqlParameter("datestamp", DateStamp),
							new SqlParameter("mark", Mark),
							new SqlParameter("adminReply", AdminReply ?? string.Empty),
							new SqlParameter("userId", UserId)
						});
						cmd.Connection = connection;
						cmd.ExecuteNonQuery();
					}
					connection.Close();
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static List<Comment> GetAllFeedbacks()
		{
			var result = new List<Comment>(); ;
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.CommandText =
					@"select ID, TEXT, DATESTAMP, MARK, ADMINREPLY, USERID, IIF(ADMINREPLY IS NULL OR ADMINREPLY like ' ', 0, 1) as EmptyReply from Comment order by EmptyReply, DATESTAMP";
					cmd.Connection = connection;
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						result.Add(new Comment()
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
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="comments"></param>
		public static void UpdateReplies(IEnumerable<Comment> comments)
		{
			var stringBuilder = new StringBuilder();
			const string connectionString = @"data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\ELibrary.mdf;integrated security=True;MultipleActiveResultSets=True;";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand())
				{
					stringBuilder.AppendLine("BEGIN TRANSACTION UpdateReplies");
					int i = 0;
					foreach (var c in comments)
					{
						++i;
						stringBuilder.AppendLine(string.Format("update COMMENT set AdminReply = @reply{0} where Id = @id{0}", i));
						cmd.Parameters.AddRange(new[] {
							new SqlParameter(string.Format("reply{0}", i), c.AdminReply ?? string.Empty),
							new SqlParameter(string.Format("id{0}", i), c.Id)
						});
					}
					stringBuilder.AppendLine("COMMIT TRANSACTION UpdateReplies");
					cmd.CommandText = stringBuilder.ToString();
					cmd.Connection = connection;
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