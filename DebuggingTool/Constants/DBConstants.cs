using System;
using System.IO;

namespace DebuggingTool.Constants;

public static class DBConstants
{
    public const string DatabaseFilename = "DbgTool.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite
        |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create
        |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

    public static string GetAppDataPath()
    {
        string path;
#if ANDROID
        path =
            Android.App.Application.Context.GetExternalFilesDir(null)?.AbsolutePath
            ?? Android.App.Application.Context.FilesDir?.AbsolutePath;
#else
        path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
        // 确保目录存在
        Directory.CreateDirectory(path);
        return Path.Combine(path,DatabaseFilename);
    }
}
