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
using System.Windows.Shapes;

namespace Prime
{
    /// <summary>
    /// Interaction logic for AlterView.xaml
    /// </summary>
    public partial class AlterView : Window
    {
        const string debug_firstlocation = @"c:\users\jay\dropbox";

        public AlterView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tabs.AddTabClick += tabs_AddTabClick;
        }

        void tabs_AddTabClick()
        {
            tabs.AddTab(new Prime.Components.ColumnStack(new Directory(debug_firstlocation)), true);
        }
        
    }
}
