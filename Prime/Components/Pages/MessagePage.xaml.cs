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

namespace Prime.Components
{
    /// <summary>
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class MessagePage : UserControl, IPagable
    {
        public MessagePage()
        {
            InitializeComponent();
        }

        public MessagePage(ColumnMessages type) : this()
        {
            switch (type)
            {
                case ColumnMessages.Empty:
                    //this.AddChild(new TextBlock() { Text = "Empty" });
                    break;
                case ColumnMessages.AccessDenied:
                    //this.AddChild(new TextBlock() { Text = "Access Denied" });
                    break;
                default:
                    break;
            }
        }
    }
}
