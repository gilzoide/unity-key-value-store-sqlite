using System;
using System.Runtime.InteropServices;
using SQLite;

namespace Gilzoide.KeyValueStore.Sqlite
{
    public class SqlitePreparedStatement : IDisposable
    {
        public static readonly IntPtr SQLITE_STATIC = IntPtr.Zero;

        [DllImport(SQLite3.LibraryPath, EntryPoint = "sqlite3_column_bytes16", CallingConvention = CallingConvention.Cdecl)]
		private static extern int ColumnBytes16(IntPtr stmt, int index);

        private IntPtr _dbHandle;
        private IntPtr _preparedStatement;

        public SqlitePreparedStatement(SQLiteConnection db, string statement)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            _dbHandle = db.Handle;
            _preparedStatement = SQLite3.Prepare2(db.Handle, statement);
        }

        public void Reset()
        {
            SQLite3.Reset(_preparedStatement);
        }

        public void Bind(int index, bool value)
        {
            SQLite3.BindInt(_preparedStatement, index, value ? 1 : 0);
        }

        public void Bind(int index, long value)
        {
            SQLite3.BindInt64(_preparedStatement, index, value);
        }

        public void Bind(int index, double value)
        {
            SQLite3.BindDouble(_preparedStatement, index, value);
        }

        public void Bind(int index, string value)
        {
            SQLite3.BindText(_preparedStatement, index, value, value.Length * sizeof(char), SQLITE_STATIC);
        }

        public void Bind(int index, byte[] value)
        {
            SQLite3.BindBlob(_preparedStatement, index, value, value.Length, SQLITE_STATIC);
        }

        public SQLite3.Result Step()
        {
            var result = SQLite3.Step(_preparedStatement);
            if (result > SQLite3.Result.OK && result < SQLite3.Result.Row)
            {
                throw SQLiteException.New(result, SQLite3.GetErrmsg(_dbHandle));
            }
            return result;
        }

        public int GetColumnCount()
        {
            return SQLite3.ColumnCount(_preparedStatement);
        }

        public bool GetBool(int column)
        {
            return SQLite3.ColumnInt(_preparedStatement, column) != 0;
        }

        public long GetLong(int column)
        {
            return SQLite3.ColumnInt64(_preparedStatement, column);
        }

        public double GetDouble(int column)
        {
            return SQLite3.ColumnDouble(_preparedStatement, column);
        }

        public string GetText(int column)
        {
            IntPtr ptr = SQLite3.ColumnText16(_preparedStatement, column);
            int sizeInBytes = ColumnBytes16(_preparedStatement, column);
            return Marshal.PtrToStringUni(ptr, sizeInBytes / sizeof(char));
        }

        public byte[] GetBlob(int column)
        {
            IntPtr blob = SQLite3.ColumnBlob(_preparedStatement, column);
            int size = SQLite3.ColumnBytes(_preparedStatement, column);
            var value = new byte[size];
            Marshal.Copy(blob, value, 0, size);
            return value;
        }

        public void Dispose()
        {
            SQLite3.Finalize(_preparedStatement);
            _preparedStatement = IntPtr.Zero;
            _dbHandle = IntPtr.Zero;
        }
    }
}
