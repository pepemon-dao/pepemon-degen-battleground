using Cysharp.Threading.Tasks;
using SQLite;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Cache
{
    public class DownloadCache
    {
        public const string CACHE_DB_FILENAME = "cache.db";
        public const string CACHE_FOLDER_NAME = "cache";
        private bool dbReady = false;
        private SQLiteAsyncConnection sqlite;

        public DownloadCache()
        {
            UniTask.Void(Init);
        }

        private async UniTaskVoid Init()
        {
            dbReady = false;
            string dbPath = GetCacheDbFullPath();
            await OpenDb(dbPath);
            dbReady = true;
        }

        private async UniTask Reset()
        {
            dbReady = false;

            if (sqlite != null)
            {
                await sqlite.CloseAsync();
            }
            sqlite = null;

            string dbPath = GetCacheDbFullPath();

            File.Delete(dbPath);

            await OpenDb(dbPath, true);

            dbReady = true;
        }

        /// <summary>
        /// Tries to get a cached entry
        /// </summary>
        public async UniTask<byte[]> Get(string url)
        {
            if (!dbReady)
            {
                Debug.LogWarning($"Database not ready");
                return null;
            }
            try
            {
                return (await sqlite.Table<FileCacheEntry>().Where(x => x.Url == url).FirstOrDefaultAsync())?.Data;
            }
            catch (SQLiteException e)
            {
                // Result.Error might be thrown if the table is not found
                if (e.Result == SQLite3.Result.Error || e.Result == SQLite3.Result.Corrupt)
                {
                    await Reset();
                    Debug.LogWarning($"Cache database is invalid");
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to set a cached entry
        /// </summary>
        public async UniTask Set(string url, byte[] data)
        {
            if (!dbReady)
            {
                Debug.LogWarning($"Database not ready");
                return;
            }
            try
            {
                await sqlite.InsertOrReplaceAsync(new FileCacheEntry() { Data = data, Url = url });
            }
            catch (SQLiteException e)
            {
                if (e.Result == SQLite3.Result.Error || e.Result == SQLite3.Result.Corrupt)
                {
                    Debug.LogWarning($"Cache database is invalid. Resetting.");
                    await Reset();
                }
            }
        }

        // also creates the DB if it does not exist
        private async UniTask OpenDb(string dbPath, bool ignoreError = false)
        {
            try
            {
                sqlite = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
                await sqlite.ExecuteAsync("PRAGMA integrity_check");
            }
            catch (SQLiteException ex)
            {
                // ignore known issue with SQLite-net where an exception is thrown but no error occurred
                // https://stackoverflow.com/a/23839503
                if (ex.Result == SQLite3.Result.Corrupt && !ignoreError)
                {
                    Debug.LogWarning($"Invalid cache database, recreating a new one");
                    await Reset();
                    return;
                }
            }

            // if table FileCacheEntry does not exist
            if ((await sqlite.GetTableInfoAsync(nameof(FileCacheEntry))).Count == 0)
            {
                // Firefox uses the same params
                string[] cmds = new string[]
                {
                    "PRAGMA auto_vacuum=incremental",
                    "PRAGMA journal_mode=WAL",
                    "PRAGMA synchronous=normal", // less performant but more reliable
                };
                foreach (string cmd in cmds)
                {
                    try
                    {
                        await sqlite.ExecuteAsync(cmd);
                    }
                    catch (SQLiteException e)
                    {
                        // ignore known issue with SQLite-net where an exception is thrown but no error occurred
                        // https://stackoverflow.com/a/23839503
                        if (e.Result != SQLite3.Result.Row)
                        {
                            Debug.LogWarning($"Error running command '{cmd}': {e.Message}");
                        }
                    }
                }
            }
        }

        public static string GetCacheDbFullPath()
        {
            string path = Path.Combine(Application.temporaryCachePath, CACHE_FOLDER_NAME);
            
#if UNITY_STANDALONE_WIN
            // Only required for windows. Android/iOS uses relative path
            path = Path.GetFullPath(path);
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, CACHE_DB_FILENAME);
            return path;
        }
    }
}