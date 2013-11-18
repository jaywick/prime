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
    /// <summary>
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class InfoPage : UserControl, IPagable
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        public InfoPage(FileSystemItem[] selection)
            : this()
        {
            var items = new List<FileSystemItem>(selection);
            text1.Text = items.Count + " items";
            
            var itemCountsText = new List<string>();
            int folderCount = items.Count(v => v.Type == ItemTypes.Folder);
            int fileCount = items.Count(v => v.Type == ItemTypes.File);
            int shortcutCount = items.Count(v => v.Type == ItemTypes.FileShortcut || v.Type == ItemTypes.FolderShortcut);
            if (folderCount > 0) itemCountsText.Add(folderCount + " folders");
            if (fileCount > 0) itemCountsText.Add(folderCount + " files");
            if (shortcutCount > 0) itemCountsText.Add(folderCount + " shortcuts");
            text2.Text = string.Join(", ", itemCountsText);

            NewMethod(items);
            
            text4.Text = "That's it";
        }

        private async Task NewMethod(List<FileSystemItem> items)
        {
            long totalSize = 0;
            List<Directory> directories = items.Where(v => v.Type == ItemTypes.Folder).Cast<Directory>().ToList();
            foreach (var dir in directories)
            {
                var s = await calculateTotalSize(dir);

                // Update the UI
                totalSize += s;
                text3.Text = totalSize.ToString() + " total bytes";
            }
        }

        async Task<long> calculateTotalSize(Directory item)
        {
            //await Task.Delay(2000);
            //return i.ToString() + " Call - " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            //return 1000;
            return await item.GetSize();
        }
    }
}
