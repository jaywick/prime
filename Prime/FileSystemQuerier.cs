using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prime
{
    class FileSystemQuerier
    {
        public enum SearchOptions
        {
            ShowSystemFiles, ShowHiddenFiles, ShowItemsStartingWithDots, ShowTemporaryFiles,
            NaturalSort
        }

        /// <summary>
        /// Get all concrete folders and jump (shortcuts to folders)
        /// </summary>
        /// <param name="path">Path of folder to search inside</param>
        /// <returns>Returns a list of directories including jumps</returns>
        public static List<FileSystemItem> Search(string path)//, SearchOptions)
        {
            var contents = new List<FileSystemItem>();
            
            // add directories
            contents.AddRange(getDirs(path));

            // add files (conver to links or jumps if they are)
            foreach (var f in getFiles(path))
                contents.Add(FileSystemItem.DeriveType(f.Path));

            return contents;
        }


        private static List<Directory> getDirs(string path)
        {
            var dir = new DirectoryInfo(path);

            try
            {
                var query = from d in dir.GetDirectories()
                            where !d.Attributes.HasFlag(FileAttributes.Hidden)
                                  && !d.Attributes.HasFlag(FileAttributes.System)
                                  && !d.Name.StartsWith(".")
                            select new Directory(d);
                query = query.OrderBy(x => x.Name, new DavidKoelle.AlphanumComparator());

                return query.ToList<Directory>();
            }
            catch (UnauthorizedAccessException) { throw; }
        }

        private static List<File> getFiles(string path)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                var query = from f in dir.GetFiles()
                            where !f.Attributes.HasFlag(FileAttributes.Hidden)
                                  && !f.Attributes.HasFlag(FileAttributes.System)
                                  && !f.Attributes.HasFlag(FileAttributes.Temporary)
                                  && !f.Name.StartsWith(".")
                                  && f.Name != "desktop.ini"
                                  && f.Name != "thumbs.db"
                            select new File(f);
                query = query.OrderBy(f => f.Extension).ThenBy(f => f.Name, new DavidKoelle.AlphanumComparator());
                return query.ToList<File>();
            }
            catch (UnauthorizedAccessException) { throw; }
        }
    }
}
