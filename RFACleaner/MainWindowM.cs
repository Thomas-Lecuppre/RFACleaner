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

namespace RFACleaner
{
    public class MainWindowM
    {
        MainWindowVM mwvm;

        public MainWindowM(MainWindowVM mvm)
        {
            mwvm = mvm;
            revitFiles = new List<SavedRevitFile>();
        }

        private List<SavedRevitFile> revitFiles;
        public List<SavedRevitFile> RevitFiles
        {
            get 
            { 
                return revitFiles; 
            }
            set 
            { 
                revitFiles = value; 
            }
        }

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
                            FileVersion = GetRevitFileVersion(file.FullName),
                            FileUid = SetNewGuid(),
                            FileWeight = file.Length
                        };

                        string ext = Path.GetExtension(file.FullName);
                        switch (ext)
                        {
                            case ".rfa":
                                {
                                    obj.Icon = ResxBitmap(Properties.Resources.RFA_256px);
                                    break;
                                }
                            case ".rvt":
                                {
                                    obj.Icon = ResxBitmap(Properties.Resources.RVT_256px);
                                    break;
                                }
                            case ".rft":
                                {
                                    obj.Icon = ResxBitmap(Properties.Resources.RFT_256px);
                                    break;
                                }
                            case ".rte":
                                {
                                    obj.Icon = ResxBitmap(Properties.Resources.RTE_256px);
                                    break;
                                }
                            default:
                                break;
                        }

                        RevitFiles.Add(obj);
                        mwvm.FilesList.Add(obj);
                    }
                }
            }));

            GetFileWeight();
        }

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
            foreach(SavedRevitFile srf in RevitFiles.Where(f => !f.IsSelected))
            {
                srf.IsSelected = true;
            }

            Filter(mwvm.SearchText);
        }

        public void UnSelectAllFiles()
        {
            foreach (SavedRevitFile srf in RevitFiles.Where(f => f.IsSelected))
            {
                srf.IsSelected = false;
            }

            Filter(mwvm.SearchText);
        }

        public void InvertFilesSelection()
        {
            foreach (SavedRevitFile srf in RevitFiles)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText);
        }

        public void Select(string uid)
        {
            SavedRevitFile srf = RevitFiles.Where(t => t.FileUid == uid).FirstOrDefault();

            if(srf != null)
            {
                srf.IsSelected = !srf.IsSelected;
            }

            Filter(mwvm.SearchText);
        }

        public async void Filter(string filtre)
        {
            await App.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                if (filtre != null)
                {
                    IEnumerable<SavedRevitFile> temp = RevitFiles.Where(t => t.FileName.Contains(filtre));

                    mwvm.FilesList.Clear();
                    foreach (SavedRevitFile srf in temp)
                    {
                        mwvm.FilesList.Add(srf);
                    }
                }
            }));
            
            GetFileWeight();
        }

        private string SetNewGuid()
        {
            string uid = Guid.NewGuid().ToString();

            if(RevitFiles.Any(t => t.FileUid == uid)) return SetNewGuid();
            else return uid;
        }

        public void GetFileWeight()
        {
            long totalWeight = 0;
            IEnumerable<SavedRevitFile> temp = RevitFiles.Where(t => t.IsSelected);

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
            foreach(SavedRevitFile srf in RevitFiles.Where(t => t.IsSelected))
            {
                File.Delete(srf.FilePath);
            }
            RevitFiles.RemoveAll(t => t.IsSelected);

            mwvm.FilesList.Clear();

            foreach(SavedRevitFile srf in RevitFiles)
            {
                srf.IsSelected = true;
                mwvm.FilesList.Add(srf);
            }
        }
    }
}
