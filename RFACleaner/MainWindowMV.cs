using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RFACleaner
{
    public class MainWindowMV : INotifyPropertyChanged
    {
        MainWindowM mwm;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChange(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindowMV()
        {
            mwm = new MainWindowM();
            filesList = new ObservableCollection<SavedRevitFile>();

            browseFolder = new CommandeRelais(Execute_BrowseFolder, CanExecute_BrowseFolder);
            mainAction = new CommandeRelais(Execute_MainAction, CanExecute_MainAction);
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
                folderPath = value;
                OnPropertyChange("FolderPath");
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
                searchText = value;
                OnPropertyChange("SearchText");
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
            FolderPath = mwm.Browse();
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

    }
}
