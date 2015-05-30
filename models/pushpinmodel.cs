using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;
// using Microsoft.Phone.Controls.Maps;
using System.Collections.ObjectModel;

namespace mapapp
{
    public class PushpinModel
    {
        public PushpinModel(mapapp.data.VoterFileEntry _voter)
        {
            VoterFile = _voter;
        }

        public PushpinModel()
        {
        }

        private GeoCoordinate _location;
        public GeoCoordinate Location
        {
            get
            {
                if (_location != null)
                    return _location;
                else
                {
                    _location = new GeoCoordinate();
                    if (VoterFile != null && VoterFile.Latitude != 0.0)
                    {
                        _location.Latitude = VoterFile.Latitude;
                        _location.Longitude = VoterFile.Longitude;

                    }
                    return _location;
                }
            }
            set { _location = value; }
        }

        public bool Valid { get; set; }

        public Visibility Visibility { get; set; }
        public string Content { get; set; }
        // public string FullContent { get; set; }
        public mapapp.data.VoterFileEntry VoterFile { get; set; }

        public string VoterID
        {
            get { return VoterFile.VoterID; }
        }
        public string FirstName { get { return VoterFile.FirstName; } }
        public string LastName { get { return VoterFile.LastName; } }
        public string Address { get { return VoterFile.Address; } }
        public string Address2 { get { return VoterFile.Address2; } }
        public string City { get { return VoterFile.City; } }
        public string State { get { return VoterFile.State; } }
        public string Zip { get { return VoterFile.Zip; } }
        public string Phone 
        { 
            get { return VoterFile.Phone; }
            set
            {
                VoterFile.Phone = value;
            }
        }
        // public string RecordID { get; set; }
        public int party { get { return VoterFile.Party; } set { VoterFile.Party = value; } }
        public string precinct { get { return VoterFile.Precinct; } }
        public int PrimaryVoteHistory { get { return VoterFile.PrimaryVoteHistory; } }
        public int GeneralVoteHistory { get { return VoterFile.GeneralVoteHistory; } }
        public bool IsSupporter { get { return VoterFile.IsSupporter; } set { VoterFile.IsSupporter = value; } }
        public bool IsVolunteer { get { return VoterFile.IsVolunteer; } set { VoterFile.IsVolunteer = value; } }
        public string Email { get { return VoterFile.Email; } set { VoterFile.Email = value; } }
        public string CellPhone { get { return VoterFile.CellPhone; } set { VoterFile.CellPhone = value; } }
        public string Comments { get { return VoterFile.Comments; } set { VoterFile.Comments = value; } }
        public int ResultOfContact { get { return VoterFile.ResultOfContact; } set { VoterFile.ResultOfContact = value; } }

        public string Street { get { return VoterFile.Street; } }

        private int _housenum = 0;
        public int HouseNum 
        {
            get
            {
                if ((this.VoterFile != null) && (this.VoterFile.Address != null) && (_housenum == 0))
                {
                    int nHouseNum = 0;
                    string strHouseNum = this.VoterFile.Address.Substring(0, this.VoterFile.Address.IndexOf(' '));
                    if (Int32.TryParse(strHouseNum, out nHouseNum))
                        _housenum = nHouseNum;
                }
                return _housenum;
            }
        }

        public bool IsEven 
        {
            get
            {
                bool bEven = false;
                if (this.HouseNum != 0)
                    bEven = (_housenum % 2) == 0;
                return bEven;
            }
        }

        public string FullName 
        {
            get
            {
                return (VoterFile != null) ? VoterFile.LastName + ", " + VoterFile.FirstName : Content;
            }
        }

        public Brush Background 
        {
            get
            {
                Brush _bg = new SolidColorBrush(Colors.White);
                if (VoterFile != null)
                {
                    switch (VoterFile.Party)
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
        }

        public Brush Foreground
        {
            get
            {
                Brush _fg = new SolidColorBrush(Colors.Black);
                if (VoterFile != null)
                {
                    switch (VoterFile.Party)
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
        }

        public double dist = 0.0;

        public string LatLong 
        { 
            set
            {
                string [] latlong = value.Split(',');
                double lat = 0.0;
                double lon = 0.0;

                if (latlong.Length != 2)
                {
                    Valid = false;
                    return;
                }

                double.TryParse(latlong[0], out lat);
                double.TryParse(latlong[1], out lon);
                if (lat != 0.0 && this.VoterFile != null)
                {
                    float _lat = (float)lat;
                    this.VoterFile.Latitude = _lat;
                }
                if (lon != 0.0 && this.VoterFile != null)
                {
                    float _lon = (float)lon;
                    this.VoterFile.Longitude = _lon;
                }

                Location = new GeoCoordinate(lat, lon);
                Valid = true;
            }
        }

        public PushpinModel Clone(GeoCoordinate location)
        {
            return new PushpinModel(VoterFile)
            {
                Location = location,
                //TypeName = TypeName,
                //Icon = Icon
            };
        }
    }

    public class PrecinctPinModel
    {
        public PrecinctPinModel(mapapp.data.PrecinctTableEntry _precinct)
        {
            PrecinctEntry = _precinct;
        }

        public PrecinctPinModel()
        {
        }

        private double _north;
        public double North
        {
            get
            {
                if (_north != null)
                    return _north;
                else
                {
                    if (PrecinctEntry != null && PrecinctEntry.North != 0.0)
                    {
                        _north = PrecinctEntry.North;
                    }
                    else
                        _north = 0.0;
                    return _north;
                }
            }
            set { _north = value; }
        }

        private double _south;
        public double South
        {
            get
            {
                if (_south != null)
                    return _south;
                else
                {
                    if (PrecinctEntry != null && PrecinctEntry.South != 0.0)
                    {
                        _south = PrecinctEntry.South;
                    }
                    else
                        _south = 0.0;
                    return _south;
                }
            }
            set { _south = value; }
        }

        private double _east;
        public double East
        {
            get
            {
                if (_east != null)
                    return _east;
                else
                {
                    if (PrecinctEntry != null && PrecinctEntry.East != 0.0)
                    {
                        _east = PrecinctEntry.East;
                    }
                    else
                        _east = 0.0;
                    return _east;
                }
            }
            set { _east = value; }
        }

        private double _west;
        public double West
        {
            get
            {
                if (_west != null)
                    return _west;
                else
                {
                    if (PrecinctEntry != null && PrecinctEntry.West != 0.0)
                    {
                        _west = PrecinctEntry.West;
                    }
                    else
                        _west = 0.0;
                    return _west;
                }
            }
            set { _west = value; }
        }

        private GeoCoordinate _center;
        public GeoCoordinate Center
        {
            get
            {
                if (_center != null)
                    return _center;
                else
                {
                    _center = new GeoCoordinate();
                    if (PrecinctEntry != null && PrecinctEntry.CenterLatitude != 0.0)
                    {
                        _center.Latitude = PrecinctEntry.CenterLatitude;
                        _center.Longitude = PrecinctEntry.CenterLongitude;

                    }
                    return _center;
                }
            }
            set { _center = value; }
        }

        public bool Valid { get; set; }

        public Visibility Visibility { get; set; }
        public string Content
        {
            get
            {
                if (PrecinctEntry != null && PrecinctEntry.Name != null)
                {
                    return PrecinctEntry.Name;
                }
                else
                    return "";
            }
        }
        public mapapp.data.PrecinctTableEntry PrecinctEntry { get; set; }
    }
    public class StreetPinModel
    {
        public StreetPinModel(mapapp.data.StreetTableEntry _street)
        {
            StreetEntry = _street;
        }

        public StreetPinModel()
        {
        }

        private double _north;
        public double North
        {
            get
            {
                if (_north != null)
                    return _north;
                else
                {
                    if (StreetEntry != null && StreetEntry.North != 0.0)
                    {
                        _north = StreetEntry.North;
                    }
                    else
                        _north = 0.0;
                    return _north;
                }
            }
            set { _north = value; }
        }

        private double _south;
        public double South
        {
            get
            {
                if (_south != null)
                    return _south;
                else
                {
                    if (StreetEntry != null && StreetEntry.South != 0.0)
                    {
                        _south = StreetEntry.South;
                    }
                    else
                        _south = 0.0;
                    return _south;
                }
            }
            set { _south = value; }
        }

        private double _east;
        public double East
        {
            get
            {
                if (_east != null)
                    return _east;
                else
                {
                    if (StreetEntry != null && StreetEntry.East != 0.0)
                    {
                        _east = StreetEntry.East;
                    }
                    else
                        _east = 0.0;
                    return _east;
                }
            }
            set { _east = value; }
        }

        private double _west;
        public double West
        {
            get
            {
                if (_west != null)
                    return _west;
                else
                {
                    if (StreetEntry != null && StreetEntry.West != 0.0)
                    {
                        _west = StreetEntry.West;
                    }
                    else
                        _west = 0.0;
                    return _west;
                }
            }
            set { _west = value; }
        }

        private GeoCoordinate _center;
        public GeoCoordinate Center
        {
            get
            {
                if (_center != null)
                    return _center;
                else
                {
                    _center = new GeoCoordinate();
                    if (StreetEntry != null && StreetEntry.CenterLatitude != 0.0)
                    {
                        _center.Latitude = StreetEntry.CenterLatitude;
                        _center.Longitude = StreetEntry.CenterLongitude;

                    }
                    return _center;
                }
            }
            set { _center = value; }
        }

        public bool Valid { get; set; }

        public Visibility Visibility { get; set; }
        public string Content
        {
            get
            {
                if (StreetEntry != null && StreetEntry.Name != null)
                {
                    return StreetEntry.Name;
                }
                else
                    return "";
            }
        }
        public mapapp.data.StreetTableEntry StreetEntry { get; set; }
    }
}
