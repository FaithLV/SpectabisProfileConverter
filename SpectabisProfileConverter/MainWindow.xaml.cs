using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace SpectabisProfileConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SelectSource.Visibility = Visibility.Visible;
        }

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public string PCSX2BonusFolder;

        //List of game info pairs (title & file path) for PCSX2Bonus
        List<Tuple<string, string>> GameListFromXML = new List<Tuple<string, string>>();

        List<string> GameListFolders = new List<string>();

        private void PCSX2Bonus_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog BonusBrowser = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            BonusBrowser.Description = "Navigate to PCSX2Bonus installation directory!";
            BonusBrowser.UseDescriptionForTitle = true;

            var BonusResult = BonusBrowser.ShowDialog();

            if(BonusResult.Value == false)
            {
                MessageBox.Show("No configuration was converted!");
                Exit();   
            }

            if (File.Exists(BonusBrowser.SelectedPath + @"\PCSX2Bonus.exe") == false)
            {
                MessageBox.Show("Folder doesn't contain PCSX2Bonus.exe!");
                Exit();
            }

            SelectSource.Visibility = Visibility.Collapsed;

            PCSX2BonusFolder = BonusBrowser.SelectedPath + @"\PCSX2Bonus\";

            GetPCSX2BonusGameList();
            GetPCSX2BonusGameFolders();

            PCSX2BonusConvert();

            CopyConverted();

            Exit();
        }

        void GetPCSX2BonusGameList()
        {
            XDocument doc = XDocument.Load(PCSX2BonusFolder + @"\mygames.xml");

            foreach (XElement Game in doc.Descendants("Game"))
            {
                string title = null;
                string isoDir = null;

                foreach (XElement Name in Game.Descendants("Name"))
                {
                    title = (string)Name;

                    title = title.Replace("<Name>", String.Empty);
                    title = title.Replace("</Name>", String.Empty);
                }

                foreach (XElement Location in Game.Descendants("Location"))
                {
                    isoDir = (string)Location;

                    isoDir = isoDir.Replace("<Location>", String.Empty);
                    isoDir = isoDir.Replace("</Location>", String.Empty);
                }

                GameListFromXML.Add(Tuple.Create(title, isoDir));
            }

        }

        void GetPCSX2BonusGameFolders()
        {
            string[] bonusFolders = Directory.GetDirectories(PCSX2BonusFolder + @"\Configs\");

            foreach(var folder in bonusFolders)
            {
                GameListFolders.Add(folder.Replace(PCSX2BonusFolder + @"\Configs\", String.Empty));
            }
        }

        private string PCSX2BonusGetBoxart(string _game)
        {
            if(File.Exists(PCSX2BonusFolder + @"\Images\" + _game + ".jpg"))
            {
                string artFile = PCSX2BonusFolder + @"\Images\" + _game + ".jpg";
                return artFile;
            }
            else
            {
                return null;
            }
        }

        void PCSX2BonusConvert()
        {
            Directory.CreateDirectory(BaseDirectory + @"\configs\");

            foreach(var game in GameListFolders)
            {
                string BonusIniFile = null;

                Directory.CreateDirectory(BaseDirectory + @"\configs\" + game);

                File.Copy(PCSX2BonusGetBoxart(game), BaseDirectory + @"\configs\" + game + @"\art.jpg", true);

                foreach (var file in Directory.GetFiles(PCSX2BonusFolder + @"\configs\" + game))
                {
                    if(file.Contains("PCSX2Bonus.ini"))
                    {
                        BonusIniFile = file;
                    }
                    else
                    {
                        File.Copy(file, BaseDirectory + @"\configs\" + game + @"\" + file.Replace(PCSX2BonusFolder + @"\configs\" + game, String.Empty), true);
                    }
                }

                IniFile SpectabisINI = new IniFile(BaseDirectory + @"\configs\" + game + @"\spectabis.ini");
                IniFile BonusINI = new IniFile(BonusIniFile);

                foreach(var item in GameListFromXML)
                {
                    if(item.Item1 == game)
                    {
                        SpectabisINI.Write("isoDirectory", item.Item2 , "Spectabis");
                    }
                }

                if (BonusINI.Read("NoGUI", "Boot") == "true")
                {
                    SpectabisINI.Write("nogui", "1", "Spectabis");
                }
                else
                {
                    SpectabisINI.Write("nogui", "0", "Spectabis");
                }

                if (BonusINI.Read("FullBoot", "Boot") == "true")
                {
                    SpectabisINI.Write("fullboot", "1", "Spectabis");
                }
                else
                {
                    SpectabisINI.Write("fullboot", "0", "Spectabis");
                }

                if (BonusINI.Read("NoHacks", "Boot") == "true")
                {
                    SpectabisINI.Write("nohacks", "1", "Spectabis");
                }
                else
                {
                    SpectabisINI.Write("nohacks", "0", "Spectabis");
                }

                SpectabisINI.Write("fullscreen", "0", "Spectabis");
                SpectabisINI.Write("fullscreen", "0", "Spectabis");
            }

        }

        private void OldSpectabis_Click(object sender, RoutedEventArgs e)
        {

        }

        void CopyConverted()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog spectabisBrowser = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            spectabisBrowser.Description = "Browse to Spectabis Folder to copy converted profile!";
            spectabisBrowser.UseDescriptionForTitle = true;

            var browserResult = spectabisBrowser.ShowDialog();

            if(browserResult.Value == true)
            {
                foreach(var profile in Directory.GetDirectories(BaseDirectory + @"\configs\"))
                {
                    string spectabisConfigFolder = spectabisBrowser.SelectedPath + @"\resources\configs\" + profile.Replace(BaseDirectory + @"\configs\", String.Empty);

                    if(Directory.Exists(spectabisConfigFolder))
                    {
                        MessageBox.Show(profile.Replace(BaseDirectory + @"\configs\", String.Empty) + " already exists! Skipping.");
                    }
                    else
                    {
                        Directory.CreateDirectory(spectabisConfigFolder);

                        foreach (var file in Directory.GetFiles(profile))
                        {
                            File.Copy(file, spectabisConfigFolder + file.Replace(profile, String.Empty), true);
                        }

                        MessageBox.Show(profile.Replace(BaseDirectory + @"\configs\", String.Empty) + " copied to Spectabis!");
                    }

                }
            }
            else
            {
                MessageBox.Show("No converted profiles were copied!");
                Exit();
            }
        }

        void Exit()
        {
            Application.Current.Shutdown();
        }

    }
}
