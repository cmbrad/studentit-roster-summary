using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace StudentIT.Roster.Summary
{
    internal class InternProvider
    {
        private string _databaseLocation = Path.Combine(Path.GetTempPath(), "roster.db");

        public void Init()
        {
            CreateTables();
        }

        public void CreateTables()
        {
            Console.WriteLine($"[INFO] Creating tables in database at {_databaseLocation}");
            using (var connection = new SqliteConnection($"Data Source={_databaseLocation}"))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Intern (
                    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    name TEXT NOT NULL,
                    email TEXT NOT NULL
                    );";

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddIntern(string name, string email)
        {
            Console.WriteLine($"[INFO] Adding intern {name} / {email}");

            using (var connection = new SqliteConnection($"Data Source={_databaseLocation}"))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Intern (name, email) VALUES ($name, $email);";
                    command.Parameters.AddWithValue("$name", name);
                    command.Parameters.AddWithValue("email", email);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool TryGetNameFromEmail(string email, out string name)
        {
            Console.WriteLine($"[INFO] Getting name for {email}");
                      
            using (var connection = new SqliteConnection($"Data Source={_databaseLocation}"))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM Intern WHERE email = $email";
                    command.Parameters.AddWithValue("email", email);

                    connection.Open();
                    command.ExecuteNonQuery();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            name = reader.GetString(0);
                            return true;
                        }
                    }
                }
            }

            name = null;
            return false;
        }
    }
}
