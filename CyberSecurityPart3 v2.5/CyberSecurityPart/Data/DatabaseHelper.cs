using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;

namespace CyberSecurityAwarenessBot.Forms
{
    public static class DatabaseHelper
    {
        private const string DefaultConnectionString =
            "Server=localhost;Port=3306;Database=cyberbot_db;Uid=root;Pwd=MYC3RISGTI*;SslMode=None;";

        private static string ConnectionString
        {
            get
            {
                string? env = Environment.GetEnvironmentVariable("CYBERBOT_DB_CONNECTION");
                if (!string.IsNullOrWhiteSpace(env)) return env;

                try
                {
                    string configPath = Path.Combine(AppContext.BaseDirectory, "db.config");
                    if (File.Exists(configPath))
                    {
                        string fromFile = File.ReadAllText(configPath).Trim();
                        if (!string.IsNullOrWhiteSpace(fromFile)) return fromFile;
                    }
                }
                catch { /* fall through to default */ }

                return DefaultConnectionString;
            }
        }

        private static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            try
            {
                conn.Open();
            }
            catch (MySqlException ex)
            {
                conn.Dispose();
                throw new InvalidOperationException(
                    "Could not connect to the MySQL database. Make sure MySQL Server is " +
                    "running and the connection details are correct (see db.config or the " +
                    "CYBERBOT_DB_CONNECTION environment variable).\n\nDetails: " + ex.Message, ex);
            }
            return conn;
        }

        public static void Initialise()
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    Id          INT AUTO_INCREMENT PRIMARY KEY,
                    Title       VARCHAR(255)  NOT NULL,
                    Description TEXT          NOT NULL,
                    ReminderAt  DATE          NULL,
                    Status      VARCHAR(20)   NOT NULL DEFAULT 'Pending',
                    CreatedAt   DATETIME      NOT NULL
                );";
            cmd.ExecuteNonQuery();

            
            cmd.CommandText = @"
                UPDATE tasks
                SET    Status = 'Pending'
                WHERE  Status IS NULL
                    OR Status = ''
                    OR Status = 'None';";
            cmd.ExecuteNonQuery();
            
        }

        public static List<CyberTask> GetAllTasks()
        {
            var results = new List<CyberTask>();

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Title, Description, ReminderAt, Status, CreatedAt " +
                "FROM tasks ORDER BY CreatedAt DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                results.Add(ReadTask(reader));

            return results;
        }

        public static int AddTask(CyberTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be empty.", nameof(task));

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO tasks (Title, Description, ReminderAt, Status, CreatedAt) " +
                "VALUES (@title, @desc, @reminder, @status, @created);";

            cmd.Parameters.AddWithValue("@title", task.Title.Trim());
            cmd.Parameters.AddWithValue("@desc", task.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@reminder",
                task.ReminderAt.HasValue ? task.ReminderAt.Value.Date : (object)DBNull.Value);

            
            string statusStr = task.Status == TaskStatus.Completed ? "Completed" : "Pending";
            cmd.Parameters.AddWithValue("@status", statusStr);
          

            cmd.Parameters.AddWithValue("@created", task.CreatedAt);

            cmd.ExecuteNonQuery();
            return (int)cmd.LastInsertedId;
        }

        public static void MarkCompleted(int id)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE tasks SET Status = 'Completed' WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException($"No task with Id {id} was found to update.");
        }

        public static void UpdateReminder(int id, DateTime reminderDate)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE tasks SET ReminderAt = @reminder WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@reminder", reminderDate.Date);
            cmd.Parameters.AddWithValue("@id", id);

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException($"No task with Id {id} was found to update.");
        }

        public static void DeleteTask(int id)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM tasks WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException($"No task with Id {id} was found to delete.");
        }

        public static List<CyberTask> GetDueReminders()
        {
            var results = new List<CyberTask>();

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Title, Description, ReminderAt, Status, CreatedAt FROM tasks " +
                "WHERE Status = 'Pending' AND ReminderAt IS NOT NULL AND ReminderAt <= @today " +
                "ORDER BY ReminderAt ASC;";
            cmd.Parameters.AddWithValue("@today", DateTime.Today);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                results.Add(ReadTask(reader));

            return results;
        }

        private static CyberTask ReadTask(MySqlDataReader reader)
        {
            int idOrd = reader.GetOrdinal("Id");
            int titleOrd = reader.GetOrdinal("Title");
            int descOrd = reader.GetOrdinal("Description");
            int reminderOrd = reader.GetOrdinal("ReminderAt");
            int statusOrd = reader.GetOrdinal("Status");
            int createdOrd = reader.GetOrdinal("CreatedAt");

            
            string rawStatus = reader.IsDBNull(statusOrd)
                ? "Pending"
                : reader.GetString(statusOrd).Trim();

            TaskStatus status = rawStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                ? TaskStatus.Completed
                : TaskStatus.Pending;  

            return new CyberTask
            {
          
                Id = reader.GetInt32(idOrd),
                Title = reader.GetString(titleOrd),
                Description = reader.IsDBNull(descOrd) ? string.Empty : reader.GetString(descOrd),
                ReminderAt = reader.IsDBNull(reminderOrd) ? null : reader.GetDateTime(reminderOrd),
                Status = status,
                CreatedAt = reader.GetDateTime(createdOrd)
            };
        }
    }
}