using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFACleaner
{
    public class SavedRevitFile
    {
        public SavedRevitFile()
        {

        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private string fileVersion;

        public string FileVersion
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }

        private string fileSelection;

        public string FileSelection
        {
            get { return fileSelection; }
            set { fileSelection = value; }
        }

    }
}
