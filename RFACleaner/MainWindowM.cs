using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace RFACleaner
{
    /// <summary>
    /// Main Model class of this application.
    /// This is where all data are processed, managed, edited, imported, exported.
    /// Part of MVVM pattern.
    /// </summary>
    public class MainWindowM
    {
        MainWindowVM mwvm;

        List<ImageSource> icons = new List<ImageSource>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mvm">ViewModel for this Model</param>
        public MainWindowM(MainWindowVM mvm)
        {
            mwvm = mvm;
            revitFiles = new List<SavedRevitFile>();
            icons.Add(ResxBitmap(Properties.Resources.RFA_256px));
            icons.Add(ResxBitmap(Properties.Resources.RVT_256px));
            icons.Add(ResxBitmap(Properties.Resources.RFT_256px));
            icons.Add(ResxBitmap(Properties.Resources.RTE_256px));
        }

        private List<SavedRevitFile> revitFiles = new List<SavedRevitFile>();

        /// <summary>
        /// Get application version.
        /// </summary>
        /// <returns>App version as string</returns>
        public string Version()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            return $"RFA Cleaner - v{FileVersionInfo.GetVersionInfo(a.Location).FileVersion}";
        }

        /// <summary>
        /// Show to user a Folder Browser Dialog
        /// </summary>
        /// <returns>The selected path.</returns>
        public string Browse()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            string fPath = "";

            if((result == DialogResult.OK || result == DialogResult.Yes) && Directory.Exists(fbd.SelectedPath))
            {
                fPath = fbd.SelectedPath;
            }
            else
            {
                MessageBoxResult r = MessageBox.Show($"Le chemin saisi \"{fbd.SelectedPath}\" n'est pas un chemin valide.\n" +
                    $"Voulez-vous choisir un nouveau dossier ?",
                    "RFA Cleaner - Erreur",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (r == MessageBoxResult.Yes)
                {
                    fPath = Browse();
                }
            }

            return fPath;
        }

        private BitmapSource ResxBitmap(Bitmap img)
        {
            BitmapSource btmSrc;


            btmSrc = Imaging.CreateBitmapSourceFromHBitmap(
                img.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return btmSrc;
        }

        /// <summary>
        /// Look after all RFA, RVT, RFT, RTE files in folder.
        /// Only files that match this type of pattern :
        /// nom_du_fichier.xxxx.rfa/rvt/rft/rte
        /// are considered and processed.
        /// This method work in a separed thread. Cannot interract with UI (like ObservableCollection).
        /// </summary>
        /// <param name="folderPath">Folder path to look into.</param>
        public void GetRevitFiles(string folderPath)
        {
            mwvm.ActionButtonText = "   Analyse des fichiers   ";
            Task.Run(() =>
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                List<FileInfo> files = dir.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(s => s.Name.EndsWith(".rfa") || s.Name.EndsWith(".rvt") || s.Name.EndsWith(".rft") || s.Name.EndsWith(".rte"))
                .ToList();

                // For each Revit file, we will look if it is a saved file and create a custom ViewModel of it, binded to the view.
                foreach (FileInfo file in files)
                {
                    if (IsSavedFile(file.FullName))
                    {
                        SavedRevitFile obj = new SavedRevitFile()
                        {
                            FileName = Path.GetFileNameWithoutExtension(file.FullName),
                            FilePath = file.FullName,
                            IsSelected = true,
                            FileUid = SetNewGuid(),
                            FileWeight = file.Length
                        };

                        string ext = Path.GetExtension(file.FullName);
                        switch (ext)
                        {
                            case ".rfa":
                                {
                                    obj.Icon = icons[0];
                                    break;
                                }
                            case ".rvt":
                                {
                                    obj.Icon = icons[1];
                                    break;
                                }
                            case ".rft":
                                {
                                    obj.Icon = icons[2];
                                    break;
                                }
                            case ".rte":
                                {
                                    obj.Icon = icons[3];
                                    break;
                                }
                            default:
                                break;
                        }
                        revitFiles.Add(obj);
                    }
                }

            });
            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
            GetFileWeight();
        }

        /*
         * WIP
        private string GetRevitFileVersion(string filePath)
        {
            string version = "";

            List<Encoding> e = new List<Encoding>
                {
                    Encoding.BigEndianUnicode,
                    Encoding.Unicode,
                    Encoding.ASCII,
                    Encoding.Default,
                    Encoding.UTF32,
                    Encoding.UTF7,
                    Encoding.UTF8
                };

            bool succes = false;

            foreach (Encoding encode in e)
            {
                const int MAX_BUFFER = 4096;
                byte[] buffer = new byte[MAX_BUFFER];
                int bytesRead;
                using (System.IO.FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    while ((bytesRead = fileStream.Read(buffer, 0, MAX_BUFFER)) != 0)
                    {
                        // Contenu du buffer en string
                        string fileContent = encode.GetString(buffer);
                        int index = fileContent.IndexOf("Build: ");

                        //Si la chaine contient le "build"
                        if (index > 0)
                        {
                            fileContent = fileContent.Remove(0, index);
                            fileContent = fileContent.Substring(0, fileContent.IndexOf(')') + 1);

                            version = fileContent.Substring(7, 4);
                            int realVersion = 0;

                            if (int.TryParse(version, out realVersion))
                            {
                                succes = true;
                                break;
                            }
                        }
                    }
                    if (succes)
                    {
                        break;
                    }
                    else
                    {
                        version = "Unknow";
                    }
                }
            }

            return version;

        }
        */

        private bool IsSavedFile(string file)
        {
            string[] composants = Path.GetFileNameWithoutExtension(file).Split('.');

            if(composants.Length > 1)
            {
                int i = 0;
                if(int.TryParse(composants[composants.Length-1], out i))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Select all files for all SavedRevitFile in list.
        /// It doesn't look after filter or anything. Maybe in next update.
        /// </summary>
        public void SelectAllFiles()
        {
            foreach(SavedRevitFile srf in revitFiles.Where(f => !f.IsSelected))
            {
                srf.IsSelected = true;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        /// <summary>
        /// Deselect all files for all SavedRevitFile in list.
        /// It doesn't look after filter or anything. Maybe in next update.
        /// </summary>
        public void UnSelectAllFiles()
        {
            foreach (SavedRevitFile srf in revitFiles.Where(f => f.IsSelected))
            {
                srf.IsSelected = false;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        /// <summary>
        /// Invert file selection for all SavedRevitFile in list.
        /// It doesn't look after filter or anything. Maybe in next update.
        /// </summary>
        public void InvertFilesSelection()
        {
            foreach (SavedRevitFile srf in revitFiles)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        /// <summary>
        /// Change selection of a given parameter identify by it's GUID
        /// </summary>
        /// <param name="uid">a GUID as string</param>
        public void Select(string uid)
        {
            SavedRevitFile srf = revitFiles.Where(t => t.FileUid == uid).FirstOrDefault();

            if(srf != null)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        /// <summary>
        /// Filter all SavedRevitFile in program by looking at user input.
        /// User can active case sensitive.
        /// User can use @ to focus a directory name ex: @Project
        /// User can combine all the filter in the same research.
        /// This method work in a separed thread. Cannot interract with UI (like ObservableCollection).
        /// </summary>
        /// <param name="filtre">Input by user</param>
        /// <param name="isCaseSensitive">Do the program need to check case sensitive ?</param>
        public void Filter(string filtre, bool isCaseSensitive)
        {
            Task.Run(() =>
            {
                if (filtre != null)
                {
                    #region Looking for @

                    List<int> indexes = IndexesOf(filtre, '@');
                    List<string> folders = new List<string>();

                    foreach(int index in indexes)
                    {
                        string tempFolder = "";
                        for(int i = index; i < filtre.Length +1; i++)
                        {
                            if (i == filtre.Length || filtre[i] == ' ')
                            {
                                folders.Add(tempFolder.Replace('@', ' ').Trim());
                                break;
                            }
                            else
                            {
                                tempFolder += filtre[i];
                            }
                        }
                    }

                    string alteredFilter = filtre;

                    foreach(string f in folders)
                    {
                        alteredFilter = alteredFilter.Replace($"@{f}", "").Trim();
                    }

                    #endregion

                    foreach (SavedRevitFile srf in revitFiles)
                    {
                        if (isCaseSensitive)
                        {
                            if (folders.Any(f => srf.FileName.Contains(f)) || srf.FileName.Contains(alteredFilter))
                            {
                                    srf.MatchFilter = true;
                            }
                            else
                            {
                                srf.MatchFilter = false;
                            }
                        }
                        else
                        {
                            if (folders.Any(f => srf.FileName.ToLowerInvariant().Contains(f.ToLowerInvariant())) || srf.FileName.ToLowerInvariant().Contains(alteredFilter.ToLowerInvariant()))
                            {
                                mwvm.FilesList.Add(srf);
                            }
                        }
                    }
                }
            });
            
            GetFileWeight();
            RefreshUI();
        }

        /// <summary>
        /// Define a new GUID wich doesn't exist for any SavedRevitFile in program.
        /// </summary>
        /// <returns>A new GUID as string</returns>
        private string SetNewGuid()
        {
            string uid = Guid.NewGuid().ToString();

            if(revitFiles.Any(t => t.FileUid == uid)) return SetNewGuid();
            else return uid;
        }

        /// <summary>
        /// Separed Thread
        /// Get total weight of all selected files.
        /// </summary>
        public void GetFileWeight()
        {
            long totalWeight = 0;
            int countFiles = 0;
            int u = 0;
            string weightunit = "o";

            Task.Run(() =>
            {
                foreach (SavedRevitFile srf in revitFiles.Where(t => t.IsSelected))
                {
                    totalWeight += srf.FileWeight;
                    countFiles++;
                }

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
            });

            string nbfichiers = "";

            nbfichiers = countFiles > 1 ? $"{countFiles} fichiers" : $"{countFiles} fichier";

            mwvm.ActionButtonText = $"   Purger {nbfichiers} pour {totalWeight} {weightunit}   ";
        }

        /// <summary>
        /// This method all files that has been selected by user in the interface.
        /// File are completly deleted and cannot be recovered manually.
        /// </summary>
        public void FileToBean()
        {
            string beanPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            foreach(SavedRevitFile srf in revitFiles.Where(t => t.IsSelected))
            {
                File.Delete(srf.FilePath);
            }
            revitFiles.RemoveAll(t => t.IsSelected);

            mwvm.FilesList.Clear();

            foreach(SavedRevitFile srf in revitFiles)
            {
                srf.IsSelected = true;
                mwvm.FilesList.Add(srf);
            }

            mwvm.ActionButtonText = "   Suppression effectuée   ";
        }

        /// <summary>
        /// Get list of indexes of given char in given string
        /// </summary>
        /// <param name="s">string to look into</param>
        /// <param name="c">char to look for</param>
        /// <returns>List of indexes as int</returns>
        private List<int> IndexesOf (string s, char c)
        {
            List<int> list = new List<int>();

            for (int i = s.IndexOf(c); i > -1; i = s.IndexOf(c, i + 1))
            {
                // for loop end when i=-1 ('a' not found)
                list.Add(i);
            }

            return list;
        }

        /// <summary>
        /// Get all data from list in model and parse it to Observable collection for view.
        /// Waiting for a best way to do it in another thread than UI to keep it free to user.
        /// </summary>
        public void RefreshUI()
        {
            mwvm.FilesList.Clear ();
            foreach(SavedRevitFile srf in revitFiles)
            {
                mwvm.FilesList.Add(srf);
            }
        }
    }
}
