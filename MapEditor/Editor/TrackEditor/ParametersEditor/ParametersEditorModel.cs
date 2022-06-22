using Editor.GameEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.TrackEditor.ParametersEditor
{
    class ParametersEditorModel : INotifyPropertyChanged
    {
        TrackParameters parameters;
        public TrackParameters Parameters { get => parameters; }

        public ParametersEditorModel(Track track)
        {
            parameters = track.Parameters;
            parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
