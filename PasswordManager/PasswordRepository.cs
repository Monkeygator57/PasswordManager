using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PasswordManager.Models;
using Microsoft.Data.Sqlite;

namespace PasswordManager.Data
{
    public interface IPasswordRepository
    {
        IList<PasswordEntry> GetAll();
        void Add(PasswordEntry passwordEntry);
        void Delete(int id);
    }

    public class PasswordRepository : IPasswordRepository
    {
        private readonly string _dbPath;

        public PasswordRepository(string dbPath)
        {
            _dbPath = dbPath;
        }

        public IList<PasswordEntry> GetAll()
        {
            var list = new List<PasswordEntry>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            const string query = @"SELECT ID, Website, Username, EncryptedPassword FROM passwords;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var entry = new PasswordEntry
                {
                    ID = reader.GetInt32(0),
                    Website = reader.GetString(1),
                    Username = reader.GetString(2),
                    EncryptedPassword = reader.GetString(3)
                };
                list.Add(entry);
            }

            return list;
        }

        public void Add(PasswordEntry entry)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            const string query = @"INSERT INTO passwords (Website, Username, EncryptedPassword) VALUES (@website, @username, @password);";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@website", entry.Website);
            command.Parameters.AddWithValue("@username", entry.Username);
            command.Parameters.AddWithValue("@password", entry.EncryptedPassword);

            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");

            connection.Open();

            const string query = @"DELETE FROM passwords WHERE ID = @id;";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }
}
