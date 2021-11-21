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
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowM mwm;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChange(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindowVM()
        {
            mwm = new MainWindowM(this);
            filesList = new ObservableCollection<SavedRevitFile>();

            browseFolder = new CommandeRelais(Execute_BrowseFolder, CanExecute_BrowseFolder);
            mainAction = new CommandeRelais(Execute_MainAction, CanExecute_MainAction);
            selectAll = new CommandeRelais(Execute_SelectAll, CanExecute_SelectAll);
            unselectAll = new CommandeRelais(Execute_UnSelectAll, CanExecute_UnSelectAll);
            invertSelection = new CommandeRelais(Execute_InvertSelection, CanExecute_InvertSelection);
            WindowTitle = mwm.Version();
        }

        private string windowTitle;

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
                        mwm.GetRevitFiles(folderPath);
                    }
                }
            }
        }

        private string searchText;

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
                    mwm.Filter(searchText);
                    OnPropertyChange("SearchText");
                }
            }
        }

        private ObservableCollection<SavedRevitFile> filesList;

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
            }
        }

        #region Command Browse Folder

        private ICommand browseFolder;

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

        public void Execute_BrowseFolder(object parameter)
        {
            string result = mwm.Browse();
            if(FolderPath != "" && result != "")
            {
                FolderPath = result;
            }
        }

        public bool CanExecute_BrowseFolder(object parameter)
        {
            return true;
        }

        #endregion

        #region Command Main Action

        private ICommand mainAction;

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

        public void Execute_MainAction(object parameter)
        {

        }

        public bool CanExecute_MainAction(object parameter)
        {
            return true;
        }

        #endregion

        #region Command SelectAll

        private ICommand selectAll;

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
            mwm.SelectAllFiles();
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
            mwm.UnSelectAllFiles();
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
            mwm.InvertFilesSelection();
        }

        public bool CanExecute_InvertSelection(object parameter)
        {
            return true;
        }

        #endregion

    }
}
