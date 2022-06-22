using Editor.GameEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.TrackEditor.ParametersEditor
{
    class ParametersEditorVM : INotifyPropertyChanged
    {
        ParametersEditorModel model = null;
        public ParametersEditorModel Model 
        { 
            get => model; 
            set
            {
                model = value;
                OnPropertyChanged("Parameters");
                model.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
            }
        }

        public TrackParameters Parameters { get => model?.Parameters; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
