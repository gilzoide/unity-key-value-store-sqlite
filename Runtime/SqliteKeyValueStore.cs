using System;
using System.Globalization;
using System.Threading.Tasks;
using SQLite;
using UnityEngine;

namespace Gilzoide.KeyValueStore.Sqlite
{
    public class SqliteKeyValueStore : IKeyValueStore, IDisposable
    {
        public const string CreateTableSql = "CREATE TABLE IF NOT EXISTS KeyValueStore (key TEXT NOT NULL PRIMARY KEY, value)";
        public const string DeleteAllSql = "DELETE FROM KeyValueStore";
        public const string DeleteKeySql = "DELETE FROM KeyValueStore WHERE key = ?";
        public const string UpsertSql = "INSERT INTO KeyValueStore(key, value) VALUES(?1, ?2) ON CONFLICT(key) DO UPDATE SET value = ?2";
        public const string SelectSql = "SELECT value FROM KeyValueStore WHERE key = ?";
        public const string BeginSql = "BEGIN";
        public const string CommitSql = "COMMIT";

        public SqliteKeyValueStore() : this(":memory:")
        {
        }

        public SqliteKeyValueStore(string filename)
        {
            _db = new SQLiteConnection(filename);
            _db.Execute(CreateTableSql);
        }

        ~SqliteKeyValueStore()
        {
            Dispose();
        }

        private SqlitePreparedStatement SelectStmt => _selectStmt != null ? _selectStmt : (_selectStmt = new SqlitePreparedStatement(_db, SelectSql));
        private SqlitePreparedStatement UpsertStmt => _upsertStmt != null ? _upsertStmt : (_upsertStmt = new SqlitePreparedStatement(_db, UpsertSql));
        private SqlitePreparedStatement DeleteAllStmt => _deleteAllStmt != null ? _deleteAllStmt : (_deleteAllStmt = new SqlitePreparedStatement(_db, DeleteAllSql));
        private SqlitePreparedStatement DeleteKeyStmt => _deleteKeyStmt != null ? _deleteKeyStmt : (_deleteKeyStmt = new SqlitePreparedStatement(_db, DeleteKeySql));
        private SqlitePreparedStatement BeginStmt => _beginStmt != null ? _beginStmt : (_beginStmt = new SqlitePreparedStatement(_db, BeginSql));
        private SqlitePreparedStatement CommitStmt => _commitStmt != null ? _commitStmt : (_commitStmt = new SqlitePreparedStatement(_db, CommitSql));

        private readonly SQLiteConnection _db;
        private SqlitePreparedStatement _selectStmt;
        private SqlitePreparedStatement _upsertStmt;
        private SqlitePreparedStatement _deleteAllStmt;
        private SqlitePreparedStatement _deleteKeyStmt;
        private SqlitePreparedStatement _beginStmt;
        private SqlitePreparedStatement _commitStmt;
        private bool _isInTransaction = false;
        private bool _isPendingCommit = false;

        public void DeleteAll()
        {
            EnsureTransaction();
            try
            {
                DeleteAllStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                DeleteAllStmt.Reset();
            }
        }

        public void DeleteKey(string key)
        {
            EnsureTransaction();
            try
            {
                DeleteKeyStmt.Bind(1, key);
                DeleteKeyStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                DeleteKeyStmt.Reset();
            }
        }

        public bool HasKey(string key)
        {
            EnsureTransaction();
            try
            {
                SelectStmt.Bind(1, key);
                return SelectStmt.Step() == SQLite3.Result.Row;
            }
            finally
            {
                SelectStmt.Reset();
            }
        }

        public void SetBool(string key, bool value)
        {
            EnsureTransaction();
            try
            {
                UpsertStmt.Bind(1, key);
                UpsertStmt.Bind(2, value);
                UpsertStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                UpsertStmt.Reset();
            }
        }

        public void SetBytes(string key, byte[] value)
        {
            EnsureTransaction();
            try
            {
                UpsertStmt.Bind(1, key);
                UpsertStmt.Bind(2, value);
                UpsertStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                UpsertStmt.Reset();
            }
        }

        public void SetDouble(string key, double value)
        {
            EnsureTransaction();
            try
            {
                UpsertStmt.Bind(1, key);
                UpsertStmt.Bind(2, value);
                UpsertStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                UpsertStmt.Reset();
            }
        }

        public void SetFloat(string key, float value)
        {
            SetDouble(key, value);
        }

        public void SetInt(string key, int value)
        {
            SetLong(key, value);
        }

        public void SetLong(string key, long value)
        {
            EnsureTransaction();
            try
            {
                UpsertStmt.Bind(1, key);
                UpsertStmt.Bind(2, value);
                UpsertStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                UpsertStmt.Reset();
            }
        }

        public void SetString(string key, string value)
        {
            EnsureTransaction();
            try
            {
                UpsertStmt.Bind(1, key);
                UpsertStmt.Bind(2, value);
                UpsertStmt.Step();
                ScheduleCommit();
            }
            finally
            {
                UpsertStmt.Reset();
            }
        }

        public bool TryGetBool(string key, out bool value)
        {
            if (TryGetLong(key, out long longValue))
            {
                value = longValue != 0;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool TryGetBytes(string key, out byte[] value)
        {
            EnsureTransaction();
            try
            {
                SelectStmt.Bind(1, key);
                switch (SelectStmt.Step())
                {
                    case SQLite3.Result.Row:
                        value = SelectStmt.GetBlob(0);
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            finally
            {
                SelectStmt.Reset();
            }
        }

        public bool TryGetDouble(string key, out double value)
        {
            EnsureTransaction();
            try
            {
                SelectStmt.Bind(1, key);
                switch (SelectStmt.Step())
                {
                    case SQLite3.Result.Row:
                        value = SelectStmt.GetDouble(0);
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            finally
            {
                SelectStmt.Reset();
            }
        }

        public bool TryGetFloat(string key, out float value)
        {
            if (TryGetDouble(key, out double doubleValue))
            {
                value = (float) doubleValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool TryGetInt(string key, out int value)
        {
            if (TryGetLong(key, out long longValue))
            {
                value = (int) longValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool TryGetLong(string key, out long value)
        {
            EnsureTransaction();
            try
            {
                SelectStmt.Bind(1, key);
                switch (SelectStmt.Step())
                {
                    case SQLite3.Result.Row:
                        value = SelectStmt.GetLong(0);
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            finally
            {
                SelectStmt.Reset();
            }
        }

        public bool TryGetString(string key, out string value)
        {
            EnsureTransaction();
            try
            {
                SelectStmt.Bind(1, key);
                switch (SelectStmt.Step())
                {
                    case SQLite3.Result.Row:
                        value = SelectStmt.GetText(0);
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            finally
            {
                SelectStmt.Reset();
            }
        }

        public void Dispose()
        {
            Commit();
            _selectStmt?.Dispose();
            _upsertStmt?.Dispose();
            _deleteAllStmt?.Dispose();
            _deleteKeyStmt?.Dispose();
            _beginStmt?.Dispose();
            _commitStmt?.Dispose();
            _db.Dispose();
        }

        public string Pragma(string pragma)
        {
            Debug.Assert(!pragma.Contains(";"), "Pragma strings must not contain ';'");
            if (!pragma.TrimStart().StartsWith("pragma ", true, CultureInfo.InvariantCulture))
            {
                pragma = "PRAGMA " + pragma;
            }
            return _db.ExecuteScalar<string>(pragma);
        }

        public void Vacuum()
        {
            Commit();
            _db.Execute("VACUUM");
        }

        public void Commit()
        {
            if (_isInTransaction)
            {
                _isInTransaction = false;
                try
                {
                    CommitStmt.Step();
                }
                finally
                {
                    CommitStmt.Reset();
                }
            }
        }

        private void EnsureTransaction()
        {
            if (!_isInTransaction)
            {
                _isInTransaction = true;
                try
                {
                    BeginStmt.Step();
                }
                finally
                {
                    BeginStmt.Reset();
                }
            }
        }

        private async void ScheduleCommit()
        {
            if (_isPendingCommit)
            {
                return;
            }

            _isPendingCommit = true;
            try
            {
                await Task.Yield();
                Commit();
            }
            finally
            {
                _isPendingCommit = false;
            }
        }
    }
}
