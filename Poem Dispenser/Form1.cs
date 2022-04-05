using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Threading;

namespace Poem_Dispenser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            GUIInit();
            GetFileNames();
            RefreshPoems.RunWorkerAsync();
        }

        List<string> PoemFiles;
        Random rnd = new Random();

        private Font printFont;
        private Font titleFont;
        private Font bylineFont;

        //Configurable options
        /*
         * A config file called "poemzconfig.ppl" must be located in the same
         * directory as the PoemDispenser.exe executable. All measurements are
         * in pixels. Contents should follow this format:
         * 
            <poemdirectory>c:\poemfolder\
            <poemfileextension>*.txt
            <logdirectory>c:\poemfolder\
            <logfile>poemlog.log
            <linewidth>300
            <paperwidth>300
            <paperheight>5000
            <leftmargin>10
            <rightmargin>10
            <topmargin>10
            <bottommargin>10
            <font>Times
            <exitmessage>Message printed at the bottom
            <logofile>logofile.jpg
            <printlogo>true
            <printer>default
        */
        string PoemPath = string.Empty;
        string FileExtension = string.Empty;
        string LogDirectory = string.Empty;
        string LogFile = string.Empty;
        string LogoFile = string.Empty;
        string SelectedPrinter = "default";
        int LineWidth = 280;
        int PaperWidth = 300;
        int PaperLength = 2500;
        int TopMargin = 10;
        int BottomMargin = 10;
        int RightMargin = 10;
        int LeftMargin = 10;
        string DefaultFontName = "Arial";
        string ExitMessage = string.Empty;
        bool PrintLogo = false;
        //End configurable options

        TextFile PoemLog = new TextFile();
        TextFile CurrentPoem = new TextFile();

        private List<string>InstalledPrinters ()
            //Creates a list of all installed printer names on the system
            //Adds "(default)" to the printer that is set as the default in Windows
        {
            List<string> result = new List<string>();
            PrinterSettings psettings = new PrinterSettings();
            string pdefault = psettings.PrinterName;
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                string def = string.Empty;
                if (printer == pdefault)
                    def = " (Default)";
                result.Add(printer + def);
            }
            return result;
        }
        private void GUIInit ()
            //Fills in the cb_Printers combobox and sets
            //the selected option to either the default printer
            //or the printer listed in the config file
        {
            List<string> printerlist = InstalledPrinters();
            string currentprinter = SelectedPrinter;
            foreach (string printer in printerlist)
            {
                if (currentprinter == "default" && printer.Contains(" (Default)"))
                    currentprinter = printer;
                cb_Printers.Items.Add(printer);
            }
            cb_Printers.SelectedItem = currentprinter;
        }
        private void SetNewPrinter()
            //Called when a new printer is selected in the cb_Printers combobox
        {
            string newprinter = cb_Printers.SelectedItem.ToString();
            //Set the SelectedPrinter setting to either the name of the selected printer or "default"
            if (!newprinter.Contains(" (Default)"))
                SelectedPrinter = newprinter;
            else
                SelectedPrinter = "default";
            //Find the path to the running .exe
            string appPath = AppContext.BaseDirectory;
            //Open the config file, change the printer option, and save
            if (File.Exists(appPath + "Poemzconfig.ppl"))
            {
                TextFile configfile = new TextFile(appPath, "Poemzconfig.ppl", true);
                int printersetting = -1;
                for (int i = 0; i < configfile.Data().Count; i++)
                {
                    if (configfile.Data()[i].Contains("<printer>"))
                    {
                        printersetting = i;
                        break;
                    }
                }
                if (printersetting >= 0)
                    configfile.UpdateLine(printersetting, "<printer>" + SelectedPrinter);
                else
                    configfile.Add("<printer>" + SelectedPrinter);
            }
        }
        
        private void LoadSettings ()
            //Load settings from the configuration file
        {
            //Get the path to the .exe
            string appPath = AppContext.BaseDirectory;
            if (File.Exists(appPath + "Poemzconfig.ppl"))
            {
                //Open the config file
                TextFile configfile = new TextFile(appPath, "Poemzconfig.ppl", false);
                foreach (string config in configfile.Data())
                {
                    //Split each line to get a configuration field and its value
                    string[] datainfo = config.Split('>');
                    string field = datainfo[0];
                    string value = string.Empty;
                    if (datainfo.Length > 1)
                        value = datainfo[1];
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                        //If the setting has a value, assign the value to the appropriate variable
                    {
                        switch (field)
                        {
                            case "<poemdirectory":
                                PoemPath = value;
                                break;
                            case "<poemfileextension":
                                FileExtension = value;
                                break;
                            case "<logdirectory":
                                LogDirectory = value;
                                break;
                            case "<logfile":
                                LogFile = value;
                                break;
                            case "<linewidth":
                                LineWidth = int.Parse(value);
                                break;
                            case "<paperwidth":
                                PaperWidth = int.Parse(value);
                                break;
                            case "<paperheight":
                                PaperLength = int.Parse(value);
                                break;
                            case "<leftmargin":
                                LeftMargin = int.Parse(value);
                                break;
                            case "<rightmargin":
                                RightMargin = int.Parse(value);
                                break;
                            case "<topmargin":
                                TopMargin = int.Parse(value);
                                break;
                            case "<bottommargin":
                                BottomMargin = int.Parse(value);
                                break;
                            case "<font":
                                DefaultFontName = value;
                                break;
                            case "<exitmessage":
                                ExitMessage = value;
                                break;
                            case "<logofile":
                                LogoFile = value;
                                break;
                            case "<printlogo":
                                PrintLogo = (value.ToLower() == "true");
                                break;
                            case "<printer":
                                SelectedPrinter = value;
                                break;
                        }
                    }
                }
                //Open or create the configured log file
                PoemLog = new TextFile(LogDirectory, LogFile);
            }
        }
        private void GetFileNames ()
            //Make a list of all of the files in the poem directory that have the configured extension (.txt by default)
        {
            string[] files = Directory.GetFiles(PoemPath,FileExtension);
            PoemFiles = new List<string>();
            foreach (string file in files)
            {
                string justfilename = Path.GetFileName(file);
                PoemFiles.Add(justfilename);
            }
        }
        private string PickAFile ()
            //Returns the filename of a poem picked at random from the list of files
        {
            string result = string.Empty;
            if (PoemFiles.Count > 0)
            {
                int poemindex = rnd.Next(0, PoemFiles.Count - 1);
                result = PoemFiles[poemindex];
            }
            return result;
        }       
        private void PrintPoemFile()
            //Prepares the poem for printing
        {            
             try
                {
                    //Set the fonts for the title, author and body of the poem
                    printFont = new Font(DefaultFontName, 10);
                    titleFont = new Font(DefaultFontName, 12, FontStyle.Bold);
                    bylineFont = new Font(DefaultFontName, 10, FontStyle.Italic);
                    PrintDocument pd = new PrintDocument();
                    if (SelectedPrinter != "default")
                        pd.PrinterSettings.PrinterName = SelectedPrinter;
                    //Create an event handler to build the contents of the printout
                    pd.PrintPage += new PrintPageEventHandler (this.pd_PrintPage);
                    //Create a new papersize to handle the poem, and set the margins
                    pd.DefaultPageSettings.PaperSize = new PaperSize("PoemSize", PaperWidth, PaperLength);
                    pd.DefaultPageSettings.Margins = new Margins(LeftMargin, RightMargin, TopMargin, BottomMargin);
                    //Send to the printer
                    pd.Print();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error printing:" + e.Message);
                }
        }
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
            //Creates the content of the printout
        {
            float topMargin = ev.MarginBounds.Top;
            //Set number of blank lines after the poem body
            int TrailingLinefeeds = 3;
            Font currentFont = printFont;
            //Tracks how many vertical pixels of space are left on the page
            float remainingPageSpace = ev.MarginBounds.Height;
            int logoMaxWidth = 75;
            int logoMaxHeight = 75;
            int logoHeight = 0;
            int logoWidth = 0;

            Image img = null;
            string logofile = PoemPath + @"\" + LogoFile;
            //Load the logo file and scale it proportionally to fit on the page
            if (File.Exists(logofile))
            {
                img = Image.FromFile(logofile);
                logoWidth = img.Width;
                logoHeight = img.Height;
                if (logoWidth > logoMaxWidth)
                {
                    logoWidth = logoMaxWidth;
                    int scaleFactor = (int)(logoWidth / logoMaxWidth);
                    logoHeight = (int)(logoHeight / scaleFactor);
                }
                if (logoHeight > logoMaxHeight)
                {
                    logoHeight = logoMaxHeight;
                    int scaleFactor = (int)(logoHeight / logoMaxHeight);
                    logoWidth = (int)(logoWidth / scaleFactor);
                }
            }

            //yIncrement tracks how many vertical pixels we've used on the page
            float yIncrement = topMargin;
            SizeF sf;

            //Load the poem and append any text to go after the body
            List<string> PoemText = CurrentPoem.Data();
            for (int i = 0; i < TrailingLinefeeds; i++)
                PoemText.Add(" ");
            PoemText.Add("-------------");
            PoemText.Add(ExitMessage);
            ev.HasMorePages = false;
            //Add each line of the poem to the page
            foreach (string nextline in PoemText)
            {
                string line = nextline;
                currentFont = printFont;
                if (string.IsNullOrEmpty(line))
                    //Ensures that blank lines are retained
                {
                    line = " ";
                }
                if (line.Contains ("<title>"))
                    //Sets title font for the title line
                {
                    currentFont = titleFont;
                    line = line.Replace("<title>", string.Empty);
                }
                if (line.Contains("<author>"))
                    //sets author font for the author line
                {
                    currentFont = bylineFont;
                    line = line.Replace("<author>", string.Empty);
                }

                Graphics gf = ev.Graphics;
                //Measure the textbox for the current line and add it to the page if there is enough space remaining
                sf = gf.MeasureString(line, currentFont, LineWidth);
                remainingPageSpace = remainingPageSpace - sf.Height;
                if (remainingPageSpace >= 0)
                {
                    ev.Graphics.DrawString(line, currentFont, Brushes.Black, new RectangleF(new PointF(4.0F, yIncrement), sf), StringFormat.GenericTypographic);
                    yIncrement += sf.Height;
                }
            }
            if (PrintLogo && img != null)
            {
                remainingPageSpace = remainingPageSpace - img.Height;
                if (File.Exists(logofile) && remainingPageSpace >= 0)
                    //Add in the logo if there is enough remaining space on the page
                {
                    int imageXPos = (int)((ev.PageBounds.Width/2)-logoWidth);
                    ev.Graphics.DrawImage(img, imageXPos,(int)yIncrement,logoWidth,logoHeight);
                }
            }
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
            //Handles key press event
        {
            if (e.KeyChar == ' ')
                //Screens for a spacebar press
            {
                //pick a poem title at random from the list loaded previously
                string PoemFile = PickAFile();
                if (File.Exists(PoemPath + PoemFile))
                {
                    //Log the poem and timestamp it for later analysis
                    PoemLog.Add(DateTime.Now.ToString("g") + "  -  " + PoemFile);
                    //Load and print the file with the matching filename
                    CurrentPoem = new TextFile(PoemPath,PoemFile,false);
                    PrintPoemFile();
                }
            }
        }
        private void RefreshPoems_DoWork(object sender, DoWorkEventArgs e)
            //Background worker scans the poem repository for new files every 60 seconds
        {
            //How often to refresh the poems list, in minutes
            int RefreshInterval = 1;
            int RefreshIntervalInMS = RefreshInterval * 60 * 1000;
            bool KeepChecking = true;
            while (KeepChecking)
            {
                Thread.Sleep(RefreshIntervalInMS);
                //Reload the list of filenames
                GetFileNames();
            }
        }
        private void cb_Printers_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetNewPrinter();
        }
    }

    public class TextFile
    {
        private string _path;
        private string _filename;
        private bool _unsavedupdates;
        private bool _saveonupdate;
        private bool FileNameSet = false;
        private bool FilePathSet = false;
        private bool FileInfoSet = false;
        private bool UnsavedUpdates
        {
            get
            {
                return _unsavedupdates;
            }
            set
            {
                _unsavedupdates = value;
                if (value && SaveOnUpdate)
                {
                    Save();
                }
            }
        }
        private DateTime LastSave;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = AddSlash(value);
                FilePathSet = true;
                FileInfoSet = (FilePathSet && FileNameSet);
            }
        }
        public string FileName
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                FileNameSet = true;
                FileInfoSet = (FilePathSet && FileNameSet);
            }
        }
        private List<string> FileData = new List<string>();
        public bool SaveOnUpdate
        {
            get { return _saveonupdate; }
            set
            {
                _saveonupdate = value;
                if (value && UnsavedUpdates)
                {
                    Save();
                }
            }
        }
        public TextFile(bool saveonupdate = true)
        {
            FileInfoSet = false;
            UnsavedUpdates = false;
            SaveOnUpdate = saveonupdate;
        }
        public TextFile(string path, string filename, bool saveonupdate = true)
        {
            Path = path;
            FileName = filename;
            Load();
            SaveOnUpdate = saveonupdate;
        }
        public TextFile(string path, string filename, List<string> newdata, bool saveonupdate = true)
        {
            Path = path;
            FileName = filename;
            FileData = newdata;
            UnsavedUpdates = true;
            SaveOnUpdate = saveonupdate;
        }
        private string AddSlash(string path)
        {
            if (!path.EndsWith(@"\"))
                path += @"\";
            return path;
        }
        public List<string> Load()
        {
            FileData = new List<string>();
            if (!File.Exists(Path + FileName) && FileInfoSet)
            {
                UnsavedUpdates = true;
                Save();
            }
            if (FileInfoSet)
            {
                System.IO.StreamReader file = new StreamReader(Path + FileName);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    FileData.Add(line);
                }
                file.Close();
                UnsavedUpdates = false;
            }
            return FileData;
        }
        public List<string> Load(string path, string filename)
        {
            Path = path;
            FileName = filename;
            return Load();
        }
        public void Save()
        {
            if (FileInfoSet && UnsavedUpdates)
            {
                if (File.Exists(Path + FileName))
                    File.Delete(Path + FileName);
                StreamWriter file = new StreamWriter(Path + FileName);
                foreach (string line in FileData)
                {
                    file.WriteLine(line);
                }
                file.Close();
                LastSave = DateTime.Now;
                UnsavedUpdates = false;
            }
        }
        public void Save(List<string> newdata)
        {
            FileData = newdata;
            UnsavedUpdates = true;
            Save();
        }
        public void Save(string newfilepath, string newfilename)
        {
            Path = newfilepath;
            FileName = newfilename;
            UnsavedUpdates = true;
            Save();
        }
        public void Save(string newfilepath, string newfilename, List<string> newdata)
        {
            Path = newfilepath;
            FileName = newfilename;
            FileData = newdata;
            UnsavedUpdates = true;
            Save();
        }
        public void Add(string line)
        {
            FileData.Add(line);
            UnsavedUpdates = true;
        }
        public void UpdateLine(int index, string newdata)
        {
            if (index >= 0 && index < FileData.Count)
            {
                FileData[index] = newdata;
                UnsavedUpdates = true;
            }
        }
        public void UpdateLine(string olddata, string newdata)
        {
            UpdateLine(FileData.IndexOf(olddata), newdata);
        }
        public void DeleteLine(int index)
        {
            if (index >= 0 && index < FileData.Count)
            {
                FileData.RemoveAt(index);
                UnsavedUpdates = true;
            }
        }
        public void DeleteLine(string targetline)
        {
            DeleteLine(FileData.IndexOf(targetline));
        }
        public void InsertLine(string newline, int index)
        {
            if (index < 0)
                index = 0;
            if (index > FileData.Count)
            {
                Add(newline);
            }
            else
            {
                FileData.Insert(index, newline);
            }
            UnsavedUpdates = true;
        }
        public void InsertLine(string newline, string existingline, string where = "before")
        {
            int index = FileData.IndexOf(existingline);
            if (where == "after")
                index++;
            InsertLine(newline, index);
        }
        public void ToConsole()
        {
            Console.WriteLine("Contents of file " + Path + FileName + ":");
            foreach (string line in FileData)
                Console.WriteLine("   " + line);
        }
        public List<string> Data()
        {
            return FileData;
        }
        public bool Unsaved()
        {
            return UnsavedUpdates;
        }
        public DateTime LastSaved()
        {
            return LastSave;
        }
    }
}
