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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Threading;
using mapapp.data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.ComponentModel;
using System.Text;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
// using BingMapsRESTService.Common.JSON;
// using System.Runtime.Serialization;
// using System.Runtime.Serialization.Json;

namespace mapapp
{
    public partial class App : Application, INotifyPropertyChanged
    {
        private static VoterViewModel _voterViewModel = null;
        public MapAppSettings _settings = null;
        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The VoterViewModel object.</returns>
        public static VoterViewModel VotersViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (_voterViewModel == null)
                    _voterViewModel = new VoterViewModel();
                return _voterViewModel;
            }
            set
            {
                if (_voterViewModel != value)
                {
                    _voterViewModel = value;
                }
            }
        }

        private VoterFileEntry _selectedVoter;

        public VoterFileEntry SelectedHouse
        {
            get { return _selectedVoter; }
            set
            {
                if (_selectedVoter != value)
                {
                    _selectedVoter = value;
                    NotifyPropertyChanged("SelectedHouse");
                }
            }
        }

        public bool UpdateVoter()
        {
            bool success = false;
            VoterFileDataContext _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, _settings.DbFileName));
            if (_voterDB.DatabaseExists())
            {
                try
                {
                    IQueryable<VoterFileEntry> voterQuery = from voter in _voterDB.AllVoters where voter.VoterID == _selectedVoter.VoterID select voter;
                    VoterFileEntry voterToUpdate = voterQuery.FirstOrDefault();
                    if (voterToUpdate.VoterID == _selectedVoter.VoterID)
                    {
                        voterToUpdate.Party = _selectedVoter.Party;
                        voterToUpdate.ResultOfContact = _selectedVoter.ResultOfContact;
                        voterToUpdate.Email = _selectedVoter.Email;
                        voterToUpdate.CellPhone = _selectedVoter.CellPhone;
                        voterToUpdate.IsSupporter = _selectedVoter.IsSupporter;
                        voterToUpdate.IsVolunteer = _selectedVoter.IsVolunteer;
                        voterToUpdate.IsUpdated = _selectedVoter.IsUpdated;
                        voterToUpdate.Comments = _selectedVoter.Comments;
                        voterToUpdate.ModifiedTime = DateTime.Now;
                        _selectedVoter.ModifiedTime = voterToUpdate.ModifiedTime;
                        _voterDB.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                        success = true;
                    }
                    else
                        Log(String.Format(" ERROR: Voter returned from query (id={0}) did not match selected voter (id={1}).", voterToUpdate.VoterID, _selectedVoter.VoterID));
                }
                catch (Exception ex)
                {
                    Log(" Error updating voter record: " + _selectedVoter.FullName + "  : " + ex.ToString());
                }
            }
            _voterDB.Dispose();
            return success;
        }

        public bool DbHasUpdates()
        {
            bool _bReturn = false;
            VoterFileDataContext _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, _settings.DbFileName));
            if (_voterDB.DatabaseExists())
            {
                try
                {
                    IEnumerable<VoterFileEntry> voters = from voter in _voterDB.AllVoters
                                                         where voter.ModifiedTime > _settings.GetSetting<DateTime>("lastsync")
                                                         select voter;
                    if (voters.Count() > 0)
                        _bReturn = true;
                }
                catch (Exception ex)
                {
                    Log(" Error reporting changes: " + ex.ToString());
                }
            }
            _voterDB.Dispose();
            return _bReturn;
        }

        public string CreateUpdateFile()
        {
            string _updatesFileName = "";
            VoterFileDataContext _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, _settings.DbFileName));
            if (_voterDB.DatabaseExists())
            {
                try
                {
                    IEnumerable<VoterFileEntry> voters = from voter in _voterDB.AllVoters
                                                         // where voter.IsUpdated == true
                                                         where voter.ModifiedTime > _settings.GetSetting<DateTime>("lastsync")
                                                         select voter;
                    App.Log("  CreateUpdateFile Query completed. Starting write to file");

                    String stDataFormat = App.thisApp._settings.GetSetting<string>("dataformat");
                    IsolatedStorageFile _iso = IsolatedStorageFile.GetUserStoreForApplication();
                    DateTime _now = DateTime.Now;
                    string _dateString = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", _now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute, _now.Second);
                    _updatesFileName = string.Format("VoterUpdates{0}.{1}", _dateString, stDataFormat);
                    IsolatedStorageFileStream _outputStream = _iso.CreateFile(_updatesFileName);
                    StreamWriter _updateFileStream = new StreamWriter(_outputStream);

                    if (stDataFormat == "xml")
                    {
                        StringBuilder xml = new StringBuilder();
                        xml.Clear();
                        xml.AppendLine("<VoterUpdates>");
                        xml.AppendFormat("\t<ReportDate>{0}</ReportDate>{1}", DateTime.Now, System.Environment.NewLine);
                        xml.AppendLine("\t<VoterList>");
                        _updateFileStream.Write(xml.ToString());
                        xml.Clear();

                        foreach (VoterFileEntry voter in voters)
                        {
                            // TODO: Add WriteUpdateToXml() method to VoterFileEntry object
                            xml.AppendLine("\t\t<VoterUpdate>");
                            xml.AppendFormat("\t\t\t<ID>{0}</ID>{1}", voter.VoterID, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Party>{0}</Party>{1}", voter.Party, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<ResultOfContact>{0}</ResultOfContact>{1}", voter.ResultOfContact, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Email>{0}</Email>{1}", voter.Email, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<CellPhone>{0}</CellPhone>{1}", voter.CellPhone, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsSupporter>{0}</IsSupporter>{1}", voter.IsSupporter, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsVolunteer>{0}</IsVolunteer>{1}", voter.IsVolunteer, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsModified>{0}</IsModified>{1}", voter.IsUpdated, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Comments>{0}</Comments>{1}", voter.Comments, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<ModifiedTime>{0}</ModifiedTime>{1}", voter.ModifiedTime, System.Environment.NewLine);
                            xml.AppendLine("\t\t</VoterUpdate>");
                            _updateFileStream.Write(xml.ToString());
                            xml.Clear();
                        }
                        xml.AppendLine("\t</VoterList>");
                        xml.AppendLine("</VoterUpdates>");
                        _updateFileStream.Write(xml.ToString());
                        _updateFileStream.Flush();
                        _updateFileStream.Close();
                        xml.Clear();
                    }
                    else if (stDataFormat == "csv")
                    {
                        StringBuilder csv = new StringBuilder();
                        csv.Clear();
                        if (App.thisApp._settings.GetSetting<bool>("_trhack"))
                            csv.AppendLine("redid,party,resultofcontact,email,cellphone,issupporter,comments,modifiedtime");
                        else
                            csv.AppendLine("recid,party,resultofcontact,email,cellphone,issupporter,comments,modifiedtime");
                        _updateFileStream.Write(csv.ToString());
                        csv.Clear();

                        foreach (VoterFileEntry voter in voters)
                        {
                            // TODO: Add WriteUpdateToCsv() method to VoterFileEntry object
                            csv.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}{8}", voter.VoterID, voter.Party, voter.ResultOfContact, voter.Email, voter.CellPhone, voter.IsSupporter, voter.Comments, voter.ModifiedTime, System.Environment.NewLine);
                            _updateFileStream.Write(csv.ToString());
                            csv.Clear();
                        }
                        _updateFileStream.Flush();
                        _updateFileStream.Close();
                        csv.Clear();
                    }
                    Log("  Completed writing update file " + _updatesFileName);
                }
                catch (Exception ex)
                {
                    Log(" Error reporting changes: " +  ex.ToString());
                    _updatesFileName = "";
                }
            }
            _voterDB.Dispose();
            return _updatesFileName;
        }

        public string GetUpdateString()
        {
            string stUpdates = "";
            VoterFileDataContext _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, _settings.DbFileName));
            if (_voterDB.DatabaseExists())
            {
                try
                {
                    IEnumerable<VoterFileEntry> voters = from voter in _voterDB.AllVoters
                                                         // where voter.IsUpdated == true
                                                         where voter.ModifiedTime > _settings.GetSetting<DateTime>("lastsync")
                                                         select voter;
                    App.Log("  GetUpdateString Query completed.");

                    String stDataFormat = App.thisApp._settings.GetSetting<string>("dataformat");
                    DateTime _now = DateTime.Now;
                    string _dateString = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", _now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute, _now.Second);
                    // StreamWriter _updateFileStream = new StreamWriter(_outputStream);

                    StringWriter _updateFileString = new StringWriter();

                    if (stDataFormat == "xml")
                    {
                        StringBuilder xml = new StringBuilder();
                        xml.Clear();
                        xml.AppendLine("<VoterUpdates>");
                        xml.AppendFormat("\t<ReportDate>{0}</ReportDate>{1}", DateTime.Now, System.Environment.NewLine);
                        xml.AppendLine("\t<VoterList>");
                        _updateFileString.Write(xml.ToString());
                        xml.Clear();

                        foreach (VoterFileEntry voter in voters)
                        {
                            // TODO: Add WriteUpdateToXml() method to VoterFileEntry object
                            xml.AppendLine("\t\t<VoterUpdate>");
                            xml.AppendFormat("\t\t\t<ID>{0}</ID>{1}", voter.VoterID, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Party>{0}</Party>{1}", voter.Party, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<ResultOfContact>{0}</ResultOfContact>{1}", voter.ResultOfContact, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Email>{0}</Email>{1}", voter.Email, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<CellPhone>{0}</CellPhone>{1}", voter.CellPhone, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsSupporter>{0}</IsSupporter>{1}", voter.IsSupporter, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsVolunteer>{0}</IsVolunteer>{1}", voter.IsVolunteer, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<IsModified>{0}</IsModified>{1}", voter.IsUpdated, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<Comments>{0}</Comments>{1}", voter.Comments, System.Environment.NewLine);
                            xml.AppendFormat("\t\t\t<ModifiedTime>{0}</ModifiedTime>{1}", voter.ModifiedTime, System.Environment.NewLine);
                            xml.AppendLine("\t\t</VoterUpdate>");
                            _updateFileString.Write(xml.ToString());
                            xml.Clear();
                        }
                        xml.AppendLine("\t</VoterList>");
                        xml.AppendLine("</VoterUpdates>");
                        _updateFileString.Write(xml.ToString());
                        _updateFileString.Flush();
                        stUpdates = _updateFileString.ToString();
                        _updateFileString.Close();
                        xml.Clear();
                    }
                    else if (stDataFormat == "csv")
                    {
                        StringBuilder csv = new StringBuilder();
                        csv.Clear();
                        if (App.thisApp._settings.GetSetting<bool>("_trhack"))
                            csv.AppendLine("redid,party,resultofcontact,email,cellphone,issupporter,comments,modifiedtime");
                        else
                            csv.AppendLine("recid,party,resultofcontact,email,cellphone,issupporter,comments,modifiedtime");
                        _updateFileString.Write(csv.ToString());
                        csv.Clear();

                        foreach (VoterFileEntry voter in voters)
                        {
                            // TODO: Add WriteUpdateToCsv() method to VoterFileEntry object
                            csv.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}{8}", voter.VoterID, voter.Party, voter.ResultOfContact, voter.Email, voter.CellPhone, voter.IsSupporter, voter.Comments, voter.ModifiedTime, System.Environment.NewLine);
                            _updateFileString.Write(csv.ToString());
                            csv.Clear();
                        }
                        _updateFileString.Flush();
                        stUpdates = _updateFileString.ToString();
                        _updateFileString.Close();
                        csv.Clear();
                    }
                    Log("  Completed: Updates = " + stUpdates);
                }
                catch (Exception ex)
                {
                    Log(" Error reporting changes: " + ex.ToString());
                    stUpdates = "";
                }
            }
            _voterDB.Dispose();
            return stUpdates;
        }

        // private static VoterFileDataContext _voterDB;

        Thread _dbLoadThread;
        /*
        private bool _dbLoaded;

        public bool IsDbLoaded
        {
            get { return _dbLoaded; }
            set
            {
                _dbLoaded = value;
                this.NotifyPropertyChanged("IsDbLoaded");
            }
        }
         */

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }


        public static App thisApp
        {
            get;
            set;
        }

        public static Thread MainThread
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            _settings = new MapAppSettings();
            
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            thisApp = this;
            MainThread = Thread.CurrentThread;
            // Create the database if it does not exist.

            // Changing from initially loading voters.xml from static resource to getting it from SkyDrive
            // _dbLoadThread = new Thread(LoadDatabase);
            // _dbLoadThread.Start("voters.xml");
            if (_settings.DbStatus != DbState.Loaded)
            {

            }
            Thread.Sleep(1500);
        }

        public void LoadDatabaseFromFile(string fileName)
        {
            if (_settings.DbStatus == DbState.Loaded)
            {
                // TODO: Handle other states
                return;
            }
            else
            {
                if (App.VotersViewModel != null)
                {
                    App.VotersViewModel.StreetList.Clear();
                    App.VotersViewModel.VoterList.Clear();
                    App.VotersViewModel.PrecinctList.Clear();
                    App.VotersViewModel.NewStreetList.Clear();
                    App.VotersViewModel = null;
                }
                if (_dbLoadThread == null)
                    _dbLoadThread = new Thread(LoadDatabase);

                _dbLoadThread.Start(fileName);
            }
        }

        public IEnumerable<string> SplitCSV(string input)
        {
            System.Text.RegularExpressions.Regex csvSplit = new System.Text.RegularExpressions.Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", System.Text.RegularExpressions.RegexOptions.Compiled);

            foreach (System.Text.RegularExpressions.Match match in csvSplit.Matches(input))
            {
                yield return match.Value.TrimStart(new[] { ',', '\"', '\\' }).TrimEnd(new[] { ',', '\"', '\\' });
            }
        }

        public XElement GetVoterXmlFromCsv(string headers, string voterCsv)
        {
            string[] fieldNames = headers.Split(new[] { ',' });
            IEnumerable<string> fields = SplitCSV(voterCsv);
            System.Diagnostics.Debug.Assert(fieldNames.Count() == fields.Count<string>());

            XElement voterXml = new XElement("voter");
            int i = 0;
            foreach (string fieldData in fields)
            {
                var xField = new XElement(fieldNames[i], fieldData);
                voterXml.Add(xField);
                i++;
            }

            return voterXml;
        }

        public void LoadDatabase(Object voterFileName)
        {
            /*
            [XML representation]
            <voters>
              <voter>
                <address>1234 Anystreet</address>
                <address2></address2>
                <city>Sammamish</city>
                <state>WA</state>
                <zip>98074</zip>
                <phone></phone>
                <lastname>BOND</lastname>
                <firstname>JAMES</firstname>
                <email></email>
                <precinct>SAM 45-1234</precinct>
                <party>1</party>
                <pvscore><pri>2</pri><gen>4</gen></pvscore>
                <recid>WA01002212345</recid>
                <location>47.6161616,-122.0551111</location>
              </voter>
            </voters>
             * 
            or [csv representation]
            recid,address,address2,city,state,zip,phone,lastname,firstname,HouseholdVoters,email,precinct,party,pri,gen,location,resultofcontact,cellphone,issupporter,comments,modifiedtime
                    1234 Anystreet,,Sammamish,WA,98074,,BOND,"JAMES, GALORE",JAMES and GALORE BOND,,SAM 45-1234,1,2,4,WA01002212345,"47.6161616,-122.0551111",,,,,
            */
            string fileName = "";

            if (voterFileName is string)
            {
                fileName = voterFileName as string;
            }
            else
            {
                // MessageBox("Unable to load voter data file.", "Oops!", MessageBoxButton.OK);
                return;
            }

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream voterFile = isf.OpenFile(fileName, FileMode.Open);
            Stream voterDataStream = voterFile;

            if (fileName.EndsWith(".xml"))
            {
                App.thisApp._settings.UpdateSetting("dataformat", "xml");
            }
            else if (fileName.EndsWith(".csv"))
            {
                App.thisApp._settings.UpdateSetting("dataformat", "csv");
            }
            else if (fileName.EndsWith(".zip"))
            {
                string _voterFileName = "voters.csv";
                if (voterFile.CanSeek)
                {
                    byte[] fileFront = new byte[64];
                    int bytesRead = voterFile.Read(fileFront, 0, 64);
                    voterFile.Seek(0, SeekOrigin.Begin);
                    byte[] fileNameBytes = new byte[20];
                    int i = 0;
                    for (i = 0; i < 16; i++)
                    {
                        fileNameBytes[i] = fileFront[30 + i];
                        if (fileNameBytes[i] > 128)
                        {
                            fileNameBytes[i] = 0;
                            break;
                        }
                    }
                    _voterFileName = Encoding.UTF8.GetString(fileNameBytes, 0, i);
                }
                if (_voterFileName.EndsWith(".xml"))
                {
                    App.thisApp._settings.UpdateSetting("dataformat", "xml");
                }
                StreamResourceInfo zipInfo = new StreamResourceInfo(voterFile, "application/zip");
                StreamResourceInfo streamInfo = Application.GetResourceStream(zipInfo, new Uri(_voterFileName, UriKind.Relative));
                if (streamInfo != null)
                    voterDataStream = streamInfo.Stream;
            }
            
            int nVoters = 0;

            VoterFileDataContext _voterDB = new VoterFileDataContext(string.Format(VoterFileDataContext.DBConnectionString, _settings.DbFileName));
            // if (_settings.DbStatus == DbState.Loaded) NOTE: Forcing to reload during testing
            if (false)
            {
                // TODO: prompt user to reload/overwrite database
                // TODO: Read database status from app settings
                // IsDbLoaded = true;               
                return;
            }
            else
            {
                try
                {
                    if (!_voterDB.DatabaseExists())
                    {
                        App.Log("Creating new database...");
                        _voterDB.CreateDatabase();
                        _settings.DbStatus = DbState.Empty;
                        App.Log("  Database created");
                    }
                    else
                    {
                        // TODO: prompt user, then Dump existing database and create a new one
                        App.Log("WARNING: Deleting existing database...");
                        _voterDB.DeleteDatabase();
                        App.Log("Creating new database...");
                        _voterDB.CreateDatabase();
                        _settings.DbStatus = DbState.Empty;
                        App.Log("  Database created");
                    }

                    if (App.thisApp._settings.GetSetting<string>("dataformat") == "xml")
                    {

                        XDocument loadedData = null;

                        loadedData = XDocument.Load(voterDataStream);

                        IEnumerable<XElement> dbDate = loadedData.Descendants("timestamp");
                        if (dbDate != null)
                        {
                            string dbDateString = dbDate.First<XElement>().Value;
                            DateTime dbTimeStamp;
                            if (DateTime.TryParse(dbDateString, out dbTimeStamp))
                                _settings.DbDate = dbTimeStamp;
                        }
                        IEnumerable<XElement> votersNode = loadedData.Descendants("voters");
                        if (votersNode != null)
                        {

                            App.Log("  XML file loaded");
                            IEnumerable<VoterFileEntry> voters = from query in votersNode.Descendants("voter")
                                                                 select new VoterFileEntry
                                                                 {
                                                                     FirstName = (string)query.Element("firstname"),
                                                                     LastName = (string)query.Element("lastname"),
                                                                     Address = (string)query.Element("address"),
                                                                     Address2 = (string)query.Element("address2"),
                                                                     City = (string)query.Element("city"),
                                                                     State = (string)query.Element("state"),
                                                                     Zip = (string)query.Element("zip"),
                                                                     VoterID = (string)query.Element("recid"),
                                                                     PartyString = (string)query.Element("party"),
                                                                     Precinct = (string)query.Element("precinct"),
                                                                     PrimaryVoteHistoryString = (string)query.Element("pvscore").Element("pri"),
                                                                     GeneralVoteHistoryString = (string)query.Element("pvscore").Element("gen"),
                                                                     Email = (string)query.Element("email"),
                                                                     Phone = (string)query.Element("phone"),
                                                                     Coordinates = (string)query.Element("location"),
                                                                     ResultOfContactString = (string)query.Element("resultofcontact"),
                                                                     CellPhone = (string)query.Element("cellphone"),
                                                                     IsSupporterString = (string)query.Element("issupporter"),
                                                                     IsVolunteerString = (string)query.Element("isvolunteer"),
                                                                     Comments = (string)query.Element("comments"),
                                                                     ModifiedTime = DateTime.Now
                                                                 };
                            App.Log("  Query completed");
                            _settings.DbStatus = DbState.Loading;
                            foreach (VoterFileEntry voter in voters)
                            {
                                if (0.0 == voter.Latitude || 0.0 == voter.Longitude)
                                {
                                    continue;
                                }
                                nVoters++;
                                _voterDB.AllVoters.InsertOnSubmit(voter);
                            }
                        }
                    }
                    else if (App.thisApp._settings.GetSetting<string>("dataformat") == "csv")
                    {
                        // Load voters from rows in csv text
                        StreamReader csvReader = new StreamReader(voterDataStream);

                        String headerLine = csvReader.ReadLine().ToLower();
                        String voterLine = csvReader.ReadLine();
                        _settings.DbStatus = DbState.Loading;
                        App.Log("  Parsing voters from CSV");

                        while (voterLine != null)
                        {
                            // recid,address,address2,city,state,zip,phone,lastname,firstname,HouseholdVoters,email,precinct,party,pri,gen,location
                            XElement voterElement = GetVoterXmlFromCsv(headerLine, voterLine);
                            // We need at least the basics of address, city, and voterid to do anything useful with this record
                            try
                            {
                                VoterFileEntry voter = new VoterFileEntry
                                                        {
                                                            Address = (string)voterElement.Element("address"),
                                                            City = (string)voterElement.Element("city"),
                                                            // VoterID = (string)voterElement.Element("recid"),
                                                            ModifiedTime = DateTime.Now
                                                        };
                                if (voterElement.Elements("recid").Count() > 0)
                                {
                                    voter.VoterID = (string)voterElement.Element("recid");
                                    App.thisApp._settings.UpdateSetting("_trhack", false);
                                }
                                // NOTE: This is a hack to fix my data error just before TechRoanoke conference
                                else if (voterElement.Elements("redid").Count() > 0)
                                {
                                    voter.VoterID = (string)voterElement.Element("redid");
                                    App.thisApp._settings.UpdateSetting("_trhack", true);
                                }
                                if (voterElement.Elements("firstname").Count() > 0)
                                    voter.FirstName = (string)voterElement.Element("firstname");
                                if (voterElement.Elements("lastname").Count() > 0)
                                    voter.LastName = (string)voterElement.Element("lastname");
                                if (voterElement.Elements("address2").Count() > 0)
                                    voter.Address2 = (string)voterElement.Element("address2");
                                if (voterElement.Elements("state").Count() > 0)
                                    voter.State = (string)voterElement.Element("state");
                                else
                                    voter.State = "WA";
                                if (voterElement.Elements("zip").Count() > 0)
                                    voter.Zip = (string)voterElement.Element("zip");
                                if (voterElement.Elements("party").Count() > 0)
                                    voter.PartyString = (string)voterElement.Element("party");
                                if (voterElement.Elements("precinctname").Count() > 0) // Prefer "precinctname" over "precinct"
                                    voter.Precinct = (string)voterElement.Element("precinctname");
                                else if (voterElement.Elements("precinct").Count() > 0)
                                    voter.Precinct = (string)voterElement.Element("precinct");
                                if (voterElement.Elements("pri").Count() > 0)
                                    voter.PrimaryVoteHistoryString = (string)voterElement.Element("pri");
                                if (voterElement.Elements("gen").Count() > 0)
                                    voter.GeneralVoteHistoryString = (string)voterElement.Element("gen");
                                if (voterElement.Elements("email").Count() > 0)
                                    voter.Email = (string)voterElement.Element("email");
                                if (voterElement.Elements("phone").Count() > 0)
                                    voter.Phone = (string)voterElement.Element("phone");
                                if (voterElement.Elements("location").Count() > 0)
                                    voter.Coordinates = (string)voterElement.Element("location");
                                else
                                {
                                    if (voterElement.Elements("lat").Count() > 0)
                                        voter.LatitudeString = (string)voterElement.Element("lat");
                                    if (voterElement.Elements("long").Count() > 0)
                                        voter.LongitudeString = (string)voterElement.Element("long");
                                }
                                if (voterElement.Elements("resultofcontact").Count() > 0)
                                    voter.ResultOfContactString = (string)voterElement.Element("resultofcontact");
                                if (voterElement.Elements("cellphone").Count() > 0)
                                    voter.CellPhone = (string)voterElement.Element("cellphone");
                                if (voterElement.Elements("issupporter").Count() > 0)
                                    voter.IsSupporterString = (string)voterElement.Element("issupporter");
                                if (voterElement.Elements("isvolunteer").Count() > 0)
                                    voter.IsVolunteerString = (string)voterElement.Element("isvolunteer");
                                if (voterElement.Elements("comments").Count() > 0)
                                    voter.Comments = (string)voterElement.Element("comments");
                                if (voterElement.Elements("address2").Count() > 0)
                                    voter.Address2 = (string)voterElement.Element("address2");

                                if (voterElement.Elements("gender").Count() > 0)
                                {
                                    voter.Gender = (string)voterElement.Element("gender");
                                    voter.FirstName = voter.FirstName + " " + voter.Gender;
                                }
                                if (voterElement.Elements("birthdate").Count() > 0)
                                {
                                    voter.Birthdate = (string)voterElement.Element("birthdate");
                                    if (voter.Age > 0)
                                        voter.FirstName = voter.FirstName + " " + voter.Age.ToString();
                                }
                                // We don't have lat/long for this voter, but we can still show them in precinct and street lists
                                if (0.0 == voter.Latitude || 0.0 == voter.Longitude)
                                {
                                    // continue; 
                                }
                                nVoters++;
                                _voterDB.AllVoters.InsertOnSubmit(voter);
                            }
                            catch (Exception ex)
                            {
                                App.Log("There was a problem parsing voter record - " + voterLine + " : " + ex.ToString());
                            }

                            voterLine = csvReader.ReadLine();
                        }
                        App.Log("  Parsing voters completed");
                    }
                    else
                    {
                        App.Log("Unrecognized file type - " + fileName);
                    }
                    voterDataStream.Close();
                    voterDataStream.Dispose();
                    voterFile.Close();
                    voterFile.Dispose();
                    App.Log("  Voters submitted to database: " + nVoters.ToString());
                    _voterDB.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                    App.Log(" Loading Precincts table");
                    IEnumerable<string> precincts = (from v in _voterDB.AllVoters select v.Precinct).Distinct();
                    foreach (string precinct in precincts)
                    {
                        PrecinctTableEntry thisPrecinct = new PrecinctTableEntry();
                        thisPrecinct.Name = precinct;
                        List<VoterFileEntry> pList = (from VoterFileEntry vPrecinct in _voterDB.AllVoters where vPrecinct.Precinct == precinct && vPrecinct.Latitude > 47.0 && vPrecinct.Longitude < 122.0 select vPrecinct).ToList();
                        App.Log(String.Format("   There are {0} voters in {1} precinct.", pList.Count, precinct));
                        if (pList.Count > 0)
                        {
                            thisPrecinct.West = pList.Min<VoterFileEntry, double>(vMin => vMin.Longitude); // smallest longitude found in list (or largest absolute value)
                            thisPrecinct.East = pList.Max<VoterFileEntry, double>(vMax => vMax.Longitude); // largest longitude found in list (or smallest absolute value)
                            thisPrecinct.North = pList.Max<VoterFileEntry, double>(vMax => vMax.Latitude); // largest latitude found in list
                            thisPrecinct.South = pList.Min<VoterFileEntry, double>(vMin => vMin.Latitude); // smallest latitude found in list
                            App.Log(String.Format("Precinct {4} - North={0}, South={1}, East={2}, West={3}.", thisPrecinct.North, thisPrecinct.South, thisPrecinct.East, thisPrecinct.West, thisPrecinct.Name));
                            double diffLat = thisPrecinct.North - thisPrecinct.South;
                            double diffLong = thisPrecinct.East - thisPrecinct.West;
                            thisPrecinct.CenterLatitude = thisPrecinct.South + (diffLat / 2);
                            thisPrecinct.CenterLongitude = thisPrecinct.West + (diffLong / 2);
                            _voterDB.Precincts.InsertOnSubmit(thisPrecinct);
                        }
                    }
                    App.Log("  Submitting precincts to database: ");
                    _voterDB.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);

                    App.Log(" Loading Streets table");
                    IEnumerable<string> _streets = (from v in _voterDB.AllVoters select v.Street).Distinct();
                    foreach (string street in _streets)
                    {
                        StreetTableEntry thisStreet = new StreetTableEntry();
                        thisStreet.Name = street;
                        List<VoterFileEntry> pList = (from VoterFileEntry vStreet in _voterDB.AllVoters where vStreet.Street == street && vStreet.Latitude > 47.0 && vStreet.Longitude < 122.0 select vStreet).ToList();
                        App.Log(String.Format("   There are {0} voters on {1} street.", pList.Count, street));
                        if (pList.Count > 0)
                        {
                            thisStreet.West = pList.Min<VoterFileEntry, double>(vMin => vMin.Longitude); // smallest longitude found in list (or largest absolute value)
                            thisStreet.East = pList.Max<VoterFileEntry, double>(vMax => vMax.Longitude); // largest longitude found in list (or smallest absolute value)
                            thisStreet.North = pList.Max<VoterFileEntry, double>(vMax => vMax.Latitude); // largest latitude found in list
                            thisStreet.South = pList.Min<VoterFileEntry, double>(vMin => vMin.Latitude); // smallest latitude found in list
                            App.Log(String.Format("Street {4} - North={0}, South={1}, East={2}, West={3}.", thisStreet.North, thisStreet.South, thisStreet.East, thisStreet.West, thisStreet.Name));
                            double diffLat = thisStreet.North - thisStreet.South;
                            double diffLong = thisStreet.East - thisStreet.West;
                            thisStreet.CenterLatitude = thisStreet.South + (diffLat / 2);
                            thisStreet.CenterLongitude = thisStreet.West + (diffLong / 2);
                            _voterDB.Streets.InsertOnSubmit(thisStreet);
                        }
                    }
                    App.Log("  Submitting streets to database: ");
                    _voterDB.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);

                    App.Log("  Changes committed to database");
                    _settings.DbStatus = DbState.Loaded;
                    _settings.UpdateSetting("lastsync", DateTime.Now);
                }
                catch (Exception ex)
                {
                    App.Log("Exception loading voters to database: " + ex.ToString());
                }
            }
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            // TODO: reload location of Car from isostore
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // TODO: reload location of Car from isostore
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // TODO: write location of Car to isostore
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // TODO: write location of Car to isostore
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void Log(string messageString)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("({2:X}) {0:H:mm:ss.fff}: {1}", DateTime.Now, messageString, Thread.CurrentThread.ManagedThreadId));
        }
    }



    public class VoterViewModel : INotifyPropertyChanged
    {
        public VoterViewModel()
        {
            _inViewVoters = new ObservableCollection<PushpinModel>();
        }

        private ObservableCollection<PushpinModel> _inViewVoters = new ObservableCollection<PushpinModel>();

        public ObservableCollection<PushpinModel> VoterList
        {
            get
            {
                return _inViewVoters;
            }
            set
            {
                _inViewVoters = value;
                NotifyPropertyChanged("VoterList");
            }
        }

        private ObservableCollection<PrecinctPinModel> _precinctList = new ObservableCollection<PrecinctPinModel>();

        public ObservableCollection<PrecinctPinModel> PrecinctList
        {
            get { return _precinctList; }
            set
            {
                _precinctList = value;
                NotifyPropertyChanged("PrecinctList");
            }
        }

        private ObservableCollection<StreetPinModel> _streetList = new ObservableCollection<StreetPinModel>();

        public ObservableCollection<StreetPinModel> NewStreetList
        {
            get { return _streetList; }
            set
            {
                _streetList = value;
                NotifyPropertyChanged("NewStreetList");
            }
        }

        private List<string> _streets = new List<string>();

        public List<string> StreetList
        {
            get
            {
                return _streets;
            }
            set
            {
                if (_streets != value)
                {
                    _streets = value;
                    NotifyPropertyChanged("StreetList");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}