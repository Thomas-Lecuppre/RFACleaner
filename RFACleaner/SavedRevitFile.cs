using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RFACleaner
{
    public class SavedRevitFile : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SavedRevitFile()
        {

        }

        public string FileName { get; set; }

        public string FileVersion { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get 
            { 
                return isSelected; 
            }
            set 
            { 
                isSelected = value;
                if (isSelected)
                {
                    FileTag = "Selected";
                }
                else
                {
                    FileTag = "NotSelected";
                }
            }
        }

        private string fileTag;

        public string FileTag
        {
            get { return fileTag; }
            set 
            { 
                fileTag = value;
                OnPropertyChange("FileTag");
            }
        }


        public ImageSource Icon { get; set; }

        private string fileUid;
        public string FileUid
        {
            get 
            { 
                return fileUid; 
            }
            set 
            { 
                fileUid = value;
                OnPropertyChange("FileUid");
            }
        }

        private long fileWeight;
        public long FileWeight
        {
            get 
            { 
                return fileWeight; 
            }
            set 
            { 
                fileWeight = value; 
            }
        }

        public string Weight
        {
            get
            {
                return GetWeightInfo(FileWeight);
            }
        }

        private string GetWeightInfo(long weight)
        {
            long totalWeight = weight;
            int u = 0;
            string weightunit = "o";

            while (totalWeight > 1000 && u <= 5)
            {
                totalWeight = totalWeight / 1000;
                u++;
                switch (u)
                {
                    case 1:
                        weightunit = "ko";
                        break;
                    case 2:
                        weightunit = "mo";
                        break;
                    case 3:
                        weightunit = "Go";
                        break;
                    case 4:
                        weightunit = "To";
                        break;
                    case 5:
                        weightunit = "Po";
                        break;
                    default:
                        weightunit = "-";
                        break;
                }
            }

            return $"Poids : {totalWeight} {weightunit}";
        }
    }
}
