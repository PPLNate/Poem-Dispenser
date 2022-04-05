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

namespace Add_Poemz
{
    public partial class AddPoemz : Form
    {
        public AddPoemz()
        {
            InitializeComponent();
            LoadSettings();
        }

        string PoemPath = @"p:\";
        string FileExtension = "*.txt";

        private void LoadSettings()
        {
            string appPath = AppContext.BaseDirectory;
            if (File.Exists(appPath + "Poemzconfig.ppl"))
            {
                TextFile configfile = new TextFile(appPath, "Poemzconfig.ppl", false);
                foreach (string config in configfile.Data())
                {
                    string[] datainfo = config.Split('>');
                    string field = datainfo[0];
                    string value = string.Empty;
                    if (datainfo.Length > 1)
                        value = datainfo[1];
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                    {
                        switch (field)
                        {
                            case "<poemdirectory":
                                PoemPath = value;
                                break;
                            case "<poemfileextension":
                                FileExtension = value;
                                break;
                        }
                    }
                }
            }
        }
        private void Status (string message)
        {
            lbl_Status.Text = message;
            Console.WriteLine(message);
        }
        private bool ValidateInput ()
        {
            bool result = (
                    tb_Title.Text.Length > 0 &&
                    tb_Author.Text.Length > 0 &&
                    tb_Poem.Text.Length > 0
                    );
            if (!result)
                Status("All fields must be filled in before saving poem");
            return result;

        }
        private void ResetForm ()
        {
            tb_Title.Clear();
            tb_Author.Clear();
            tb_Poem.Clear();
        }
        private void SavePoem ()
        {
            int iteration = 1;
            FileExtension = FileExtension.Replace("*", string.Empty);
            Status("Validating poem...");
            if (ValidateInput())
            {
                Status("Accepted...");
                string author = tb_Author.Text;
                string title = tb_Title.Text;
                string poem = tb_Poem.Text;
                string filename = author.Replace(" ", string.Empty) + "-" + title.Replace(" ", string.Empty);
                if (filename.Length > 32)
                    filename = filename.Substring(0, 32);
                filename += FileExtension;

                Status("Compiling poem info...");
                string[] pl = poem.Split('\n');
                List<string> poemlines = pl.ToList();
                for (int i = 0; i < poemlines.Count; i++)
                    poemlines[i] = poemlines[i].Trim();

                poemlines.Insert(0, "----------------------");
                poemlines.Insert(0, "<author>By " + author);
                poemlines.Insert(0, "<title>" + title);

                string tryfilename = filename;
                while (File.Exists(PoemPath + @"\" + tryfilename))
                {
                    tryfilename = filename.Replace(FileExtension, string.Empty);
                    tryfilename += "_" + iteration.ToString() + FileExtension;
                    iteration++;
                }
                filename = tryfilename;

                Status("Saving poem file as " + filename);
                TextFile newpoem = new TextFile(PoemPath, filename,poemlines, true);
                newpoem.Save();

                Status("Poem has been saves as " + filename);

                ResetForm();
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            SavePoem();
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
