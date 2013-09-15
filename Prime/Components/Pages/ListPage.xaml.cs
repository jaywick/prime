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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prime.Components
{
    public delegate void SelectedFileEventHandler(ListPage sender, Directory fromDirectory, File selection);
    public delegate void SelectedDirectoryEventHandler(ListPage sender, Directory fromDirectory, Directory selection);
    public delegate void SelectedJumpEventHandler(ListPage sender, Directory fromDirectory, Jump selection);
    public delegate void SelectedLinkEventHandler(ListPage sender, Directory fromDirectory, Link selection);
    public delegate void SelectedNothingEventHandler(ListPage sender, Directory fromDirectory);
    public delegate void SelectedMultipleEventHandler(ListPage sender, Directory fromDirectory, FileSystemItem[] selection);

    public partial class ListPage : UserControl, IPagable
    {
        public event SelectedFileEventHandler SelectedFile;
        public event SelectedDirectoryEventHandler SelectedDirectory;
        public event SelectedJumpEventHandler SelectedJump;
        public event SelectedLinkEventHandler SelectedLink;
        public event SelectedNothingEventHandler SelectedNothing;
        public event SelectedMultipleEventHandler SelectedMultiple;
        public event ItemDoubleClickedEventHandler ItemDoubleClicked;

        private Directory directory;

        public Directory Directory { get { return directory; } }

        public ListPage()
        {
            InitializeComponent();
            listMain.SelectionMode = SelectionMode.Extended;
            listMain.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            listMain.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            listMain.BorderBrush = Prime.StandardColours.LightBorder;
            listMain.Background = Prime.StandardColours.Normal;
            listMain.Width = 250;

            ////listMain.Width = 200;
            //listMain.Width = Double.NaN;
            //listMain.MinWidth = 200;
            //listMain.MaxWidth = 300;

            listMain.SelectionChanged += listMain_SelectionChanged;
        }

        public ListPage(Directory directory, bool isHome = false, FileSystemItem selection = null)
            : this()
        {
            this.directory = directory;

            if (isHome)
                listMain.Background = Prime.StandardColours.Home;

            if (directory == FileSystemItem.Root)
                loadRoot(selection);
            else
                loadDirectory(directory, selection);
        }

        private void loadRoot(FileSystemItem selection = null)
        {
            listMain.Items.Clear();

            // add all drives
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                var newItem = new ListPageItem(new Directory(drive.RootDirectory));
                newItem.ItemDoubleClicked += newItem_ItemDoubleClicked;

                listMain.Items.Add(newItem);
                if (selection != null && FileSystemItem.IsSamePath(selection, drive.RootDirectory.FullName))
                {
                    ignoreSelectionChanges.Start();
                    listMain.SelectedIndex = listMain.Items.IndexOf(newItem);
                    ignoreSelectionChanges.Stop();
                }
            }
        }

        IgnoreFlag ignoreSelectionChanges = new IgnoreFlag();
        private void loadDirectory(Directory directory, FileSystemItem selection = null)
        {
            listMain.Items.Clear();

            // add items, directories first
            var allItems = new List<FileSystemItem>();

            try { allItems.AddRange(FileSystemQuerier.Search(directory.Path)); }
            catch (UnauthorizedAccessException) { throw; }

            if (allItems.Count > 0)
            {
                foreach (var item in allItems)
                {
                    var newItem = new ListPageItem(item);
                    newItem.ItemDoubleClicked += newItem_ItemDoubleClicked;
                    listMain.Items.Add(newItem);

                    if (selection != null && FileSystemItem.IsSamePath(selection, item.Path))
                    {
                        ignoreSelectionChanges.Start();
                        listMain.SelectedIndex = listMain.Items.IndexOf(newItem);
                        ignoreSelectionChanges.Stop();
                        break;
                    }
                }
            }
            else
            {
                // show message that directory is empty
                textEmpty.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Processes and reroutes events when the list box selection changes.
        /// The three types of events are when selection count is 0, 1 or many.
        /// </summary>
        void listMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanges.IsBlocked) return;

            var count = listMain.SelectedItems.Count;
            if (count == 0)
            {
                SelectedNothing.Invoke(this, directory);
            }
            else if (count == 1)
            {
                var item = (ListPageItem)listMain.SelectedItem;

                if (item.Reference is File)
                    SelectedFile.Invoke(this, directory, (File)item.Reference);
                else if (item.Reference is Directory)
                    SelectedDirectory.Invoke(this, directory, (Directory)item.Reference);
                else if (item.Reference is Jump)
                    SelectedJump.Invoke(this, directory, (Jump)item.Reference);
                else if (item.Reference is Link)
                    SelectedLink.Invoke(this, directory, (Link)item.Reference);
            }
            else
            {
                var items = new List<FileSystemItem>();
                foreach (ListPageItem item in listMain.SelectedItems)
                    items.Add(item.Reference);

                SelectedMultiple.Invoke(this, directory, items.ToArray());
            }
        }

        void newItem_ItemDoubleClicked(FileSystemItem reference)
        {
            // bubble event to parent (column)
            if (ItemDoubleClicked != null)
                ItemDoubleClicked.Invoke(reference);
        }

        private void getRealIcon()
        {
            /*var bitmap = IconFromFilePath(item.FullName);
                BitmapImage bitmapImage;
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                fi.iconBlank.Source = bitmapImage;*/
        }

        /*public System.Drawing.Bitmap IconFromFilePath(string filePath)
        {
            System.Drawing.Bitmap result = null;
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                result = icon.ToBitmap();
                // try http://stackoverflow.com/a/2668620/80428
            }
            catch { }

            return result;*/
    }
}
