using ClientVideoStream.ViewModels;
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

namespace ClientVideoStream.Pages
{
    /// <summary>
    /// Interaction logic for MediaPlayerPage.xaml
    /// </summary>
    public partial class MediaPlayerPage : Page
    {
        public MediaPlayerPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((MediaPlayerPageModel)MainGrid.DataContext).SetController(mediaController);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ((MediaPlayerPageModel)MainGrid.DataContext).OnEnter();
            }
        }
    }
}
