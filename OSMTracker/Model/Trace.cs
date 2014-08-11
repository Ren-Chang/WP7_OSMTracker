using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OSMTracker.ViewModels
{
    public class Trace : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public DateTime gen;
        public int pts;

        private string _lineTwo;
        public string LineTwo
        {
            get
            {
                return _lineTwo;
            }
            set
            {
                if (value != _lineTwo)
                {
                    _lineTwo = value;
                    NotifyPropertyChanged("LineTwo");
                }
            }
        }

        public Trace()
        { }

        public Trace(string traceName, DateTime created, int number)
        {
            Name = traceName; gen = created; pts = number;
            LineTwo = created.ToShortDateString() + " " + created.ToShortTimeString() + " | " + number + " pts";

            _name = Name; _lineTwo = LineTwo;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class Traces
    {
        public List<Trace> traces;
        public Traces(){}
    }
}
