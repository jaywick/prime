using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using Prime;

namespace Prime
{
    public static class Constants
    {
        //public const string WindowsExplorerCommand = "explorer.exe";
    }

    public enum ItemTypes
    {
        File, Folder, FolderShortcut, FileShortcut, Drive
    }

    public enum ColumnMessages
    {
        Empty, AccessDenied
    }

    public class IgnoreFlag
    {
        public bool IsBlocked { get; set; }

        public IgnoreFlag(bool initialState = false)
        {
            IsBlocked = initialState;
        }

        public void Start()
        {
            IsBlocked = true;
        }

        public void Stop()
        {
            IsBlocked = false;
        }
    }

    public static class StandardColours
    {
        private static Brush hexToBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public static Brush Home { get { return hexToBrush(0xF6, 0xF6, 0xF6); } }
        public static Brush Normal { get { return hexToBrush(0xFF, 0xFF, 0xFF); } }

        public static Brush LightBorder { get { return hexToBrush(0xF1, 0xF1, 0xF1); } }
    }

    public abstract class FileSystemItem
    {
        private enum Primitives { Directory, File, ShortcutFile, Unknown, Root, Drive }

        private readonly static Directory rootReference = new Directory("");
        public static Directory Root { get { return rootReference; } }

        public abstract string Name { get; }
        public abstract string Path { get; }
        public virtual bool IsShortcut { get { return false; } }
        public abstract ItemTypes Type { get; }

        /// <summary>
        /// Used to get the proper derived class given a path.
        /// </summary>
        /// <param name="path">Absolute path to file system object</param>
        /// <returns>Returns a FileSystemItem containing the correct derived type</returns>
        public static FileSystemItem DeriveType(string path)
        {
            var type = getPrimitive(path);

            switch (type)
            {
                case Primitives.Directory:
                    return new Directory(path);
                case Primitives.Root:
                    return FileSystemItem.Root;
                case Primitives.File:
                    return new File(path);
                case Primitives.Drive:
                    return new Drive(path);
                case Primitives.ShortcutFile:
                    var destination = GetDestination(path);
                    var destinationType = getPrimitive(destination);

                    if (destinationType == Primitives.Directory || destinationType == Primitives.Drive)
                        return new Jump(path);
                    else
                        return new Link(path);
                default:
                    throw new Exception("Invalid Type");
            }
        }

        /// <summary>
        /// Checks the primitive type of the path on the file system (i.e. file, directory or shortcut)
        /// </summary>
        /// <param name="path">Path of file to check</param>
        /// <returns>Returns either File, Directory or Shortcut const from Primitives enum</returns>
        private static Primitives getPrimitive(string path)
        {
            if (path == "")
                return Primitives.Root;

            else if (directoryExists(path) && fileExists(path))
                throw new Exception("both path and dir!!!");

            else if (directoryExists(path))
                return isDrive(path) ? Primitives.Drive : Primitives.Directory;

            else if (fileExists(path))
                return isShortcut(path) ? Primitives.ShortcutFile : Primitives.File;

            else
                return Primitives.Unknown;
        }

        private static bool isShortcut(string path)
        {
            return path.EndsWith(".lnk");
        }

        private static bool directoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        private static bool fileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        private static bool isDrive(string path)
        {
            var driveFolder = new DirectoryInfo(path);
            return driveFolder.FullName == driveFolder.Root.FullName;
        }

        /// <summary>
        /// Returns the destination of a shortcut file
        /// </summary>
        /// <param name="path">Path of shortcut</param>
        /// <returns>Path of destination for shortcut</returns>
        protected static string GetDestination(string path)
        {
            var shl = new Shell32.Shell();
            var lnkPath = System.IO.Path.GetFullPath(path);
            var dir = shl.NameSpace(System.IO.Path.GetDirectoryName(lnkPath));
            var itm = dir.Items().Item(System.IO.Path.GetFileName(lnkPath));
            var lnk = (Shell32.ShellLinkObject)itm.GetLink;
            return lnk.Target.Path;
        }

        public static bool IsSamePath(FileSystemItem subject, string other)
        {
            if ((subject is Directory && ((Directory)subject) == Root) && other == "") return true;

            var path1 = System.IO.Path.GetFullPath(subject.Path.TrimEnd('\\'));
            var path2 = System.IO.Path.GetFullPath(other.TrimEnd('\\'));

            return string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class File : FileSystemItem
    {
        protected System.IO.FileInfo destination;

        protected File() { }

        public File(string path)
        {
            this.destination = new System.IO.FileInfo(path);
        }

        public File(System.IO.FileInfo file)
        {
            this.destination = file;
        }

        public Directory Parent()
        {
            return new Directory(this.destination.Directory);
        }

        public override string Path { get { return destination.FullName; } }
        public override string Name { get { return destination.Name; } }
        public override ItemTypes Type { get { return ItemTypes.File; } }

        public string Extension { get { return destination.Extension; } }

        public override string ToString()
        {
            return Type.ToString() + ": " + Path + (IsShortcut ? " (shortcut)" : "");
        }
    }

    public class Directory : FileSystemItem
    {
        protected System.IO.DirectoryInfo destination;

        protected Directory() { }

        public Directory(string path)
        {
            if (path == "")
                this.destination = null;
            else
                this.destination = new System.IO.DirectoryInfo(path);
        }

        public Directory(System.IO.DirectoryInfo directory)
        {
            this.destination = directory;
        }

        // DO NOT USE, since querier will return different set of data
        //public bool isEmpty { get { return destination.GetFiles().Length + destination.GetDirectories().Length == 0; } }

        public override string Path { get { return destination.FullName; } }
        public override string Name { get { return destination.Name; } }
        public override ItemTypes Type { get { return ItemTypes.Folder; } }

        public async Task<long> GetSize()
        {
            var filesList = new List<FileInfo>(destination.GetFiles());
            return filesList.Sum(v => v.Length);
        }

        public override string ToString()
        {
            return Type.ToString() + ": " + Path + (IsShortcut ? " (shortcut)" : "");
        }

        public Directory Parent()
        {
            if (this.destination == null) return null;
            return new Directory(this.destination.Parent);
        }

        public static bool operator ==(Directory a, Directory b)
        {
            if (a.destination == null && b.destination == null) // if both null, they're the same
                return true;
            else if (a.destination == null ^ b.destination == null) // if only one is null, they're different
                return false;
            else
                return a.destination.FullName == b.destination.FullName; // compare root paths
        }

        public static bool operator !=(Directory a, Directory b) { return !(a == b); }
    }

    public class Drive : Prime.Directory
    {
        private System.IO.DriveInfo drive;

        public Drive(string driveLetter)
        {
            this.drive = new System.IO.DriveInfo(driveLetter);
            base.destination = drive.RootDirectory;
        }

        public override string Name { get { return drive.Name; } }
        public override bool IsShortcut { get { return false; } }
        public override ItemTypes Type { get { return ItemTypes.Drive; } }

        public override string ToString()
        {
            return Type.ToString() + ": " + Path + (IsShortcut ? " (shortcut)" : "");
        }
    }

    public class Jump : Prime.Directory
    {
        private System.IO.FileInfo source;

        public Jump(string shortcutPath)
        {
            this.source = new System.IO.FileInfo(shortcutPath);
            base.destination = new System.IO.DirectoryInfo(GetDestination(shortcutPath));
        }

        public string Source { get { return source.FullName; } }
        public override string Name { get { return source.Name.RemoveIfEndsWith(".lnk"); } }
        public override bool IsShortcut { get { return true; } }
        public override ItemTypes Type { get { return ItemTypes.FolderShortcut; } }

        public override string ToString()
        {
            return Type.ToString() + ": " + Path + (IsShortcut ? " (shortcut)" : "");
        }
    }

    public class Link : Prime.File
    {
        private System.IO.FileInfo source;
        private string shortcutName;

        public Link(string shortcutPath)
        {
            this.source = new System.IO.FileInfo(shortcutPath);

            var redirect = GetDestination(shortcutPath);
            if (System.IO.File.Exists(redirect))
                base.destination = new System.IO.FileInfo(redirect);
            else
                base.destination = source;

            // replace shortcut extension (LNK) with extesion of destination
            shortcutName = source.Name.RemoveIfEndsWith(".lnk") + destination.Extension;
        }

        public string Source { get { return source.FullName; } }
        public override string Name { get { return shortcutName; } }
        public override bool IsShortcut { get { return true; } }
        public override ItemTypes Type { get { return ItemTypes.FileShortcut; } }

        public override string ToString()
        {
            return Type.ToString() + ": " + Path + (IsShortcut ? " (shortcut)" : "");
        }
    }

    public static class Extensions
    {
        public static string RemoveIfEndsWith(this string text, string ending)
        {
            if (text.EndsWith(ending))
                return text.Substring(0, text.Length - ending.Length);
            else
                return text;
        }
    }
}