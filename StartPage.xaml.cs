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

namespace mapapp
{
    public partial class StartPage : PhoneApplicationPage
    {
        public StartPage()
        {
            InitializeComponent();
            if (App.thisApp._settings.DbStatus == DbState.Loaded)
            {
                btnMap.IsEnabled = true;
                // btnFilter.IsEnabled = true;
                // btnStats.IsEnabled = true;
            }
            App.thisApp._settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_settings_PropertyChanged);
        }

        void _settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("dbstat"))
            {
                if (App.thisApp._settings.DbStatus == DbState.Loaded)
                {
                    btnMap.IsEnabled = true;
                    // btnFilter.IsEnabled = true;
                    // btnStats.IsEnabled = true;
                }
            }
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void btnDataMgmt_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/DataManagePage.xaml", UriKind.Relative));
        }
    }
}