using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prime.Components
{
    public delegate void ColumnDirectoryChangedEvent(Directory newPath);
    public partial class ColumnStack : UserControl
    {
        private Directory currentPath;
        private Directory root;

        public event ColumnDirectoryChangedEvent ColumnDirectoryChanged;

        public Directory CurrentDirectory { get { return currentPath; } }

        public ColumnStack()
        {
            InitializeComponent();
            scrollTimer.Elapsed += scrollTimer_Elapsed;
        }

        public ColumnStack(Directory path)
            : this()
        {
            Initialise(path);
        }

        // for late init, eg control already in window
        public void Initialise(Directory path)
        {
            stackMain.Children.Clear();
            root = path;
            Navigate(path);
        }

        /*Directory getLowestCommonAncestor(Directory path1, Directory path2)
        {
            var chunks1 = path1.Path.Split('\\');
            var chunks2 = path2.Path.Split('\\');

            var bloodLine = new List<String>();
            for (int i = 0; i < Math.Min(chunks1.Length, chunks2.Length); i++)
            {
                if (chunks1[i] != chunks2[i])
                    break;

                bloodLine.Add(chunks1[i]);
            }

            if (bloodLine.Count == 0)
                return null;
            else
                return new Directory(String.Join("\\", bloodLine.ToArray()));
        }*/

        private ListPage Navigate(ListPage source, Directory item, FileSystemItem selection = null)
        {
            // trim columns to right and add new list
            if (selection == null) trimRight(source);
            var list = addListPage(item, false, selection);

            navigationComplete(item);

            return list;
        }

        private void navigationComplete(Directory item)
        {
            currentPath = item;

            // broadcast changes
            if (ColumnDirectoryChanged != null)
                ColumnDirectoryChanged.Invoke(item);

            scrollMain.ScrollToRightEnd();
        }

        System.Timers.Timer scrollTimer = new System.Timers.Timer(20);
        double scrollOffset = 0;

        void scrollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            scrollMain.ScrollToHorizontalOffset(scrollOffset++);
        }

        private void clearAll()
        {
            stackMain.Children.Clear();
        }

        private void trimRight(ListPage source)
        {
            var start = stackMain.Children.IndexOf(source) + 1;
            var finish = stackMain.Children.Count - 1;
            stackMain.Children.RemoveRange(start, finish);
        }

        public void Navigate(string path)
        {
            Navigate(new Directory(path));
        }

        public void Navigate(Hyperlink link)
        {
            Navigate(new Directory(link.NavigateUri.LocalPath));
        }

        public void Navigate(Directory path)
        {
            if (path == root)
            {
                addListPage(path, true);
            }
            else
            {
                clearAll();
                
                // get list of parent paths
                Directory forefather = path;
                var bloodLine = new List<Directory>();

                do
                {
                    bloodLine.Add(forefather);
                    forefather = forefather.Parent();
                } while (forefather != Directory.Root);

                bloodLine.Reverse();

                var nextList = addListPage(FileSystemItem.Root, true, bloodLine.First());
                for (int i = 0; i < bloodLine.Count - 1; i++)
                {
                    var current = bloodLine[i];
                    var next = bloodLine[i + 1];
                    nextList = Navigate(nextList, current, next);
                }
                nextList = Navigate(nextList, bloodLine.Last());
            }

            navigationComplete(path);
        }

        private ListPage addListPage(Directory path, bool isRoot = false, FileSystemItem selection = null)
        {
            // add new list
            ListPage list = null;
            try
            {
                list = new ListPage(path, isRoot, selection);
            } 
            catch (UnauthorizedAccessException)
            {
                addMessagePage(ColumnMessages.AccessDenied);
                return null;
            }
            
            stackMain.Children.Add(list);

            // subscribe to events
            list.SelectedMultiple += list_SelectedMultiple;
            list.SelectedNothing += list_SelectedNothing;
            list.SelectedFile += list_SelectedFile;
            list.SelectedDirectory += list_SelectedDirectory;
            list.SelectedJump += list_SelectedDirectory; // future: have settings to reset home?
            list.SelectedLink += list_SelectedLink;
            list.ItemDoubleClicked += list_ItemDoubleClicked;

            return list;
        }

        void list_ItemDoubleClicked(FileSystemItem reference)
        {
            if (reference.Type == ItemTypes.File || reference.Type == ItemTypes.FileShortcut)
                System.Diagnostics.Process.Start(reference.Path);
            else if (reference.Type == ItemTypes.Folder || reference.Type == ItemTypes.FolderShortcut)
                System.Diagnostics.Process.Start(reference.Path);
        }

        private void addMessagePage(ColumnMessages type)
        {
            var message = new MessagePage(type);
            stackMain.Children.Add(message);
        }

        private void addInfoPage(FileSystemItem[] items)
        {
            var message = new InfoPage(items);
            stackMain.Children.Add(message);
        }

        #region Event Handlers

        void list_SelectedLink(ListPage sender, Directory fromDirectory, Link selection)
        {
            trimRight(sender);
        }

        void list_SelectedFile(ListPage sender, Directory fromDirectory, File selection)
        {
            trimRight(sender);
        }

        void list_SelectedDirectory(ListPage sender, Directory fromDirectory, Directory selection)
        {
            //Navigate(selection);
            Navigate(sender, selection);
        }

        void list_SelectedNothing(ListPage sender, Directory fromDirectory)
        {
            trimRight(sender);
        }

        void list_SelectedMultiple(ListPage sender, Directory fromDirectory, FileSystemItem[] selection)
        {
            trimRight(sender);
            addInfoPage(selection);
        }

        #endregion
    }
}
