using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RFACleaner
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowModel model;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChange(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Constructeur de class
        /// </summary>
        public MainWindowViewModel()
        {
            CaseSensitiveTag = "NotSelected";

            filesList = new ObservableCollection<SavedRevitFile>();

            browseFolder = new CommandeRelais(Execute_BrowseFolder, CanExecute_BrowseFolder);
            mainAction = new CommandeRelais(Execute_MainAction, CanExecute_MainAction);
            selectAll = new CommandeRelais(Execute_SelectAll, CanExecute_SelectAll);
            unselectAll = new CommandeRelais(Execute_UnSelectAll, CanExecute_UnSelectAll);
            invertSelection = new CommandeRelais(Execute_InvertSelection, CanExecute_InvertSelection);
            caseSensitive = new CommandeRelais(Execute_CaseSensitive, CanExecute_CaseSensitive);

            model = new MainWindowModel(this);

            WindowTitle = model.Version();
        }

        private string windowTitle;

        /// <summary>
        /// Titre de la fenêtre.
        /// </summary>
        public string WindowTitle
        {
            get 
            { 
                return windowTitle; 
            }
            set 
            { 
                windowTitle = value;
                OnPropertyChange("WindowTitle");
            }
        }

        private string folderPath;

        /// <summary>
        /// Chemin de recherche actuel.
        /// </summary>
        public string FolderPath
        {
            get 
            { 
                return folderPath; 
            }
            set 
            { 
                if(value != folderPath)
                {
                    folderPath = value;
                    OnPropertyChange("FolderPath");

                    if (Directory.Exists(folderPath))
                    {
                        model.GetRevitFiles(folderPath);
                    }
                }
            }
        }

        private string searchText;

        /// <summary>
        /// Filtre de recherche.
        /// </summary>
        public string SearchText
        {
            get 
            { 
                return searchText; 
            }
            set 
            {
                if(value != searchText)
                {
                    searchText = value;
                    model.Filter(searchText, CaseSensitiveTag == "Selected");
                    OnPropertyChange("SearchText");
                }
            }
        }

        private ObservableCollection<SavedRevitFile> filesList;

        /// <summary>
        /// Liste des fichiers de sauvegarde Revit affichés dans l'interface.
        /// </summary>
        public ObservableCollection<SavedRevitFile> FilesList
        {
            get 
            { 
                return filesList; 
            }
            set 
            {
                filesList = value;
                OnPropertyChange("FilesList");
            }
        }

        private string actionButtonText;

        /// <summary>
        /// Contenu du bouton pour lancer le nettoyage.
        /// </summary>
        public string ActionButtonText
        {
            get 
            { 
                return actionButtonText; 
            }
            set 
            { 
                actionButtonText = value;
                OnPropertyChange("ActionButtonText");
                if (value == "" || value == null)
                {
                    ActionButtonEnable = false;
                }
                else
                {
                    ActionButtonEnable = true;
                }
            }
        }

        private bool actionButtonEnable;

        /// <summary>
        /// Défini si le bouton est actif pour l'utilisateur.
        /// </summary>
        public bool ActionButtonEnable
        {
            get { return actionButtonEnable; }
            set 
            {
                actionButtonEnable = value;
                OnPropertyChange("ActionButtonEnable");
            }
        }

        private string caseSensitiveTag;

        /// <summary>
        /// Defini le tag du bouton CaseSensitive (permet de changer son apparence) "Selected" ou "".
        /// </summary>
        public string CaseSensitiveTag
        {
            get { return caseSensitiveTag; }
            set 
            { 
                caseSensitiveTag = value;
                if(value == "Selected")
                {
                    CaseSensitiveToolTip = "Le filtre tiens compte des majuscules et minuscules";
                }
                else
                {
                    CaseSensitiveToolTip = "Le filtre ne tiens compte des majuscules et minuscules";
                }
                OnPropertyChange("CaseSensitiveTag");
            }
        }

        private string caseSensitiveToolTip;

        /// <summary>
        /// Information à l'utilisateur de la manière dont foncitonne le bouton CaseSensitive.
        /// </summary>
        public string CaseSensitiveToolTip
        {
            get { return caseSensitiveToolTip; }
            set 
            { 
                caseSensitiveToolTip = value;
                OnPropertyChange("CaseSensitiveToolTip");
            }
        }


        #region Command Browse Folder

        private ICommand browseFolder;

        /// <summary>
        /// Iterface de commande permettant de lancer la fonction de recherche de dossier.
        /// </summary>
        public ICommand BrowseFolder
        {
            get 
            { 
                return browseFolder; 
            }
            set 
            { 
                browseFolder = value; 
            }
        }

        /// <summary>
        /// Commande de recherche de dossier.
        /// </summary>
        /// <param name="parameter">Paramètres.</param>
        public void Execute_BrowseFolder(object parameter)
        {
            string result = model.Browse();
            if(FolderPath != "" && result != "")
            {
                FolderPath = result;
                model.UpdateUI();
            }
        }

        /// <summary>
        /// Vérifie si le bouton de recherche de dossier est actif.
        /// </summary>
        /// <param name="parameter">Paramètres.</param>
        /// <returns>Vrai.</returns>
        public bool CanExecute_BrowseFolder(object parameter)
        {
            return true;
        }

        #endregion

        #region Command Main Action

        private ICommand mainAction;

        /// <summary>
        /// Interface de commande qui anime l'action principale du programme.
        /// </summary>
        public ICommand MainAction
        {
            get
            {
                return mainAction;
            }
            set
            {
                mainAction = value;
            }
        }


        /// <summary>
        /// Action principale du programme.
        /// </summary>
        /// <param name="parameter">Paramètres.</param>
        public void Execute_MainAction(object parameter)
        {
            model.FileToBean();
        }

        /// <summary>
        /// Vérifie si le bouton d'action principal est actif.
        /// </summary>
        /// <param name="parameter">Paramètres.</param>
        /// <returns>Vrai.</returns>
        public bool CanExecute_MainAction(object parameter)
        {
            return true;
        }

        #endregion

        #region Command SelectAll

        private ICommand selectAll;

        /// <summary>
        /// Interface de commande pour selectionner tout les fichiers de sauvergarde Revit que peu contenir le programme.
        /// </summary>
        public ICommand SelectAll
        {
            get
            {
                return selectAll;
            }
            set
            {
                selectAll = value;
            }
        }

        public void Execute_SelectAll(object parameter)
        {
            model.TreatAllFiles(true);
        }

        public bool CanExecute_SelectAll(object parameter)
        {
            return true;
        }

        #endregion

        #region Command UnSelectAll

        private ICommand unselectAll;

        public ICommand UnSelectAll
        {
            get
            {
                return unselectAll;
            }
            set
            {
                unselectAll = value;
            }
        }

        public void Execute_UnSelectAll(object parameter)
        {
            model.TreatAllFiles(false);
        }

        public bool CanExecute_UnSelectAll(object parameter)
        {
            return true;
        }

        #endregion

        #region Command InvertSelection

        private ICommand invertSelection;

        public ICommand InvertSelection
        {
            get
            {
                return invertSelection;
            }
            set
            {
                invertSelection = value;
            }
        }

        public void Execute_InvertSelection(object parameter)
        {
            model.InvertFilesSelection();
        }

        public bool CanExecute_InvertSelection(object parameter)
        {
            return true;
        }

        #endregion

        #region Command CaseSensitive

        private ICommand caseSensitive;

        public ICommand CaseSensitive
        {
            get
            {
                return caseSensitive;
            }
            set
            {
                caseSensitive = value;
            }
        }

        public void Execute_CaseSensitive(object parameter)
        {
            if(CaseSensitiveTag == "Selected")
            {
                CaseSensitiveTag = "NotSelected";
            }
            else
            {
                CaseSensitiveTag = "Selected";
            }
        }

        public bool CanExecute_CaseSensitive(object parameter)
        {
            return true;
        }

        #endregion

        #region Application size

        private double appSize;
        public double AppSize
        {
            get 
            { 
                return appSize; 
            }
            set 
            { 
                appSize = value;
                TwoColumnMaxSize = appSize - 240;
                OnPropertyChange("AppSize");
            }
        }

        private double twoColumnMaxSize;
        public double TwoColumnMaxSize
        {
            get 
            { 
                return twoColumnMaxSize; 
            }
            set 
            { 
                twoColumnMaxSize = value;
                OnPropertyChange("TwoColumnMaxSize");
            }
        }


        #endregion

    }
}
