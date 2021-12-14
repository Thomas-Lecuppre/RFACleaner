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
    public class MainWindowM
    {
        MainWindowVM mwvm;

        List<ImageSource> icons = new List<ImageSource>();

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

        public string Version()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            return $"RFA Cleaner - v{FileVersionInfo.GetVersionInfo(a.Location).FileVersion}";
        }

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

        public async void GetRevitFiles(string folderPath)
        {
            mwvm.ActionButtonText = "   Analyse des fichiers   ";
            await App.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                List<FileInfo> files = dir.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(s => s.Name.EndsWith(".rfa") || s.Name.EndsWith(".rvt") || s.Name.EndsWith(".rft") || s.Name.EndsWith(".rte"))
                .ToList();

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
                        mwvm.FilesList.Add(obj);
                    }
                }

            }));

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

        public void SelectAllFiles()
        {
            foreach(SavedRevitFile srf in revitFiles.Where(f => !f.IsSelected))
            {
                srf.IsSelected = true;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        public void UnSelectAllFiles()
        {
            foreach (SavedRevitFile srf in revitFiles.Where(f => f.IsSelected))
            {
                srf.IsSelected = false;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        public void InvertFilesSelection()
        {
            foreach (SavedRevitFile srf in revitFiles)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        public void Select(string uid)
        {
            SavedRevitFile srf = revitFiles.Where(t => t.FileUid == uid).FirstOrDefault();

            if(srf != null)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText, mwvm.CaseSensitiveTag == "Selected");
        }

        public async void Filter(string filtre, bool isCaseSensitive)
        {
            await App.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                if (filtre != null)
                {
                    mwvm.FilesList.Clear();
                    foreach (SavedRevitFile srf in revitFiles)
                    {
                        if (isCaseSensitive)
                        {
                            if (srf.FileName.Contains(filtre))
                            {
                                mwvm.FilesList.Add(srf);
                            }
                        }
                        else
                        {
                            if (srf.FileName.ToLowerInvariant().Contains(filtre.ToLowerInvariant()))
                            {
                                mwvm.FilesList.Add(srf);
                            }
                        }
                    }
                }
            }));
            
            GetFileWeight();
        }

        private string SetNewGuid()
        {
            string uid = Guid.NewGuid().ToString();

            if(revitFiles.Any(t => t.FileUid == uid)) return SetNewGuid();
            else return uid;
        }

        public void GetFileWeight()
        {
            long totalWeight = 0;
            IEnumerable<SavedRevitFile> temp = revitFiles.Where(t => t.IsSelected);

            foreach (SavedRevitFile srf in temp)
            {
                totalWeight += srf.FileWeight;
            }

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

            string nbfichiers = "";

            if (temp.Count() > 1) nbfichiers = $"{temp.Count()} fichiers";
            else nbfichiers = $"{temp.Count()} fichier";

            mwvm.ActionButtonText = $"   Purger {nbfichiers} pour {totalWeight} {weightunit}   ";
        }

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
        }
    }
}
