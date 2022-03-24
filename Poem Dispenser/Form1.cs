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
            ConnectToPoemsShare();
            GetFileNames();
            RefreshPoems.RunWorkerAsync();
        }

        List<string> PoemFiles;
        const string PoemPath = @"p:\";
        const string FileExtension = "*.txt";
        Random rnd = new Random();
        TextFile PoemLog = new TextFile(@"c:\all", "poemlog.log");

        private Font printFont;
        private StreamReader streamToPrint;

        private void GetFileNames ()
        {
            string[] files = Directory.GetFiles(PoemPath,FileExtension);
            PoemFiles = new List<string>();
            foreach (string file in files)
            {
                string justfilename = Path.GetFileName(file);
                PoemFiles.Add(justfilename);
                Console.WriteLine(justfilename);
            }
        }
        private string PickAFile ()
        {
            string result = string.Empty;

            if (PoemFiles.Count > 0)
            {
                int poemindex = rnd.Next(0, PoemFiles.Count - 1);
                result = PoemFiles[poemindex];
            }
            return result;
        }       
        private void ConnectToPoemsShare ()
        {
            if (!Directory.Exists("p:"))
            {
                Process proc = new Process();
                ProcessStartInfo ps = new ProcessStartInfo();
                ps.UseShellExecute = false;
                ps.CreateNoWindow = true;
                ps.RedirectStandardOutput = true;
                ps.FileName = "cmd.exe";
                ps.Arguments = @"/c net use p: \\10.92.90.45\poems$ /user:ppl\pdispenser hLJqMX+idT";
                proc.StartInfo = ps;
                proc.Start();
                proc.WaitForExit();
            }
        }
        private void PrintPoemFile(string filename)
        {            
            try
            {
                streamToPrint = new StreamReader (filename);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler (this.pd_PrintPage);
                    pd.DefaultPageSettings.PaperSize = new PaperSize("PoemSize", 300, 1000);
                    pd.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
                    pd.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;
            bool LineIsNull = false;

            float yIncrement = topMargin;

            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            SizeF sf;
            while (count < linesPerPage)// && ((line = streamToPrint.ReadLine()) != null))
            {
                line = streamToPrint.ReadLine();
                //If line is blank, add a space to keep formatting intact
                if (string.IsNullOrEmpty(line))
                {
                    LineIsNull = (line==null);
                    line = " ";
                }
                Graphics gf = ev.Graphics;
                sf = gf.MeasureString(line, printFont, 300);
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black, new RectangleF(new PointF(4.0F,yIncrement),sf),StringFormat.GenericTypographic);
                yIncrement += sf.Height;
                count++;
            }

            if (!LineIsNull)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                string PoemFile = PoemPath + PickAFile();
                if (File.Exists(PoemFile))
                {
                    Console.WriteLine("  Poem Selected: " + PoemFile);
                    PoemLog.Add(DateTime.Now.ToString("g") + "  -  " + PoemFile);
                    PrintPoemFile(PoemFile);
                }
            }
        }
        private void RefreshPoems_DoWork(object sender, DoWorkEventArgs e)
        {
            //How often to refresh the poems list, in minutes
            int RefreshInterval = 1;
            int RefreshIntervalInMS = RefreshInterval * 60 * 1000;
            bool KeepChecking = true;
            while (KeepChecking)
            {
                Thread.Sleep(RefreshIntervalInMS);
                GetFileNames();
            }
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
            //Console.WriteLine("Attempting to save file...");
            //Console.WriteLine("   FileInfoSet = " + FileInfoSet.ToString());
            //Console.WriteLine("   UnsavedUpdates = " + UnsavedUpdates.ToString());
            if (FileInfoSet && UnsavedUpdates)
            {
                //FileStream file = File.Open(Path + FileName, FileMode.Create);
                //StreamWriter filedata = new StreamWriter(file);
                if (File.Exists(Path + FileName))
                    File.Delete(Path + FileName);
                StreamWriter file = new StreamWriter(Path + FileName);
                foreach (string line in FileData)
                {
                    //Console.WriteLine("    Saving: " + line);
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
