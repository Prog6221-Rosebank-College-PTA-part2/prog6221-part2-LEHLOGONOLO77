using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CyberSecurityAwarenessBot
{
    // ── Domain model ──────────────────────────────────────────────────────────

    public enum TaskStatus { Pending, Completed }

    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReminderAt { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string ReminderDisplay =>
            ReminderAt.HasValue ? ReminderAt.Value.ToString("dd MMM yyyy") : "None";

        public string StatusDisplay =>
            Status == TaskStatus.Completed ? "✔ Completed" : "⏳ Pending";
    }

    // ── Database helper ───────────────────────────────────────────────────────

    /// <summary>
    /// Handles all MySQL operations for CyberTask persistence.
    ///
    /// ⚙ SETUP:
    ///   1. Install NuGet package:  MySql.Data
    ///   2. Edit DB_PASSWORD below if your MySQL root has a password
    ///   3. Run cyberbot_mysql_workbench.sql in MySQL Workbench first
    /// </summary>
    public static class DatabaseHelper
    {
        // ── ⚙ Edit these to match your MySQL installation ─────────────────
        private const string DB_SERVER = "localhost";
        private const string DB_PORT = "3306";
        private const string DB_NAME = "cyberbot_db";
        private const string DB_USER = "root";
        private const string DB_PASSWORD = "";   // ← add your password here if you set one
        // ─────────────────────────────────────────────────────────────────

        private static string ConnectionString =>
            $"Server={DB_SERVER};Port={DB_PORT};Database={DB_NAME};" +
            $"Uid={DB_USER};Pwd={DB_PASSWORD};CharSet=utf8mb4;";

        // ── Initialisation ────────────────────────────────────────────────

        /// <summary>
        /// Verifies the connection and ensures the tasks table exists.
        /// Call once at application startup.
        /// </summary>
        public static void Initialise()
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS tasks (
                    id          INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    title       VARCHAR(200) NOT NULL,
                    description TEXT         NOT NULL,
                    reminder_at DATETIME         NULL,
                    status      TINYINT      NOT NULL DEFAULT 0,
                    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn);

            cmd.ExecuteNonQuery();
        }

        // ── CRUD operations ───────────────────────────────────────────────

        public static int AddTask(CyberTask task)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            const string sql = @"
                INSERT INTO tasks (title, description, reminder_at, status, created_at)
                VALUES (@title, @desc, @rem, @status, @created);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@desc", task.Description);
            cmd.Parameters.AddWithValue("@rem", (object?)task.ReminderAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", (int)task.Status);
            cmd.Parameters.AddWithValue("@created", task.CreatedAt);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new MySqlCommand(@"
                SELECT id, title, description, reminder_at, status, created_at
                FROM tasks
                ORDER BY created_at DESC;", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new CyberTask
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    ReminderAt = reader.IsDBNull(reader.GetOrdinal("reminder_at"))
                                    ? null
                                    : reader.GetDateTime("reminder_at"),
                    Status = (TaskStatus)reader.GetInt32("status"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }

            return tasks;
        }

        public static void MarkCompleted(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE tasks SET status = 1 WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteTask(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(
                "DELETE FROM tasks WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateReminder(int id, DateTime reminderAt)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE tasks SET reminder_at = @rem WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@rem", reminderAt);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns pending tasks whose reminder date is today or in the past.
        /// Used to show reminder notifications when the user logs in.
        /// </summary>
        public static List<CyberTask> GetDueReminders()
        {
            var tasks = new List<CyberTask>();

            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new MySqlCommand(@"
                SELECT id, title, description, reminder_at, status, created_at
                FROM tasks
                WHERE status = 0
                  AND reminder_at IS NOT NULL
                  AND reminder_at <= @now
                ORDER BY reminder_at;", conn);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new CyberTask
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    ReminderAt = reader.GetDateTime("reminder_at"),
                    Status = (TaskStatus)reader.GetInt32("status"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }

            return tasks;
        }
    }
}