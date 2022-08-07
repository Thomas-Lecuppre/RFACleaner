using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace RFACleaner
{
    /// <summary>
    /// Class représentant un fichier de sauvegarde Revit au format RFA, RFT, RVT, RTE
    /// </summary>
    public class SavedRevitFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Evenement de changement de valeur de propriété.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Change la valeur de la propiété dans la vue une fois appelé.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété</param>
        protected virtual void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        MainWindowM mainM;

        /// <summary>
        /// Constructor
        /// </summary>
        public SavedRevitFile(MainWindowM mwm)
        {
            mainM = mwm;
            selectCommand = new CommandeRelais(Execute_Select, CanExecute_Select);
        }

        /// <summary>
        /// Nom du fichier
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Chemin du fichier
        /// </summary>
        public string FilePath { get; set; }

        private bool isSelected;
        /// <summary>
        /// Etat de selection du fichier.
        /// </summary>
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
        /// <summary>
        /// Définie m'état du bouton du fichier de sauvegarde. Selected / NotSelected
        /// </summary>
        public string FileTag
        {
            get { return fileTag; }
            set 
            { 
                fileTag = value;
                OnPropertyChange("FileTag");
            }
        }

        private ImageSource icon;
        /// <summary>
        /// Icone du fichier en fonction de son type.
        /// </summary>
        public ImageSource Icon
        {
            get { return icon; }
            set 
            { 
                icon = value;
                OnPropertyChange("Icon");
            }
        }

        private long fileWeight;
        /// <summary>
        /// Poids du fichier.
        /// </summary>
        public long FileWeight
        {
            get 
            { 
                return fileWeight; 
            }
            set 
            { 
                fileWeight = value;
                OnPropertyChange("Weight");
            }
        }

        /// <summary>
        /// Poids du fichier lisible pour un utilisateur.
        /// </summary>
        public string Weight
        {
            get
            {
                return GetWeightInfo(FileWeight);
            }
        }

        /// <summary>
        /// Le fichier est-il visé par le filtre ?
        /// </summary>
        public bool MatchFilter { get; set; }

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

        #region Select Command

        private ICommand selectCommand;

        /// <summary>
        /// Commande derrière le clique du bouton.
        /// </summary>
        public ICommand SelectCommand
        {
            get { return selectCommand; }
            set 
            { 
                selectCommand = value; 
            }
        }

        /// <summary>
        /// Execute la fonction.
        /// </summary>
        /// <param name="parameter">Objet</param>
        public void Execute_Select(object parameter)
        {
            IsSelected = !IsSelected;
            mainM.GetFileWeight();
        }

        /// <summary>
        /// Peut executer la fonction ?
        /// </summary>
        /// <param name="parameter">Objet</param>
        /// <returns></returns>
        public bool CanExecute_Select(object parameter)
        {
            return true;
        }

        #endregion
    }
}
