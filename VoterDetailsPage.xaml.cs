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
using Microsoft.Phone.Shell;
using mapapp.data;


namespace mapapp
{
    public class UpdateResult
    {
        public string VersionTag { get; set; }
        public UpdateResult() { }
        public UpdateResult(string version)
        {
            VersionTag = version;
        }
    }

    public partial class VoterDetailsPage : PhoneApplicationPage
    {
        ApplicationBar _detailsAppBar = new ApplicationBar();

        VoterFileDataContext _voterDB = null;

        VoterViewModel VotersInViewList = App.VotersViewModel;

        int _VoterIndexInList = 0;
        bool _CurrentVoterModified = false;

        public VoterDetailsPage()
        {
            InitializeComponent();

            _detailsAppBar.Mode = ApplicationBarMode.Default;
            _detailsAppBar.Opacity = 1.0;
            _detailsAppBar.IsVisible = true;
            _detailsAppBar.IsMenuEnabled = false;

            ApplicationBarIconButton buttonPrevious = new ApplicationBarIconButton();
            buttonPrevious.IconUri = new Uri("/Images/appbar.back.rest.png", UriKind.Relative);
            buttonPrevious.Text = "previous";
            buttonPrevious.Click += buttonPrevious_Click;
            _detailsAppBar.Buttons.Add(buttonPrevious);

            ApplicationBarIconButton buttonCancel = new ApplicationBarIconButton();
            buttonCancel.IconUri = new Uri("/Images/appbar.cancel.rest.png", UriKind.Relative);
            buttonCancel.Text = "cancel";
            buttonCancel.Click += cancelButton_Click;
            _detailsAppBar.Buttons.Add(buttonCancel);

            ApplicationBarIconButton buttonNext = new ApplicationBarIconButton();
            buttonNext.IconUri = new Uri("/Images/appbar.next.rest.png", UriKind.Relative);
            buttonNext.Text = "next";
            buttonNext.Click += buttonNext_Click;
            _detailsAppBar.Buttons.Add(buttonNext);

            ApplicationBar = _detailsAppBar;

            if (VotersInViewList.VoterList.Count > 1)
            {
                int CurrVoterIndex = VotersInViewList.VoterList.IndexOf(App.thisApp.SelectedHouse);
                if (CurrVoterIndex >= 0)
                {
                    _VoterIndexInList = CurrVoterIndex;
                }
            }
            DisplayVoter(App.thisApp.SelectedHouse);
        }

        void buttonNext_Click(object sender, EventArgs e)
        {
            if (_CurrentVoterModified)
                SaveVoterChanges();
            if (_VoterIndexInList < (VotersInViewList.VoterList.Count-1))
            {
                _VoterIndexInList++;
                DisplayVoter(VotersInViewList.VoterList[_VoterIndexInList]);
                App.thisApp.SelectedHouse = VotersInViewList.VoterList[_VoterIndexInList];
            }
        }

        void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (_CurrentVoterModified)
                SaveVoterChanges();
            if (_VoterIndexInList > 0)
            {
                _VoterIndexInList--;
                DisplayVoter(VotersInViewList.VoterList[_VoterIndexInList]);
                App.thisApp.SelectedHouse = VotersInViewList.VoterList[_VoterIndexInList];
            }
        }

        void voterToUpdate_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            // Flag voter object as being "dirty", then any change to the voter being viewed needs to trigger an update
            App.Log("voter object property changed : " + e.PropertyName);
            _CurrentVoterModified = true;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApplicationBar != _detailsAppBar)
                ApplicationBar = _detailsAppBar;
        }

        private void ListPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                App.Log(" Problem in ListPicker_Tap: " + ex);
            }
        }

        private void partyList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnlParty != null)
            {
                pnlParty.Height = e.NewSize.Height+20;
            }
            if (ApplicationBar != _detailsAppBar)
                ApplicationBar = _detailsAppBar;
        }

        private void resultList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnlContact != null)
            {
                pnlContact.Height = e.NewSize.Height + 20;
                if (resultList.IsExpanded)
                {
                    pnlEmail.Visibility = System.Windows.Visibility.Collapsed;
                    pnlParty.Visibility = System.Windows.Visibility.Collapsed;
                }
                else 
                {
                    pnlEmail.Visibility = System.Windows.Visibility.Visible;
                    pnlParty.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public void DisplayVoter(PushpinModel _currVoter)
        {
            DataContext = _currVoter;
            _CurrentVoterModified = false;
            // TODO: Determine whether we really need to open the database at this point
            _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, App.thisApp._settings.DbFileName));
            if (_voterDB.DatabaseExists())
            {
                try
                {
                    IQueryable<VoterFileEntry> voterQuery = from voter in _voterDB.AllVoters where voter.VoterID == _currVoter.VoterID select voter;
                    VoterFileEntry voterToUpdate = voterQuery.FirstOrDefault();
                    if (voterToUpdate.VoterID == _currVoter.VoterID)
                    {
                        DataContext = voterToUpdate;
                        voterToUpdate.PropertyChanging += voterToUpdate_PropertyChanging;
                        App.Log(String.Format("Setting Voter Detail page DataContext to voter {0}", voterToUpdate.FullName));
                    }
                    else
                    {
                        App.Log(String.Format("ERROR: Setting Voter Detail page DataContext to voter {0} failed.", voterToUpdate.FullName));
                    }
                }
                catch (Exception ex)
                {
                    App.Log("Exception setting voter as DataContext" + ex.ToString());
                }
            }
            /*
            */
            ApplicationBarIconButton buttonPrevious = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            ApplicationBarIconButton buttonNext = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            if (VotersInViewList.VoterList.Count > 1)
            {
                if (_VoterIndexInList > 0)
                    buttonPrevious.IsEnabled = true;
                else
                    buttonPrevious.IsEnabled = false;
                if (_VoterIndexInList < (VotersInViewList.VoterList.Count-1))
                    buttonNext.IsEnabled = true;
                else
                    buttonNext.IsEnabled = false;
            }
            else
            {
                buttonPrevious.IsEnabled = false;
                buttonNext.IsEnabled = false;
            }
        }

        private void SaveVoterChanges()
        {
            // If either the email or cell textbox controls had focus when save button was tapped those changes were 
            // not updated ot the view model (because focus was not lost), so we will do that now.
            if (txtEmail.Text != ((VoterFileEntry)DataContext).Email)
            {
                BindingExpression expression = txtEmail.GetBindingExpression(TextBox.TextProperty);
                expression.UpdateSource();
            }
            if (txtCell.Text != ((VoterFileEntry)DataContext).CellPhone)
            {
                BindingExpression expression = txtCell.GetBindingExpression(TextBox.TextProperty);
                expression.UpdateSource();
            }
            if (txtComments.Text != ((VoterFileEntry)DataContext).Comments)
            {
                BindingExpression expression = txtComments.GetBindingExpression(TextBox.TextProperty);
                expression.UpdateSource();
            }
            if (_voterDB.DatabaseExists())
            {
                ((VoterFileEntry)DataContext).ModifiedTime = System.DateTime.Now;
                ((VoterFileEntry)DataContext).IsUpdated = true;
                _voterDB.SubmitChanges();
                App.Log("Submitted changes to database");
            }
            else
                App.Log("ERROR: we don't have a database!");

            _voterDB.Dispose();
            // PostUpdate() will attempt to post this update (and any outstanding updates) if there is network access.
            // If the update fails, then the next change will trigger another call to PostUpdate(), which will attempt 
            // to post that update and this one.
            App.thisApp.PostUpdate();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_CurrentVoterModified)
                SaveVoterChanges();
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _voterDB.Dispose();
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (_CurrentVoterModified)
                SaveVoterChanges();
            _voterDB.Dispose();
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void pnlVoterStatic_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (pnlCityPrecinct.Visibility == System.Windows.Visibility.Collapsed)
            {
                pnlCityPrecinct.Visibility = System.Windows.Visibility.Visible;
                pnlPhone.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                pnlCityPrecinct.Visibility = System.Windows.Visibility.Collapsed;
                pnlPhone.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

    }

    public class ResultListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int limitedValue = 0; // set initially to 
            if (value is int?)
            {
                limitedValue = (int)value;
                // restrict value range to 0 to 6, with anything outside that range getting set to "unknown"
                if (limitedValue > 9 || limitedValue < 0)
                    limitedValue = 0;
            }
            return limitedValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    public class PartyListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int limitedValue = 0; // set initially to 
            if (value is int?)
            {
                limitedValue = (int)value;
                // restrict value range to 0 to 6, with anything outside that range getting set to "unknown"
                if (limitedValue > 6 || limitedValue < 0)
                    limitedValue = 0;
            }
            return limitedValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}