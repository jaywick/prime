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
using System.IO;
using Prime.Components;
using Microsoft.VisualBasic.FileIO;

/*
 *  TODO:
 *  - DONE real icons (needs async)
 *  - DONE breadcrumbs
 *  - DONE empty/restricted pages
 *  - DONE refactor into user controls
 *  - soon: copy/delete etc via windows handlers
 *  - soon: save info to desktop.ini
 *  - soon: group, invert search etc keycommands
 *  - soon: scrolling list (with elipses on title)
 *  - soon: history
 *  - future: convert png pasted to icon
 *  - future: custom toolbar
 *  - future: custom menus
 *  - future: css
 *  - DONE settings
 */

namespace Prime
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;

            LayoutRoot.MouseLeftButtonDown += topStack_MouseLeftButtonDown;
            buttonMenu.Click += buttonMenu_Click;
            buttonHome.Click += buttonHome_Click;

            this.Width = Preferences.Instance.WindowWidth;
            this.Height = Preferences.Instance.WindowHeight;
            this.Left = Preferences.Instance.WindowPositionX;
            this.Top = Preferences.Instance.WindowPositionY;

            staggeredStart();
        }

        void buttonMenu_Click(object sender, RoutedEventArgs e)
        {
            //FileSystem.CopyDirectory(@"C:\Users\Jay\Downloads", @"C:\Users\Jay\Downloads2", UIOption.AllDialogs);
            //FileSystem.
        }

        void topStack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized) return;
            Preferences.Instance.WindowPositionX = Application.Current.MainWindow.Left;
            Preferences.Instance.WindowPositionY = Application.Current.MainWindow.Top;
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var NUM_OF_BUTTONS = 5;
            var menuButtonWidth = buttonMenu.ActualWidth;
            borderUrl.Width = this.ActualWidth - (menuButtonWidth * NUM_OF_BUTTONS); //left - menuButtonWidth - buffer;

            if (this.WindowState == System.Windows.WindowState.Maximized) return;
            Preferences.Instance.WindowWidth = Application.Current.MainWindow.ActualWidth;
            Preferences.Instance.WindowHeight = Application.Current.MainWindow.ActualHeight;
        }

        void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            staggeredStart();
        }

        /*private void checkWidth()
        {
            if (columns.Count == 0) return;

            var transform = columns.Last().TransformToVisual(this.LayoutRoot);
            double right = transform.Transform(new Point(0, 0)).X + columns.Last().ActualWidth;

            if (right > this.Width)
                this.SizeToContent = System.Windows.SizeToContent.Width;
        }*/

        #region Address Bar
        //TODO: make into user control
        private void displayAddressBar(bool rich = true)
        {
            var path = columnStack.CurrentDirectory.Path;
            dockAddress.Children.Clear();

            if (!rich)
            {
                textAddress.Text = path;
                textAddress.Visibility = System.Windows.Visibility.Visible;
                dockAddress.Visibility = System.Windows.Visibility.Collapsed;

                return;
            }
            dockAddress.Visibility = System.Windows.Visibility.Visible;
            textAddress.Visibility = System.Windows.Visibility.Collapsed;

            // divide by '/' symbol
            var pathChunks = path.Replace('\\', '/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int position = 0;
            foreach (var chunk in pathChunks)
            {
                string crumbText = chunk;
                //TODO: assign each part a tag to its acnestral path
                Brush textcolor = Brushes.Gray;
                if (chunk == pathChunks.First())
                {
                    var d = new DriveInfo(chunk);
                    crumbText = d.VolumeLabel;
                    if (crumbText == "" && chunk.Substring(0, 1) == Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0, 1))
                        crumbText = "Local Disk";
                    else if (crumbText == "")
                        crumbText = d.DriveType.ToString() + " (" + chunk + ")";

                    textcolor = Brushes.DarkGreen;
                }
                else if (chunk == pathChunks.Last())
                {
                    textcolor = Brushes.Black;
                }

                var collectedPath = string.Join("\\", pathChunks.ToArray<String>(), 0, position + 1) + "\\";

                // hyperlink
                var link = new Hyperlink(new Run(crumbText));
                link.RequestNavigate += link_RequestNavigate;
                link.Tag = "";
                link.NavigateUri = new Uri(collectedPath);
                link.IsEnabled = true;
                link.Foreground = textcolor;
                link.Cursor = Cursors.Hand;

                var linkContainer = new TextBlock(link);
                linkContainer.Cursor = Cursors.Hand;
                linkContainer.ForceCursor = true;

                var tbslash = new TextBlock() { Text = @"\", Foreground = Brushes.LightGray, Padding = new Thickness(3, 0, 3, 0) };
                linkContainer.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                linkContainer.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tbslash.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                tbslash.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                dockAddress.Children.Add(linkContainer);
                dockAddress.Children.Add(tbslash);

                position++;
            }
        }

        void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var link = (Hyperlink)sender;
            columnStack.Navigate(link);
        }

        private void rtfAddress_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            displayAddressBar(rich: false);
        }

        private void dockAddress_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(e.NewFocus is Hyperlink))
            {
                displayAddressBar(rich: false);
                textAddress.Focus();
                textAddress.SelectAll();
            }
        }

        private void dockAddress_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            displayAddressBar(rich: true);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            displayAddressBar(rich: false);
            textAddress.Focus();
            textAddress.SelectAll();
        }

        #endregion

        void staggeredStart()
        {
            // setup column stack
            columnStack.Initialise(new Prime.Directory(Preferences.Instance.HomeFolderPath));

            // subscribe to events
            columnStack.ColumnDirectoryChanged += columnStack_ColumnDirectoryChanged;

        }

        void columnStack_ColumnDirectoryChanged(Directory newPath)
        {
            displayAddressBar(rich: true);
        }

        private void textAddress_LostMouseCapture(object sender, MouseEventArgs e)
        {
        }
    }
}