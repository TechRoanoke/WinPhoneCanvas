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
using mapapp.data;

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
                btnList.IsEnabled = true;
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
                    btnList.IsEnabled = true;
                }
                else
                {
                    btnMap.IsEnabled = false;
                    btnList.IsEnabled = false;
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

        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            if (App.VotersViewModel.VoterList == null || App.VotersViewModel.VoterList.Count == 0)
            {
                App.VotersViewModel.FillVoterList(null);
                if (App.VotersViewModel.VoterList.Count() <= 0)
                {
                    btnList.IsEnabled = false;
                }
            }
            this.NavigationService.Navigate(new Uri("/HouseListPage.xaml", UriKind.Relative));
        }
    }
}