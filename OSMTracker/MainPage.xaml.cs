using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
namespace OSMTracker
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 将 listbox 控件的数据上下文设置为示例数据
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // 为 ViewModel 项加载数据
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            // Initialize appbar test
            ApplicationBar = App.Current.Resources["TrackingBar"] as Microsoft.Phone.Shell.ApplicationBar;

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1) return;
            if (!(e.AddedItems[0] is PanoramaItem)) return;

            PanoramaItem selectedItem = (PanoramaItem)e.AddedItems[0];

            string strTag = (string)selectedItem.Tag;
            if (strTag.Equals("tracking"))
            {
                // Do places stuff
                ApplicationBar = App.Current.Resources["TrackingBar"] as Microsoft.Phone.Shell.ApplicationBar;
            }

            else if (strTag.Equals("traces"))
            {
                // Do routes stuff
                ApplicationBar = App.Current.Resources["ManageBar"] as Microsoft.Phone.Shell.ApplicationBar;
            }

        }


    }
}