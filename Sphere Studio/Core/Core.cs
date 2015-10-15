﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;

using SphereStudio.Forms;
using Sphere.Core;
using Sphere.Core.Editor;
using Sphere.Plugins;
using Sphere.Plugins.Interfaces;

namespace SphereStudio
{
    static class Core
    {
        static Core()
        {
            string sphereDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Sphere Studio");
            string iniPath = Path.Combine(sphereDir, "Settings", "Sphere Studio.ini");
            MainIniFile = new IniFile(iniPath);
            Settings = new CoreSettings(Core.MainIniFile);

            // load plugin modules (user-installed plugins first)
            Plugins = new Dictionary<string, PluginShim>();
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] paths =
            {
                Path.Combine(sphereDir, "Plugins"),
                Path.Combine(programDataPath, "Sphere Studio", "Plugins"),
                Path.Combine(Application.StartupPath, "Plugins"),
            };
            foreach (string path in from path in paths
                where Directory.Exists(path)
                select path)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (FileInfo file in dir.GetFiles("*.dll"))
                {
                    string handle = Path.GetFileNameWithoutExtension(file.Name);
                    if (!Plugins.Keys.Contains(handle))  // only the first by that name is used
                        Plugins[handle] = new PluginShim(file.FullName, handle);
                }
            }
        }

        /// <summary>
        /// Grants access to the main .ini file (Sphere Studio.ini).
        /// </summary>
        public static IniFile MainIniFile { get; private set; }

        /// <summary>
        /// Gets or sets the currently loaded project.
        /// </summary>
        public static Project Project { get; set; }

        /// <summary>
        /// Grants access to the Sphere Studio core configuration.
        /// </summary>
        public static CoreSettings Settings { get; private set; }

        /// <summary>
        /// Gets the list of loaded plugins.
        /// </summary>
        public static Dictionary<string, PluginShim> Plugins { get; private set; }

        /// <summary>
        /// Gets the registered name of the IFileOpener handling a specified filename.
        /// </summary>
        /// <param name="fileName">The filename to find a file opener for.</param>
        /// <returns>The registered name of the correct file opener, or null if none was found.</returns>
        public static string GetFileOpenerName(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            if (fileExtension.StartsWith("."))  // remove dot from extension
                fileExtension = fileExtension.Substring(1);

            var names = from name in PluginManager.GetNames<IFileOpener>()
                        let plugin = PluginManager.Get<IFileOpener>(name)
                        where plugin.FileExtensions.Contains(fileExtension)
                        select name;
            return names.FirstOrDefault();
        }
    }

    class CoreSettings : IniSettings, ICoreSettings
    {
        public CoreSettings(IniFile ini) :
            base(ini, "Sphere Studio")
        {
            Preset = GetString("preset", "");
        }

        public bool AutoHideBuild
        {
            get { return GetBoolean("autoHideBuild", false); }
            set { SetValue("autoHideBuild", value); }
        }

        public bool AutoOpenLastProject
        {
            get { return GetBoolean("autoOpenProject", false); }
            set { SetValue("autoOpenProject", value); }
        }

        public bool UseScriptHeaders
        {
            get { return GetBoolean("useScriptHeaders", false); }
            set { SetValue("useScriptHeaders", value); }
        }

        public bool UseStartPage
        {
            get { return GetBoolean("autoStartPage", true); }
            set { SetValue("autoStartPage", value); }
        }

        public string Compiler
        {
            get { return GetString("defaultCompiler", ""); }
            set { Preset = null; SetValue("defaultCompiler", value); }
        }

        public string Engine
        {
            get { return GetString("defaultEngine", ""); }
            set { Preset = null; SetValue("defaultEngine", value); }
        }

        public string FileOpener
        {
            get { return GetString("defaultFileOpener", ""); }
            set { Preset = null; SetValue("defaultFileOpener", value); }
        }

        public string ImageEditor
        {
            get { return GetString("imageEditor", ""); }
            set { Preset = null; SetValue("imageEditor", value); }
        }

        public string ScriptEditor
        {
            get { return GetString("scriptEditor", ""); }
            set { Preset = null; SetValue("scriptEditor", value); }
        }

        public string LastProject
        {
            get { return GetString("lastProject", ""); }
            set { SetValue("lastProject", value); }
        }

        public string[] OffPlugins
        {
            get { return GetStringArray("disabledPlugins"); }
            set { Preset = ""; SetValue("disabledPlugins", value); }
        }

        public string Preset
        {
            get
            {
                string value = GetString("preset", "");
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
            set
            {
                string sphereDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Sphere Studio");
                string path = Path.Combine(sphereDir, @"Presets", value + ".preset");
                if (!string.IsNullOrWhiteSpace(value) && File.Exists(path))
                {
                    using (IniFile preset = new IniFile(path, false))
                    {
                        Compiler = preset.Read("Preset", "compiler", "");
                        Engine = preset.Read("Preset", "engine", "");
                        FileOpener = preset.Read("Preset", "defaultFileOpener", "");
                        ImageEditor = preset.Read("Preset", "imageEditor", "");
                        ScriptEditor = preset.Read("Preset", "scriptEditor", "");
                        OffPlugins = preset.Read("Preset", "disabledPlugins", "").Split('|');
                    }
                    SetValue("preset", value);
                }
                else
                {
                    SetValue("preset", "");
                }
            }
        }

        public string[] ProjectPaths
        {
            get { return GetStringArray("gamePaths"); }
            set { SetValue("gamePaths", value); }
        }

        public View StartPageView
        {
            get
            {
                string val = GetString("startView", "Tile");
                return (View)Enum.Parse(typeof(View), val);
            }
            set
            {
                SetValue("startView", value);
            }
        }

        public string UIStyle
        {
            get { return GetString("uiStyle", "Dark"); }
            set { SetValue("uiStyle", value); }
        }

        public void Apply()
        {
            StyleSettings.CurrentStyle = UIStyle;
            foreach (var plugin in Core.Plugins)
                plugin.Value.Enabled = !OffPlugins.Contains(plugin.Key);
            PluginManager.Core.Docking.Refresh();
        }
    }
}
