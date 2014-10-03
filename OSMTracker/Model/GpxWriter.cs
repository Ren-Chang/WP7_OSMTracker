using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Device.Location;

namespace OSMTracker
{
    public class GpxWriter
    {
        private string _gpxFile = "";
        private IList<GeoPosition<GeoCoordinate>> _lstGpsInfo = null;

        private XmlWriterSettings _settings = null;

        public GpxWriter(string gpxFile)
        {
            _settings = new XmlWriterSettings();
            _settings.Indent = true;
            _settings.Encoding = new UTF8Encoding(false);
            _settings.NewLineChars = Environment.NewLine;

            _lstGpsInfo = new List<GeoPosition<GeoCoordinate>>();

            _gpxFile = gpxFile;
        }

        public void AddGpsInfo(GeoPosition<GeoCoordinate> gpsInfo)
        {
            _lstGpsInfo.Add(gpsInfo);
        }

        public void WriteToGpx()
        {
            try
            {
                if (_lstGpsInfo == null || _lstGpsInfo.Count == 0) return;

                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (myIsolatedStorage.FileExists(_gpxFile) == true)
                {
                    myIsolatedStorage.DeleteFile(_gpxFile);
                }

                using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile(_gpxFile, FileMode.CreateNew))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, _settings))
                    {
                        //写xml文件开始<?xml version="1.0" encoding="utf-8" ?>
                        xmlWriter.WriteStartDocument();

                        //写根节点
                        xmlWriter.WriteStartElement("gpx", "http://www.topografix.com/GPX/1/0");
                        //给节点添加属性
                        xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                        xmlWriter.WriteAttributeString("xmlns", "mbx", null, "http://www.motionbased.net/mbx");
                        xmlWriter.WriteAttributeString("xsi", "schemaLocation", null, "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.motionbased.net/mbx http://www.motionbased.net/site/schemas/mbx/0.0.1/mbx.xsd");
                        xmlWriter.WriteAttributeString("creator", "OSMTracker for WindowsPhone - https://github.com/Ren-Chang/WP7_OSMTracker");
                        xmlWriter.WriteAttributeString("version", "1.0");

                        //写内容根节点
                        xmlWriter.WriteStartElement("trk");
                        //写内容根节点
                        xmlWriter.WriteStartElement("trkseg");

                        //写GPS信息节点
                        foreach (GeoPosition<GeoCoordinate> gpsInfo in _lstGpsInfo)
                        {
                            xmlWriter.WriteStartElement("trkpt");

                            //给节点添加属性
                            xmlWriter.WriteAttributeString("lat", gpsInfo.Location.Latitude.ToString());
                            xmlWriter.WriteAttributeString("lon", gpsInfo.Location.Longitude.ToString());

                            //添加子节点
                            xmlWriter.WriteElementString("ele", gpsInfo.Location.Altitude.ToString());
                            xmlWriter.WriteElementString("time", gpsInfo.Timestamp.ToString());
                            xmlWriter.WriteElementString("course", gpsInfo.Location.Course.ToString());
                            xmlWriter.WriteElementString("speed", gpsInfo.Location.Speed.ToString());

                            xmlWriter.WriteEndElement();//trkpt
                        }

                        xmlWriter.WriteEndElement();//trkseg
                        xmlWriter.WriteEndElement();//trk
                        xmlWriter.WriteEndElement();//gpx

                        xmlWriter.WriteEndDocument();
                    }
                }

                _lstGpsInfo.Clear();//清空历史记录
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("写GPX文件错误：" + ex.Message);
            }
        }
    }
}
