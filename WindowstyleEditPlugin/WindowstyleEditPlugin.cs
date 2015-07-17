﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sphere.Plugins;
using System.IO;

using Sphere.Core.Editor;

namespace SphereStudio.Plugins
{
    public class WindowstyleEditPlugin : IEditorPlugin
    {
        public string Name { get { return "Windowstyle Editor"; } }
        public string Author { get { return "Radnen"; } }
        public string Description { get { return "Sphere Studio default windowstyle editor"; } }
        public string Version { get { return "1.2.0"; } }
        public Icon Icon { get; set; }

        private readonly string[] _extensions = new[] { "rws" };
        private const string _openFileFilters = "*.rws";

        public WindowstyleEditPlugin()
        {
            Icon = Icon.FromHandle(Properties.Resources.GridToolIcon.GetHicon());
        }

        public void Initialize(ISettings conf)
        {
            PluginManager.RegisterExtensions(this, _extensions);
            PluginManager.IDE.RegisterNewHandler(this, "Windowstyle");
            PluginManager.IDE.RegisterOpenFileType("Sphere Windowstyles", _openFileFilters);
        }

        public void Destroy()
        {
            PluginManager.UnregisterExtensions(_extensions);
            PluginManager.IDE.UnregisterNewHandler(this);
            PluginManager.IDE.UnregisterOpenFileType(_openFileFilters);
        }

        public DocumentView CreateEditView() { return null; }

        public DocumentView NewDocument()
        {
            return null;
        }
        
        public DocumentView OpenDocument(string filepath)
        {
            WindowstyleEditView view = new WindowstyleEditView();
            view.Load(filepath);
            return view;
        }
   }
}
