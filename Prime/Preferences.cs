using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;

namespace Prime
{
    /// <summary>
    /// Database of settings serialised as XML and accessible as a singleton
    /// </summary>
    /// <remarks></remarks>
    [XmlRoot("Preferences")]
    public class Preferences
    {

        #region "Singleton Access"

        private static Preferences instance_;
        public static Preferences Instance
        {
            get
            {
                // public access to self contained instance
                if (instance_ == null)
                {
                    instance_ = new Preferences();
                    instance_.init();
                }
                return instance_;
            }
            // only this class should ever set the value
            private set { instance_ = value; }
        }

        #endregion

        #region "All Settings"

        #region "Defaults"

        private void loadDefaults()
        {
            HomeFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            System.Drawing.Rectangle screen = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            WindowWidth = screen.Width * 0.7;
            WindowHeight = screen.Height * 0.7;
            WindowPositionX = screen.Width / 2 - WindowWidth / 2;
            WindowPositionY = screen.Height / 2 - WindowHeight / 2;
        }

        #endregion

        // home folder
        [XmlElement(Type = typeof(string))]
        public string HomeFolderPath { get; set; }

        // window position x
        [XmlElement(Type = typeof(double))]
        public double WindowPositionX { get; set; }

        // window position y
        [XmlElement(Type = typeof(double))]
        public double WindowPositionY { get; set; }

        // window size y
        [XmlElement(Type = typeof(double))]
        public double WindowWidth { get; set; }

        // window size x
        [XmlElement(Type = typeof(double))]
        public double WindowHeight { get; set; }

        #endregion

        #region "Serialisation"

        // components of settings file's path
        private readonly string[] settingsPath = {
		    "Jaywick Labs",
		    "Prime",
		    "preferences.xml"
	    };

        // path to file (created when settingsPath array is compiled into a path)
        private string filePath;

        // specifies if settings file has been read yet
        private static bool isRead = false;

        private Preferences()
        {
            loadDefaults();
            checkSettingsFile();
        }

        /// <summary>
        /// Private initialiser to be used only once (otherwise serialisation will call constructor again causing a stackoverflow)
        /// </summary>
        /// <remarks></remarks>
        private void init()
        {
            if (System.IO.File.Exists(filePath))
                read();
        }

        /// <summary>
        /// Save settings on finalisation
        /// </summary>
        /// <remarks>This is called when the GC chooses to destroy the instance, which should upon expected application exit</remarks>
        ~Preferences()
        {
            write();
        }

        /// <summary>
        /// Write to the settings file
        /// </summary>
        /// <remarks></remarks>
        public void write()
        {
            StreamWriter writer = new StreamWriter(filePath);
            XmlSerializer s = new XmlSerializer(typeof(Preferences));
            s.Serialize(writer, Instance);
            writer.Close();
        }

        /// <summary>
        /// Read from the settings from settings file
        /// </summary>
        /// <remarks></remarks>
        public void read()
        {
            // only read once for single instance
            if (isRead)
                return;
            else
                isRead = true;

            StreamReader reader = new StreamReader(filePath);
            XmlSerializer serialiser = new XmlSerializer(typeof(Preferences));

            try
            {
                Instance = (Preferences)serialiser.Deserialize(reader);
            }
            catch (InvalidOperationException ex)
            {
                reader.Close();

                var result = System.Windows.MessageBox.Show("Loading preferences failed. " + ex.Message + Environment.NewLine
                                                            + "Fix the settings file and press OK or Cancel to overwrite with defaults",
                                                            "Settings Error",
                                                            System.Windows.MessageBoxButton.OKCancel,
                                                            System.Windows.MessageBoxImage.Exclamation);

                if (result == System.Windows.MessageBoxResult.OK)
                {
                    isRead = false;
                    read();
                    return;
                }
            }

            reader.Close();
        }

        /// <summary>
        /// Check for settings file and create the folder hierarchy if missing
        /// </summary>
        /// <remarks>In future, take into account portable versions of this software (no appdata)</remarks>
        public void checkSettingsFile()
        {
            var appData = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            var labsFolder = Path.Combine(appData, settingsPath[0]);
            if (!System.IO.Directory.Exists(labsFolder))
                System.IO.Directory.CreateDirectory(labsFolder);

            var kalqFolder = Path.Combine(labsFolder, settingsPath[1]);
            if (!System.IO.Directory.Exists(kalqFolder))
                System.IO.Directory.CreateDirectory(kalqFolder);

            filePath = Path.Combine(kalqFolder, settingsPath[2]);
        }

        #endregion

    }

}
