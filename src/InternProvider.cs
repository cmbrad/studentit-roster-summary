using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace StudentIT.Roster.Summary
{
    internal class InternProvider
    {
        private readonly string _databaseLocation = Path.Combine("extra", "people.db");

        public string NameFromEmail(string email)
        {
            Console.WriteLine($"Getting name for {email}");
                      
            using (var connection = new SqliteConnection($"Data Source={_databaseLocation}"))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM Employees WHERE email = $email";
                    command.Parameters.AddWithValue("email", email);

                    connection.Open();
                    command.ExecuteNonQuery();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }
            return null;
        }
    }
}
