using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows.Resources;

//Edited for Windows Phone compatibility 
//by REN Chang
//July, 2014
//Wuhan, China
//renc.whu@gmail.com
//New Features:
//-Support several new attributes in schema 1.1
//-Add write file functionality

//GPX Class C#.NET

//Written by Kevin D. MacDonald
//December, 2011
//Vancouver, Canada
//Kevin@geekfrog.ca

//Version 1.0

//Open and read the GPX or XML file into a string, and instantiate the class by passing in the string.
//Incorporates the latest Geocaching.com(GroundSpeak) and Opencaching.com modifications
//Please email me GPX files if you find bugs or things I have missed.

//Thank you!

//http://www.topografix.com/gpx.asp

public class GPXClass
{
    private XDocument _GPX = new XDocument();

    private string _Name = "";

    public string Name
    {
        get { return _Name; }
        set { _Name = value; }
    }

    public string Description = "";
    public string Author = "";
    public string EMail = "";
    public string Time = "";
    public string KeyWords = "";
    public string URL = "";
    public string URLName = "";
    public GPSBoundary Bounds = new GPSBoundary();
    public List<wpt> WayPoints = new List<wpt>();
    public List<rte> Routes = new List<rte>();
    public List<trk> Tracks = new List<trk>();

    public GPXClass(StreamResourceInfo XML)
    {
        if (XML != null)
        {
            _GPX = XDocument.Load(XML.Stream);
            System.Diagnostics.Debug.WriteLine(_GPX.Root.Name.LocalName.ToString() + _GPX.Root.Name.LocalName.ToString().Equals("gpx"));
            if (_GPX.Root.Name.LocalName.ToString().Equals("gpx"))
            {
                System.Diagnostics.Debug.WriteLine("OK");
                IEnumerable<XElement> GPXNodes = _GPX.Root.Elements();
                System.Diagnostics.Debug.WriteLine(GPXNodes);
                foreach (XElement Node in GPXNodes)
                {
                    System.Diagnostics.Debug.WriteLine(Node.Name);
                    switch (Node.Name.LocalName.ToString())
                    {
                        case "name":
                            Name = Node.Value;
                            break;
                        case "desc":
                            Description = Node.Value;
                            break;
                        case "author":
                            Author = Node.Value;
                            break;
                        case "email":
                            EMail = Node.Value;
                            break;
                        case "time":
                            Time = Node.Value;
                            break;
                        case "keywords":
                            KeyWords = Node.Value;
                            break;
                        case "bounds":
                            Bounds = new GPSBoundary();
                            foreach (XAttribute Att in Node.Attributes())
                            {
                                switch (Att.Name.LocalName.ToString())
                                {
                                    case "minlat":
                                        Bounds.Min.lat = Att.Value;
                                        break;
                                    case "minlon":
                                        Bounds.Min.lon = Att.Value;
                                        break;
                                    case "maxlat":
                                        Bounds.Max.lat = Att.Value;
                                        break;
                                    case "maxlon":
                                        Bounds.Max.lon = Att.Value;
                                        break;
                                }
                            }
                            break;
                        case "wpt":
                            wpt NewWayPoint = new wpt(Node);
                            WayPoints.Add(NewWayPoint);
                            break;
                        case "rte":
                            rte NewRoute = new rte(Node);
                            Routes.Add(NewRoute);
                            break;
                        case "trk":
                            trk Track = new trk(Node);
                            Tracks.Add(Track);
                            break;
                        case "url":
                            URL = Node.Value;
                            break;
                        case "urlname":
                            URLName = Node.Value;
                            break;
                        case "topografix:active_point":
                        case "topografix:map":
                            break;
                        default:
                            throw new Exception("Unhandled for Child Object: " + Node.Name.LocalName.ToString());
                    }
                }
            }
        }
    }

    public class travelbug
    {
        public string ID = "";
        public string Reference = "";
        public string Groundspeak_Name = "";

        public travelbug(XElement TravelBugNode)
        {
            ID = TravelBugNode.Attribute("id").Value.ToString();
            Reference = TravelBugNode.Attribute("ref").Value.ToString();
            foreach (XElement TBChildNode in TravelBugNode.Elements())
            {
                switch (TBChildNode.Name.LocalName.ToString())
                {
                    case "groundspeak:name":
                        Groundspeak_Name = TBChildNode.Value;
                        break;
                    default:
                        throw new Exception("Unhandled Child Node: " + TBChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    public class cachelog
    {
        public string ID = "";
        public string Groundspeak_Date = "";
        public string Groundspeak_Type = "";
        public string Groundspeak_Finder = "";
        public string Groundspeak_FinderID = "";
        public string Groundspeak_Text = "";
        public string Groundspeak_TextEncoded = "";
        public GPSCoordinates Groundspeak_LogWayPoint = new GPSCoordinates();

        public cachelog(XElement ChildNode)
        {
            ID = ChildNode.Attribute("id").Value.ToString();
            foreach (XElement Node in ChildNode.Elements())
            {
                switch (Node.Name.LocalName.ToString())
                {
                    case "groundspeak:date":
                        Groundspeak_Date = Node.Value;
                        break;
                    case "groundspeak:type":
                        Groundspeak_Type = Node.Value;
                        break;
                    case "groundspeak:finder":
                        Groundspeak_Finder = Node.Value;
                        Groundspeak_FinderID = Node.Attribute("id").Value.ToString();
                        break;
                    case "groundspeak:text":
                        Groundspeak_Text = Node.Value;
                        Groundspeak_TextEncoded = Node.Attribute("encoded").Value.ToString();
                        break;
                    case "groundspeak:log_wpt":
                        Groundspeak_LogWayPoint.lat = Node.Attribute("lat").Value.ToString();
                        Groundspeak_LogWayPoint.lon = Node.Attribute("lon").Value.ToString();
                        break;
                    default:
                        throw new Exception("Unhandled Child Node: " + Node.Name.LocalName.ToString());
                }
            }
        }
    }

    public class cache
    {
        public string ID = "";
        public string Available = "";
        public string Archived = "";
        public string Xmlns = "";

        public string Groundspeak_Name = "";
        public string Groundspeak_PlacedBy = "";
        public string Groundspeak_Owner = "";
        public string Groundspeak_OwnerID = "";
        public string Groundspeak_Type = "";
        public string Groundspeak_Container = "";
        public string Groundspeak_Difficulty = "";
        public string Groundspeak_Terrain = "";
        public string Groundspeak_Country = "";
        public string Groundspeak_State = "";
        public string Groundspeak_ShortDescription = "";
        public bool Groundspeak_ShortDescriptionIsHTML = false;
        public string Groundspeak_LongDescription = "";
        public bool Groundspeak_LongDescriptionIsHTML = false;
        public string Groundspeak_EncodedHint = "";

        public List<cachelog> Groundspeak_Logs = new List<cachelog>();
        public List<travelbug> Groundspeak_Travelbugs = new List<travelbug>();
        public List<Attribute> Groundspeak_Attributes = new List<Attribute>();

        public cache(XElement Node)
        {
            #region Attributes

            foreach (XAttribute Attribute in Node.Attributes())
            {
                switch (Attribute.Name.LocalName.ToString())
                {
                    case "id":
                        ID = Attribute.Value;
                        break;
                    case "available":
                        Available = Attribute.Value;
                        break;
                    case "archived":
                        Archived = Attribute.Value;
                        break;
                    case "xmlns:groundspeak":
                        Xmlns = Attribute.Value;
                        break;
                    default:
                        throw new Exception("Unhandled Attribute: " + Attribute.Name.LocalName.ToString());
                }
            }
            #endregion Attributes

            foreach (XElement ChildNode in Node.Elements())
            {
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "groundspeak:name":
                        Groundspeak_Name = ChildNode.Value;
                        break;
                    case "groundspeak:placed_by":
                        Groundspeak_PlacedBy = ChildNode.Value;
                        break;
                    case "groundspeak:owner":
                        Groundspeak_Owner = ChildNode.Value;
                        Groundspeak_OwnerID = ChildNode.Attribute("id").Value.ToString();
                        break;
                    case "groundspeak:type":
                        Groundspeak_Type = ChildNode.Value;
                        break;
                    case "groundspeak:container":
                        Groundspeak_Container = ChildNode.Value;
                        break;
                    case "groundspeak:difficulty":
                        Groundspeak_Difficulty = ChildNode.Value;
                        break;
                    case "groundspeak:terrain":
                        Groundspeak_Terrain = ChildNode.Value;
                        break;
                    case "groundspeak:country":
                        Groundspeak_Country = ChildNode.Value;
                        break;
                    case "groundspeak:state":
                        Groundspeak_State = ChildNode.Value;
                        break;
                    case "groundspeak:short_description":
                        Groundspeak_ShortDescription = ChildNode.Value;
                        if (ChildNode.Attribute("html").Value.Equals("True"))
                        {
                            Groundspeak_ShortDescriptionIsHTML = true;
                        }
                        break;
                    case "groundspeak:long_description":
                        Groundspeak_LongDescription = ChildNode.Value;
                        if (ChildNode.Attribute("html").Value.Equals("True"))
                        {
                            Groundspeak_LongDescriptionIsHTML = true;
                        }
                        break;
                    case "groundspeak:encoded_hints":
                        Groundspeak_EncodedHint = ChildNode.Value;
                        break;
                    case "groundspeak:logs":
                        foreach (XElement LogNode in ChildNode.Elements())
                        {
                            cachelog Groundspeak_LogEntry = new cachelog(LogNode);
                            Groundspeak_Logs.Add(Groundspeak_LogEntry);
                        }
                        break;
                    case "groundspeak:travelbugs":
                        foreach (XElement TravelBugNode in ChildNode.Elements())
                        {
                            travelbug Travelbug = new travelbug(TravelBugNode);
                            Groundspeak_Travelbugs.Add(Travelbug);
                        }
                        break;
                    case "groundspeak:attributes":
                        foreach (XElement AttributeNode in ChildNode.Elements())
                        {
                            Attribute CacheAttribute = new Attribute(AttributeNode);
                            Groundspeak_Attributes.Add(CacheAttribute);
                        }
                        break;
                    default:
                        throw new Exception("Unhandled Child Node: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }

        public cache()
        {
        }
    }


    //WayPoint contains Caches and other Objects

    public class wpt
    {
        public GPSCoordinates Coordinates = new GPSCoordinates();
        public string Name = "";
        public string Desc = "";
        public string Time = "";
        public string URLName = "";
        public string URL = "";
        public string Sym = "";
        public string Type = "";
        public string Ele = "";
        public string Cmt = "";
        public string Opencaching_Awesomeness = "";
        public string Opencaching_Difficulty = "";
        public string Opencaching_Terrain = "";
        public string Opencaching_Size = "";
        public string Opencaching_VerificationPhrase = "";
        public string Opencaching_VerificationNumber = "";
        public string Opencaching_VerificationQR = "";
        public string Opencaching_VerificationChirp = "";
        public string Opencaching_SeriesID = "";
        public string Opencaching_SeriesName = "";
        public List<string> Opencaching_Tags = new List<string>();

        public cache Groundspeak_Cache = new cache();

        public wpt(XElement Node)
        {
            Coordinates.lat = Node.Attribute("lat").Value.ToString();
            Coordinates.lon = Node.Attribute("lon").Value.ToString();
            foreach (XElement ChildNode in Node.Elements())
            {
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "time":
                        Time = ChildNode.Value;
                        break;
                    case "name":
                        Name = ChildNode.Value;
                        break;
                    case "desc":
                        Desc = ChildNode.Value;
                        break;
                    case "url":
                        URL = ChildNode.Value;
                        break;
                    case "urlname":
                        URLName = ChildNode.Value;
                        break;
                    case "sym":
                        Sym = ChildNode.Value;
                        break;
                    case "type":
                        Type = ChildNode.Value;
                        break;
                    case "ele":
                        Ele = ChildNode.Value;
                        break;
                    case "cmt":
                        Cmt = ChildNode.Value;
                        break;
                    case "groundspeak:cache":
                        Groundspeak_Cache = new cache(ChildNode);
                        break;
                    case "ox:opencaching":
                        foreach (XElement OpenCachingChildNode in ChildNode.Elements())
                        {
                            switch (OpenCachingChildNode.Name.LocalName.ToString())
                            {
                                case "ox:ratings":
                                    foreach (XElement OpenCachingRatingsChildNode in OpenCachingChildNode.Elements())
                                    {
                                        switch (OpenCachingRatingsChildNode.Name.LocalName.ToString())
                                        {
                                            case "ox:awesomeness":
                                                Opencaching_Awesomeness = (OpenCachingRatingsChildNode).Value;
                                                break;
                                            case "ox:difficulty":
                                                Opencaching_Difficulty = (OpenCachingRatingsChildNode).Value;
                                                break;
                                            case "ox:terrain":
                                                Opencaching_Terrain = (OpenCachingRatingsChildNode).Value;
                                                break;
                                            case "ox:size":
                                                Opencaching_Size = (OpenCachingRatingsChildNode).Value;
                                                break;
                                            default:
                                                throw new Exception("Unhandled for Child Object: " + OpenCachingRatingsChildNode.Name.LocalName.ToString());
                                        }
                                    }
                                    break;
                                case "ox:tags":
                                    foreach (XElement OpenCachingTagNode in OpenCachingChildNode.Elements())
                                    {
                                        switch (OpenCachingTagNode.Name.LocalName.ToString())
                                        {
                                            case "ox:tag":
                                                //Opencaching_Tags.Add((OpenCachingTagNode).InnerXml);
                                                break;
                                            default:
                                                throw new Exception("Unhandled for Child Object: " + OpenCachingTagNode.Name.LocalName.ToString());
                                        }
                                    }

                                    break;
                                case "ox:verification":
                                    foreach (XElement OpenCachingVerificationNode in OpenCachingChildNode.Elements())
                                    {
                                        switch (OpenCachingVerificationNode.Name.LocalName.ToString())
                                        {
                                            case "ox:phrase":
                                                Opencaching_VerificationPhrase = OpenCachingChildNode.Value;
                                                break;
                                            case "ox:number":
                                                Opencaching_VerificationNumber = OpenCachingChildNode.Value;
                                                break;
                                            case "ox:QR":
                                                Opencaching_VerificationQR = OpenCachingChildNode.Value;
                                                break;
                                            case "ox:chirp":
                                                Opencaching_VerificationChirp = OpenCachingChildNode.Value;
                                                break;
                                            default:
                                                throw new Exception("Unhandled for Child Object: " + OpenCachingVerificationNode.Name.LocalName.ToString());
                                        }
                                    }
                                    break;
                                case "ox:series":
                                    Opencaching_SeriesName = OpenCachingChildNode.Value;
                                    Opencaching_SeriesID = (OpenCachingChildNode).Attribute("id").Value.ToString();
                                    break;
                                default:
                                    throw new Exception("Unhandled for Child Object: " + OpenCachingChildNode.Name.LocalName.ToString());
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    public class GPSBoundary
    {
        public GPSCoordinates Min = new GPSCoordinates();
        public GPSCoordinates Max = new GPSCoordinates();

        public GPSBoundary()
        {
        }
    }

    public class GPSCoordinates
    {
        public string lat = "";
        public string lon = "";

        public GPSCoordinates()
        {
        }
    }

    public class Attribute : IComparable<Attribute>
    {
        public string ID = "";
        public string Inc = "";
        public string Description = "";

        public Attribute(XElement AttributeNode)
        {
            ID = AttributeNode.Attribute("id").Value.ToString();
            Inc = AttributeNode.Attribute("inc").Value.ToString();
            Description = AttributeNode.Value;
        }

        public Attribute()
        {
        }


        public int CompareTo(Attribute other)
        {
            return this.Description.CompareTo(other.Description);

        }
    }

    //Route ans Route Points

    public class rte
    {
        public string Name = "";
        public string Desc = "";
        public string Number = "";
        public string URL = "";
        public string URLName = "";
        public List<rtept> RoutePoints = new List<rtept>();//temporary test remove public later

        public rte(XElement Node)
        {
            foreach (XElement ChildNode in Node.Elements())
            {
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "name":
                        Name = ChildNode.Value;
                        break;
                    case "desc":
                        Desc = ChildNode.Value;
                        break;
                    case "number":
                        Number = ChildNode.Value;
                        break;
                    case "rtept":
                        rtept RoutePoint = new rtept(ChildNode);
                        RoutePoints.Add(RoutePoint);
                        break;
                    case "url":
                        URL = ChildNode.Value;
                        break;
                    case "urlname":
                        URLName = ChildNode.Value;
                        break;
                    case "topografix:color":case "src":case "type":
                        // TODO: Handle unsupported attributes - src, type, etc.
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    public class rtept
    {
        public string Lat = "";
        public string Lon = "";
        public string Ele = "";
        public string Time = "";
        public string Name = "";
        public string Cmt = "";
        public string Desc = "";
        public string Sym = "";
        public string Type = "";
        public string URL = "";
        public string URLName = "";

        public rtept(XElement Node)
        {
            Lat = Node.Attribute("lat").Value.ToString();
            Lon = Node.Attribute("lon").Value.ToString();
            foreach (XElement ChildNode in Node.Elements())
            {
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "ele":
                        Ele = ChildNode.Value;
                        break;
                    case "time":
                        Time = ChildNode.Value;
                        break;
                    case "name":
                        Name = ChildNode.Value;
                        break;
                    case "cmt":
                        Cmt = ChildNode.Value;
                        break;
                    case "desc":
                        Desc = ChildNode.Value;
                        break;
                    case "sym":
                        Sym = ChildNode.Value;
                        break;
                    case "type":
                        Type = ChildNode.Value;
                        break;
                    case "url":
                        URL = ChildNode.Value;
                        break;
                    case "urlname":
                        URLName = ChildNode.Value;
                        break;
                    case "topografix:leg":
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    //Tracks

    public class trk
    {
        public string Name = "";
        public string Desc = "";
        public string Number = "";
        public string URL = "";
        public string URLName = "";
        List<trkseg> Segments = new List<trkseg>();

        public trk(XElement Node)
        {
            System.Diagnostics.Debug.WriteLine(Node.Name);
            foreach (XElement ChildNode in Node.Elements())
            {
                System.Diagnostics.Debug.WriteLine(ChildNode.Name);
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "name":
                        Name = ChildNode.Value;
                        break;
                    case "desc":
                        Desc = ChildNode.Value;
                        break;
                    case "number":
                        Number = ChildNode.Value;
                        break;
                    case "trkseg":
                        trkseg Segment = new trkseg(ChildNode);
                        Segments.Add(Segment);
                        break;
                    case "url":
                        URL = ChildNode.Value;
                        break;
                    case "urlname":
                        URLName = ChildNode.Value;
                        break;
                    case "topografix:color":
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    public class trkseg
    {
        List<trkpt> TrackPoints = new List<trkpt>();

        public trkseg(XElement Node)
        {
            foreach (XElement ChildNode in Node.Elements())
            {
                System.Diagnostics.Debug.WriteLine(ChildNode.Name);
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "trkpt":
                        trkpt TrackPoint = new trkpt(ChildNode);
                        TrackPoints.Add(TrackPoint);
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }

    public class trkpt
    {
        public string Lat = "";
        public string Lon = "";
        public string Sym = "";
        public string Ele = "";
        public string Time = "";
        public string Cmt = "";
        public string Name = "";
        public string Desc = "";

        public trkpt(XElement Node)
        {
            Lat = Node.Attribute("lat").Value.ToString();
            Lon = Node.Attribute("lon").Value.ToString();
            foreach (XElement ChildNode in Node.Elements())
            {
                switch (ChildNode.Name.LocalName.ToString())
                {
                    case "sym":
                        Sym = ChildNode.Value;
                        break;
                    case "ele":
                        Ele = ChildNode.Value;
                        break;
                    case "time":
                        Time = ChildNode.Value;
                        break;
                    case "cmt":
                        Cmt = ChildNode.Value;
                        break;
                    case "name":
                        Name = ChildNode.Value;
                        break;
                    case "desc":
                        Desc = ChildNode.Value;
                        break;
                    default:
                        throw new Exception("Unhandled for Child Object: " + ChildNode.Name.LocalName.ToString());
                }
            }
        }
    }
}