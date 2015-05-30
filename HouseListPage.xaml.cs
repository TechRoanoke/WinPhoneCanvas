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
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;
using Telerik.Windows.Data;


namespace mapapp
{
    public partial class HouseListPage : PhoneApplicationPage
    {
        CollectionViewSource cvsVoters = null;
        SortDescription sortOnHouseNum;
        GroupDescription groupOddEven;

        public HouseListPage()
        {
            InitializeComponent();

            lstVoters.GroupPickerItemTap += new EventHandler<Telerik.Windows.Controls.GroupPickerItemTapEventArgs>(lstVoters_GroupPickerItemTap);

            GenericGroupDescriptor<PushpinModel, string> groupByStreet = new GenericGroupDescriptor<PushpinModel, string>(voter => voter.Street);
            this.lstVoters.GroupDescriptors.Add(groupByStreet);
            GenericSortDescriptor<PushpinModel, int> sortByHouseNumber = new GenericSortDescriptor<PushpinModel, int>(voter => voter.HouseNum);
            this.lstVoters.SortDescriptors.Add(sortByHouseNumber);
            /*
            GenericFilterDescriptor<PushpinModel> filterLegRaces = new GenericFilterDescriptor<PushpinModel>((PushpinModel voter) =>
            {
                if (voter != null && voter.IsEven)
                    return true;
                else
                    return false;
            });
            this.lstVoters.FilterDescriptors.Add(filterLegRaces);
            */

            // groupOddEven = new PropertyGroupDescription("IsEven");

            DataContext = App.VotersViewModel;

        }

        void lstVoters_GroupPickerItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
        {
            e.DataItemToNavigate = this.lstVoters.Groups.ElementAt(0);
            e.DataItemToNavigate = lstVoters.Groups.FirstOrDefault<DataGroup>(group => Object.Equals(group.Key.ToString(), e.DataItem.ToString()));
        }

        private void ApplicationBarIconButtonSortUp_Click(object sender, EventArgs e)
        {
            // cvsVoters.SortDescriptions.Clear();
            // sortOnHouseNum.Direction = ListSortDirection.Ascending;
            // cvsVoters.SortDescriptions.Add(sortOnHouseNum);
        }

        private void ApplicationBarIconButtonSortDown_Click(object sender, EventArgs e)
        {
            // cvsVoters.SortDescriptions.Clear();
            // sortOnHouseNum.Direction = ListSortDirection.Descending;
            // cvsVoters.SortDescriptions.Add(sortOnHouseNum);
        }

        private void ApplicationBarIconButtonSortOddEven_Click(object sender, EventArgs e)
        {
            // bool bIsGrouped = cvsVoters.GroupDescriptions.Count > 0;
            // cvsVoters.GroupDescriptions.Clear();
            // if (!bIsGrouped)
            //     cvsVoters.GroupDescriptions.Add(groupOddEven);
        }

        private void lstVoters_ItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            if (this.lstVoters.SelectedItem is PushpinModel)
            {
                PushpinModel _pin = this.lstVoters.SelectedItem as PushpinModel;
                App.thisApp.SelectedHouse = _pin.VoterFile;
                this.NavigationService.Navigate(new Uri("/VoterDetailsPage.xaml", UriKind.Relative));
            }
        }
    }

    // This converter converts between boolean true/false data values and control visibility Visible/Collapsed values
    public class BoolVizConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if (value is bool?) return ((bool)value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Visible;
            if (value is bool?) return ((bool)value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            return System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Visibility)
                return (value.Equals(System.Windows.Visibility.Visible)) ? true : false;
            return false;
        }
    }

    // This converter converts between boolean true/false data values and control opacity level (0.4/1.0) values
    // Currently this is used in the voter list to show voters that have been updated (contacted) with a reduced
    // opacity so that those voters appear dimmed in the list.
    public class BoolOpaqueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool?) return ((bool)value) ? 0.4 : 1.0;
            return 1.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
                return ((double) value < 1.0) ? true : false;
            return false;
        }
    }

}