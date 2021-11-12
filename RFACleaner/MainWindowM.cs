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

namespace RFACleaner
{
    public class MainWindowM
    {
        public MainWindowM()
        {

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
    }
}
