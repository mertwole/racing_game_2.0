using System.ComponentModel;
using System.Windows.Media;

namespace Editor.GameEntities
{
    public class TrackParameters : INotifyPropertyChanged
    {
        double length = 100.0;
        public double Length 
        { 
            get => length; 
            set { length = value; OnPropertyChanged("Length"); } 
        }

        Color mainColor = Color.FromArgb(255, 100, 100, 100);
        public Color MainColor 
        { 
            get => mainColor;
            set { mainColor = value; OnPropertyChanged("MainColor"); } 
        }

        Color secondaryColor = Color.FromArgb(255, 200, 200, 200);
        public Color SecondaryColor { 
            get => secondaryColor; 
            set { secondaryColor = value; OnPropertyChanged("SecondaryColor"); } 
        }

        public TrackParameters()
        {

        }

        public TrackParameters(TrackParameters prototype)
        {
            length = prototype.length;
            mainColor = prototype.mainColor;
            secondaryColor = prototype.secondaryColor;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
