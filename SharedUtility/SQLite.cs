using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SUS.Shared.SQLite
{
    interface ISQLCompatibility
    {
        string ToString();
        int GetHashCode();
        bool Equals(Object obj);
        void ToInsert(ref SQLiteCommand cmd);
    }

    public class SQLWrapper
    {
        private const string _database = "GameStates.db3";
        private static string _dbSource;
        private static List<string> _queries;

        public SQLWrapper()
        {
            if (string.IsNullOrEmpty(_dbSource))
                _dbSource = $"Data Source={_database};Vesion=3;";

            if (_queries == null)
            {
                _queries = new List<string>();
                if (_queries.Count == 0)
                    queryInit();
            }

            // Setup the database if it doesn't exist.
            if (!File.Exists(_database))
                setupDatabase();
        }

        private void setupDatabase()
        {
            string[] tables =
            {
                "create table gamestates (ID int64, State VARBINARY(MAX)"
            };

            Console.WriteLine("[ Creating Database... ]");
            SQLiteConnection.CreateFile(_database);

            Console.WriteLine(" => Creating Tables...");
            // Establish connection to create tables
            using (SQLiteConnection db = new SQLiteConnection(_dbSource))
            {
                db.Open();
                using (var cmd = new SQLiteCommand(db))
                using (var transaction = db.BeginTransaction())
                {
                    // Create the tables.
                    foreach (string table in tables)
                    {
                        Console.WriteLine($"   => {table,-30}");
                        cmd.CommandText = table;
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine(" => Commiting changes.");
                    transaction.Commit();
                }
            }
            Console.WriteLine("[ Database Established. ]");
        }

        #region Queries
        // Returns all matches to the index.
        public HashSet<T> GetAll<T>(int index)
        {
            // Validate we have a database, if not create and return.
            if (!File.Exists(_database))
            {
                setupDatabase();
                return null;
            }

            // Get our query string, return if it is a bad string.
            string queryString = queryGet(index);
            if (string.IsNullOrEmpty(queryString))
                return null;

            // Passed the initial checks, create the HashSet.
            HashSet<T> items = new HashSet<T>();

            // Begin out query.
            using (SQLiteConnection _dbConnection = new SQLiteConnection(_dbSource))
            {
                _dbConnection.Open();
                using (SQLiteCommand fmd = _dbConnection.CreateCommand())
                {
                    fmd.CommandText = queryString;
                    fmd.CommandType = CommandType.Text;

                    SQLiteDataReader r = fmd.ExecuteReader();
                    while (r.Read())
                    {
                        // Process data here.
                        var obj = (T)Activator.CreateInstance(typeof(T), r);
                        items.Add(obj);
                    }
                }
            }

            return items;
        }

        // Returns the first occurence.
        public T Get<T>(int index)
        {
            // Validate we have a database, if not create and return.
            if (!File.Exists(_database))
            {
                setupDatabase();
                return default(T);
            }

            // Get our query string, return if it is a bad string.
            string queryString = queryGet(index);
            if (string.IsNullOrEmpty(queryString))
                return default(T);

            // Begin out query.
            using (SQLiteConnection _dbConnection = new SQLiteConnection(_dbSource))
            {
                _dbConnection.Open();
                using (SQLiteCommand fmd = _dbConnection.CreateCommand())
                {
                    fmd.CommandText = queryString;
                    fmd.CommandType = CommandType.Text;

                    SQLiteDataReader r = fmd.ExecuteReader();
                    while (r.Read())
                    {
                        // Process data here.
                        var obj = (T)Activator.CreateInstance(typeof(T), r);
                        return obj;
                    }
                }
            }

            return default(T);
        }

        public void Insert<T>(int index, object toInsert)
        {
            // Validate we have a database, if not create it and prepare for processing.
            if (!File.Exists(_database))
                setupDatabase();

            // Get our query string, return if it is a bad string.
            string queryString = queryGet(index);
            if (string.IsNullOrEmpty(queryString))
                return;

            // Begin out query.
            using (SQLiteConnection _dbConnection = new SQLiteConnection(_dbSource))
            {
                _dbConnection.Open();
                SQLiteCommand fmd = _dbConnection.CreateCommand();
                fmd.CommandText = queryString;
                fmd.CommandType = CommandType.Text;

                // Verify the object is compatible and meets the minimum requirements to be inserted and selected.
                ISQLCompatibility obj = toInsert as ISQLCompatibility;
                if (obj == null)
                    return;

                // Get our information to store and assign it back to the SQLiteCommand.
                obj.ToInsert(ref fmd);

                fmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Miscellaneous
        private string queryGet(int index)
        {
            // If it is an invalid number (OOB on the List) then return early.
            if (index + 1 > _queries.Count || index < 0)
                return "";

            return _queries[index];
        }

        private void queryInit()
        {
            _queries.Add("SELECT * FROM gamestates");
            _queries.Add("INSERT INTO gamestates (ID, GameState) VALUES (@p1, @p2)");
        }

        private void tableInit(ref SQLiteConnection _db)
        {

        }
        #endregion
    }
}
