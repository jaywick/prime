using System;
using System.Collections.Generic;
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
using Prime.FileSystem;

namespace Prime.Components
{
    public delegate void ItemActionEventHandler(ListPageItem listItem, FileSystemItem reference);

    public partial class ListPageItem : UserControl
    {
        FileSystemItem reference;

        public FileSystemItem Reference { get { return reference; } }
        public ItemTypes ItemType { get { return reference.Type; } }

        public event ItemActionEventHandler ItemDoubleClicked;
        public event ItemActionEventHandler ItemRightClicked;

        public ListPageItem()
        {
            InitializeComponent();
            this.Width = double.NaN;
        }

        public ListPageItem(FileSystemItem genericReference)
            : this()
        {
            try
            {
                reference = FileSystemItem.DeriveType(genericReference.Path);

                if (reference is File)
                {
                    var fileRef = (File)reference;
                    textFileName.Inlines.Add(new Run(fileRef.GetNameWithoutExtension()));
                    var r = new Run(fileRef.Extension);
                    r.Foreground = Brushes.Gray;
                    textFileName.Inlines.Add(r);
                }
                else
                {
                    textFileName.Inlines.Add(new Run(reference.Name));
                }

                iconBlank.Source = getIcon(reference);
            }
            catch (Exception)
            {
                textFileName.Text = genericReference.Name;
                iconBlank.Source = getIcon();
            }

            iconBlank.Visibility = System.Windows.Visibility.Visible;

            if (genericReference.IsShortcut)
                textFileName.FontStyle = FontStyles.Italic;

            /*switch (this.ItemType)
            {
                case ItemTypes.File:
                    iconBlank.Visibility = System.Windows.Visibility.Visible;
                    break;
                case ItemTypes.Folder:
                    iconFolder.Visibility = System.Windows.Visibility.Visible;
                    break;
                case ItemTypes.FolderShortcut:
                    iconFolder.Visibility = System.Windows.Visibility.Visible;
                    textFileName.FontStyle = FontStyles.Italic;
                    break;
                case ItemTypes.FileShortcut:
                    iconBlank.Visibility = System.Windows.Visibility.Visible;
                    textFileName.FontStyle = FontStyles.Italic;
                    break;
                default:
                    break;
            }*/

        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            if (ItemDoubleClicked != null)
                ItemDoubleClicked.Invoke(this, reference);
        }

        private void UserControl_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ItemRightClicked != null)
                ItemRightClicked.Invoke(this, reference);
        }

        public bool IsTarget { get; set; }

        private ImageSource getIcon(FileSystemItem item = null)
        {
            string header = "pack://application:,,,/Prime;component/Resources/";
            string icon = "modern-files/file.png";

            if (item == null)
                icon = "empty.png";
            else if (item is Drive)
                icon = "drive.png";
            else if (item is Directory || item is Jump)
                //icon = "folder.png";
                //icon = "folder-vi.png";
                icon = "modern-files/folder.png";
            //icon = "dir32.png";
            else
            {
                string ext = (item as File).Extension;
                switch (ext.ToLower())
                {
                    case "":
                        icon = "empty.png";
                        break;
                    case ".doc":
                    case ".docx":
                    case ".rtf":
                        icon = "modern-files/doc.png";
                        break;
                    /*case ".xls":
                    case ".xlsx":
                        icon = "excel.png";
                        break;
                    case ".ppt":
                    case ".pptx":
                        icon = "slides.png";
                        break;*/
                    case ".jar":
                    case ".exe":
                    case ".bat":
                    case ".msi":
                    case ".clickonce":
                    case ".com":
                        icon = "modern-files/exe.png";
                        break;
                    case ".img":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".ico":
                        icon = "modern-files/img.png";
                        break;
                    case ".pdf":
                        icon = "modern-files/doc.png";
                        break;
                    /*case ".dll":
                    case ".lib":
                    case ".sys":
                        icon = "plugin.png";
                        break;
                    case ".html":
                    case ".htm":
                    case ".url":
                        icon = "url.png";
                        break;*/
                    case ".wma":
                    case ".wav":
                    case ".mp3":
                    case ".ogg":
                        icon = "modern-files/vid.png";
                        break;
                    case ".zip":
                    case ".rar":
                    case ".gz":
                    case ".tar":
                    case ".7z":
                        icon = "modern-files/zip.png";
                        break;
                    case ".avi":
                    case ".mkv":
                    case ".mov":
                    case ".wmv":
                    case ".mp4":
                    case ".mpeg":
                    case ".mpg":
                    case ".vob":
                        icon = "modern-files/vid.png";
                        break;
                    case ".php":
                    case ".cs":
                    case ".java":
                    case ".js":
                    case ".ini":
                    case ".vb":
                    case ".frm":
                    case ".frx":
                    case ".txt":
                    case ".text":
                    case ".py":
                    case ".c":
                    case ".h":
                    case ".cpp":
                    case ".vbs":
                    case ".sql":
                    case ".xml":
                    case ".xhtml":
                    case ".manifest":
                        icon = "modern-files/code.png";
                        break;
                    default:
                        break;
                }
            }
            return new BitmapImage(new Uri(header + icon, UriKind.Absolute));
        }

        public bool Selected
        {
            set
            {
                if (value)
                {
                    textFileName.Foreground = Brushes.White;
                }
                else
                {
                    textFileName.Foreground = Brushes.Black;
                }
            }
        }

    }
}
