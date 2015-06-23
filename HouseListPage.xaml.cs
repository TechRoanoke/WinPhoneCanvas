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
using Microsoft.Phone.Shell;



namespace mapapp
{
    public enum OddEvenSortOrder
    {
        None,
        OddFirst,
        OddFirstDescending,
        EvenFirst,
        EvenFirstDescending
    }

    public partial class HouseListPage : PhoneApplicationPage
    {
        GenericGroupDescriptor<PushpinModel, string> groupByStreet;
        GenericSortDescriptor<PushpinModel, int> sortByHouseNumber;

        // public bool EnableOddEven = false;

        private OddEvenSortOrder oddEvenSortOrder = OddEvenSortOrder.None;

        private List<PushpinModel> initialVoterList;

        public HouseListPage()
        {
            initialVoterList = App.VotersViewModel.VoterList.ToList();
            InitializeComponent();
            oddEvenSortOrder = OddEvenSortOrder.None;
            DataContext = App.VotersViewModel;

            // lstVoters.GroupPickerItemTap += new EventHandler<Telerik.Windows.Controls.GroupPickerItemTapEventArgs>(lstVoters_GroupPickerItemTap);

            groupByStreet = new GenericGroupDescriptor<PushpinModel, string>(voter => voter.Street);
            this.lstVoters.GroupDescriptors.Add(groupByStreet);
            sortByHouseNumber = new GenericSortDescriptor<PushpinModel, int>(voter => voter.HouseNum);
            sortByHouseNumber.SortMode = ListSortMode.Ascending;
            this.lstVoters.SortDescriptors.Add(sortByHouseNumber);
            // EnableOddEven = (lstVoters.Groups.Count() <= 1);

            UpdateOddEvenButtonEnabled();
            /*
            ApplicationBarMenuItem menuitemClearFilter = new ApplicationBarMenuItem("clear filter");
            menuitemClearFilter.Click += menuitemClearFilter_Click;
            ApplicationBar.MenuItems.Add(menuitemClearFilter);
             * */
        }

        private void UpdateOddEvenButtonEnabled()
        {
            ApplicationBarIconButton btnOddEven = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            if (btnOddEven != null)
            {
                btnOddEven.IsEnabled = (lstVoters.Groups.Count() <= 1);
            }
        }

        private void menuitemClearFilter_Click(object sender, EventArgs e)
        {
            ResetVoterList();
        }

        private void ResetVoterList()
        {
            App.VotersViewModel.FillVoterList(initialVoterList);
            ResetFilters();
        }

        private void ResetFilters()
        {
            lstVoters.FilterDescriptors.Clear();
            lstVoters.SortDescriptors.Clear();
            sortByHouseNumber.SortMode = ListSortMode.Ascending;
            lstVoters.SortDescriptors.Add(sortByHouseNumber);
            oddEvenSortOrder = OddEvenSortOrder.None;
            UpdateOddEvenButtonEnabled();
        }

        void lstVoters_GroupPickerItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
        {
            e.DataItemToNavigate = this.lstVoters.Groups.ElementAt(0);
            e.DataItemToNavigate = lstVoters.Groups.FirstOrDefault<DataGroup>(group => Object.Equals(group.Key.ToString(), e.DataItem.ToString()));
        }

        private void ApplicationBarIconButtonSortUp_Click(object sender, EventArgs e)
        {
            lstVoters.SortDescriptors.Clear();
            sortByHouseNumber.SortMode = ListSortMode.Ascending;
            lstVoters.SortDescriptors.Add(sortByHouseNumber);
        }

        private void ApplicationBarIconButtonSortDown_Click(object sender, EventArgs e)
        {
            lstVoters.SortDescriptors.Clear();
            sortByHouseNumber.SortMode = ListSortMode.Descending;
            lstVoters.SortDescriptors.Add(sortByHouseNumber);
        }

        private void ApplicationBarIconButtonSortOddEven_Click(object sender, EventArgs e)
        {
            // Tapping this button more than once will cycle through four different sort orders, 
            // OddFirst (1 3 5 7 8 6 4 2) -> EvenFirst (2 4 6 8 7 5 3 1) -> OddFirstDescending (7 5 3 1 2 4 6 8) -> EvenFirstDescending (8 6 4 2 1 3 5 7) -> OddFirst...
            // The first tap will always start the cycle with the OddFirst sorting, i.e., 1 3
            if (lstVoters.Groups != null && lstVoters.Groups.Count() == 1 && lstVoters.Groups[0] != null)
            {
                if (oddEvenSortOrder == OddEvenSortOrder.OddFirst || oddEvenSortOrder == OddEvenSortOrder.OddFirstDescending)
                {
                    // Just reverse the existing list
                    List<PushpinModel> newList = App.VotersViewModel.VoterList.ToList();
                    newList.Reverse();
                    App.VotersViewModel.VoterList.Clear();
                    this.lstVoters.SortDescriptors.Clear();
                    foreach (PushpinModel _p in newList)
                    {
                        App.VotersViewModel.VoterList.Add(_p);
                    }
                    oddEvenSortOrder = (oddEvenSortOrder == OddEvenSortOrder.OddFirst) ? OddEvenSortOrder.EvenFirst : OddEvenSortOrder.EvenFirstDescending;
                }
                else
                {
                    string filterStreet = lstVoters.Groups[0].Key as string;
                    List<PushpinModel> filteredEven = App.VotersViewModel.VoterList.Where<PushpinModel>(voter => voter.Street == filterStreet && voter.IsEven).OrderBy<PushpinModel, int>(voter => voter.HouseNum).ToList();
                    List<PushpinModel> filteredOdd = App.VotersViewModel.VoterList.Where<PushpinModel>(voter => voter.Street == filterStreet && !voter.IsEven).OrderBy<PushpinModel, int>(voter => voter.HouseNum).ToList();

                    if (oddEvenSortOrder == OddEvenSortOrder.None || oddEvenSortOrder == OddEvenSortOrder.EvenFirstDescending)
                    {
                        // Target sort is OddFirst (ascending)
                        oddEvenSortOrder = OddEvenSortOrder.OddFirst;
                        filteredEven.Reverse();
                    }
                    else // Current order is EvenFirst, switch to OddFirstDescending
                    {
                        oddEvenSortOrder = OddEvenSortOrder.OddFirstDescending;
                        filteredOdd.Reverse();
                    }
                    App.VotersViewModel.VoterList.Clear();
                    this.lstVoters.SortDescriptors.Clear();
                    // First add odd houses to list
                    foreach (PushpinModel _p in filteredOdd)
                    {
                        App.VotersViewModel.VoterList.Add(_p);
                    }
                    // Now add even houses to list
                    foreach (PushpinModel _p in filteredEven)
                    {
                        App.VotersViewModel.VoterList.Add(_p);
                    }
                }
            }
            lstVoters.FilterDescriptors.Clear();
        }

        private void lstVoters_ItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            if (this.lstVoters.SelectedItem is PushpinModel)
            {
                PushpinModel _pin = this.lstVoters.SelectedItem as PushpinModel;
                App.thisApp.SelectedHouse = _pin;
                if (this.lstVoters.FilterDescriptors.Count > 0 && lstVoters.Groups != null && lstVoters.Groups[0] != null)
                {
                    string filterStreet = lstVoters.Groups[0].Key as string;
                    // TODO: Determine whether including OrderBy here loses our odd/even sorting
                    List<PushpinModel> filteredPins = App.VotersViewModel.VoterList.Where<PushpinModel>(voter => voter.Street == filterStreet).OrderBy<PushpinModel, int>(voter => voter.HouseNum).ToList();
                    if (sortByHouseNumber.SortMode == ListSortMode.Descending)
                        filteredPins.Reverse();
                    App.VotersViewModel.VoterList.Clear();
                    foreach (PushpinModel _p in filteredPins)
                    {
                        App.VotersViewModel.VoterList.Add(_p);
                    }
                }
                this.NavigationService.Navigate(new Uri("/VoterDetailsPage.xaml", UriKind.Relative));
            }
        }

        private void lstVoters_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (lstVoters.FilterDescriptors.Count > 0)
            {
                ResetFilters();
            }
            else if (oddEvenSortOrder != OddEvenSortOrder.None)
            {
                ResetVoterList();
            }
            else 
            {
                if (e.OriginalSource.GetType() == typeof(TextBlock))
                {
                    if (App.VotersViewModel.StreetList.Contains(((TextBlock)e.OriginalSource).Text))
                    {
                        GenericFilterDescriptor<PushpinModel> filterByStreet = new GenericFilterDescriptor<PushpinModel>(voter => voter.Street == ((TextBlock)e.OriginalSource).Text);
                        lstVoters.FilterDescriptors.Add(filterByStreet);
                        // EnableOddEven = true;
                        UpdateOddEvenButtonEnabled();
                    }
                }
            }
        }

        private void lstVoters_GroupHeaderItemTap(object sender, Telerik.Windows.Controls.GroupHeaderItemTapEventArgs e)
        {
            if (oddEvenSortOrder != OddEvenSortOrder.None)
                ResetVoterList();
            else if (lstVoters.FilterDescriptors.Count > 0)
            {
                ResetFilters();
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
    public class PartyBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush _bg = new SolidColorBrush(Colors.White);
            if ((value is int?) && (targetType == typeof(Brush)))
            {
                int partyNum = (int)value;
                switch (partyNum)
                {
                    case 1:
                        _bg = new SolidColorBrush(Colors.Red);
                        break;
                    case 2:
                        _bg = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x88, 0x88));
                        break;
                    case 3:
                        _bg = new SolidColorBrush(Colors.Purple);
                        break;
                    case 4:
                        _bg = new SolidColorBrush(Color.FromArgb(0xff, 0x88, 0x88, 0xff));
                        break;
                    case 5:
                        _bg = new SolidColorBrush(Colors.Blue);
                        break;
                    case 6:
                        _bg = new SolidColorBrush(Colors.Black);
                        break;
                    default:
                        break;
                }
            }
            return _bg;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    public class PartyForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush _fg = new SolidColorBrush(Colors.Black);
            if ((value is int?) && (targetType == typeof(Brush)))
            {
                int partyNum = (int)value;
                switch (partyNum)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        _fg = new SolidColorBrush(Colors.White);
                        break;
                    default:
                        break;
                }
            }
            return _fg;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

}