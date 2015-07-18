﻿using ScintillaNET;
using Sphere.Core.Editor;
using Sphere.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SphereStudio.Plugins
{
    partial class ScriptEditView : ScriptView
    {
        private Scintilla _codeBox = new Scintilla();

        // We should technically be using ISO-8859-1 or Windows-1252 for compatibility with the old editor.
        // However, UTF-8 works fine in Sphere and some JS engines (e.g. Duktape) won't accept
        // 8-bit encodings if they contain extended characters, so we'll use UTF-8 and compromise
        // by not including a byte order mark.
        private readonly Encoding UTF_8_NOBOM = new UTF8Encoding(false);
        private readonly Encoding ISO_8859_1 = Encoding.GetEncoding("iso-8859-1");

        private bool _autocomplete;

        public ScriptEditView()
        {
            Icon = Icon.FromHandle(Properties.Resources.script_edit.GetHicon());

            string configPath = Application.StartupPath + "\\SphereLexer.xml";
            if (File.Exists(configPath))
                _codeBox.ConfigurationManager.CustomLocation = configPath;

            _codeBox.Encoding = Encoding.UTF8;
            _codeBox.ConfigurationManager.Language = "js";
            _codeBox.AutoComplete.SingleLineAccept = false;
            _codeBox.AutoComplete.FillUpCharacters = "";
            _codeBox.AutoComplete.StopCharacters = "(";
            _codeBox.AutoComplete.ListSeparator = ';';
            _codeBox.AutoComplete.IsCaseSensitive = false;
            _codeBox.SupressControlCharacters = true;

            _codeBox.Folding.MarkerScheme = FoldMarkerScheme.Custom;
            _codeBox.Folding.Flags = FoldFlag.LineAfterContracted;
            _codeBox.Folding.UseCompactFolding = false;
            _codeBox.Margins.Margin1.IsClickable = true;
            _codeBox.Margins.Margin1.IsFoldMargin = true;
            _codeBox.Styles.LineNumber.BackColor = Color.FromArgb(235, 235, 255);
            _codeBox.Margins.FoldMarginColor = Color.FromArgb(235, 235, 255);

            _codeBox.Indentation.SmartIndentType = SmartIndent.CPP;
            _codeBox.Styles.BraceLight.ForeColor = Color.Black;
            _codeBox.Styles.BraceLight.BackColor = Color.LightGray;

            _codeBox.Caret.CurrentLineBackgroundColor = Color.LightGoldenrodYellow;

            _codeBox.CharAdded += codeBox_CharAdded;
            _codeBox.ModifiedChanged += codeBox_ModifiedChanged;
            _codeBox.TextDeleted += codeBox_TextChanged;
            _codeBox.TextInserted += codeBox_TextChanged;
            _codeBox.Dock = DockStyle.Fill;

            Controls.Add(_codeBox);
            Restyle();
        }

        public override string[] FileExtensions
        {
            get { return new[] { "js", "coffee" }; }
        }

        public override string Text
        {
            get
            {
                return _codeBox.Text;
            }
            set
            {
                _codeBox.Text = value;
                _codeBox.UndoRedo.EmptyUndoBuffer();
            }
        }

        public override string ViewState
        {
            get
            {
                return string.Format("{0}|{1}|{2}",
                    _codeBox.Caret.Position,
                    _codeBox.Caret.Anchor,
                    _codeBox.Lines.FirstVisibleIndex);
            }
            set
            {
                string[] parse = value.Split('|');
                _codeBox.Caret.Position = Convert.ToInt32(parse[0]);
                _codeBox.Caret.Anchor = Convert.ToInt32(parse[1]);
                _codeBox.Lines.FirstVisibleIndex = Convert.ToInt32(parse[2]);
            }
        }

        public override bool NewDocument()
        {
            _codeBox.Text = "";
            if (PluginManager.IDE.Settings.GetBoolean("autoScriptHeader", false))
            {
                string author = (PluginManager.IDE.CurrentGame != null) ? PluginManager.IDE.CurrentGame.Author : "Unnamed";
                const string header = "/**\n* Script: Untitled.js\n* Written by: {0}\n* Updated: {1}\n**/";
                _codeBox.Text = string.Format(header, author, DateTime.Today.ToShortDateString());
            }
            _codeBox.UndoRedo.EmptyUndoBuffer();
            _codeBox.Modified = false;
            return true;
        }

        public override void Load(string filename)
        {
            using (StreamReader fileReader = new StreamReader(File.OpenRead(filename), true))
            {
                if (Path.GetExtension(filename) != ".js")
                {
                    _codeBox.ConfigurationManager.Language = "default";
                }
                
                _codeBox.Text = fileReader.ReadToEnd();
                _codeBox.UndoRedo.EmptyUndoBuffer();
                _codeBox.Modified = false;
                
                SetMarginSize(_codeBox.Styles[StylesCommon.LineNumber].Font);
            }
        }

        public override void Save(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename, false, UTF_8_NOBOM))
            {
                if (PluginManager.IDE.Settings.GetBoolean("autoScriptUpdate", false))
                {
                    _codeBox.UndoRedo.IsUndoEnabled = false;
                    if (_codeBox.Lines.Count > 1 && _codeBox.Lines[1].Text[0] == '*')
                        _codeBox.Lines[1].Text = "* Script: " + Path.GetFileName(filename);
                    if (_codeBox.Lines.Count > 2 && _codeBox.Lines[2].Text[0] == '*')
                        _codeBox.Lines[2].Text = "* Written by: " + PluginManager.IDE.CurrentGame.Author;
                    if (_codeBox.Lines.Count > 3 && _codeBox.Lines[3].Text[0] == '*')
                        _codeBox.Lines[3].Text = "* Updated: " + DateTime.Today.ToShortDateString();
                    _codeBox.UndoRedo.IsUndoEnabled = true;
                }

                writer.Write(_codeBox.Text);
            }
            _codeBox.Modified = false;
        }

        public override void Restyle()
        {
            _codeBox.Indentation.TabWidth = PluginManager.IDE.Settings.GetInteger("script-spaces", 2);
            _codeBox.Indentation.UseTabs = PluginManager.IDE.Settings.GetBoolean("script-tabs", true);
            _codeBox.Caret.HighlightCurrentLine = PluginManager.IDE.Settings.GetBoolean("script-hiline", true);
            _codeBox.IsBraceMatching = PluginManager.IDE.Settings.GetBoolean("script-hibraces", true);

            _autocomplete = PluginManager.IDE.Settings.GetBoolean("script-autocomplete", true);

            bool fold = PluginManager.IDE.Settings.GetBoolean("script-fold", true);
            _codeBox.Margins.Margin1.Width = fold ? 16 : 0;

            /*string fontstring = PluginManager.IDE.EditorSettings.GetString("script-font");
            if (!String.IsNullOrEmpty(fontstring))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
                SetFont((Font)converter.ConvertFromString(fontstring));
            }*/
        }

        public override void Activate()
        {
            ScriptEditPlugin.ShowMenus(true);
        }

        public override void Deactivate()
        {
            ScriptEditPlugin.ShowMenus(false);
        }

        public override void Cut()
        {
            if (_codeBox.Clipboard.CanCut)
                _codeBox.Clipboard.Cut();
        }

        public override void Copy()
        {
            if (_codeBox.Clipboard.CanCopy)
                _codeBox.Clipboard.Copy();
        }

        public override void Paste()
        {
            if (_codeBox.Clipboard.CanPaste)
                _codeBox.Clipboard.Paste();
        }

        public override void Undo()
        {
            if (_codeBox.UndoRedo.CanUndo)
            {
                _codeBox.UndoRedo.Undo();
            }
        }

        public override void Redo()
        {
            if (_codeBox.UndoRedo.CanRedo)
            {
                _codeBox.UndoRedo.Redo();
            }
        }

        public override void ZoomIn()
        {
            _codeBox.ZoomIn();
        }

        public override void ZoomOut()
        {
            _codeBox.ZoomOut();
        }

        private void SetMarginSize(Font font)
        {
            int spaces = (int)Math.Log10(_codeBox.Lines.Count) + 1;
            _codeBox.Margins[0].Width = 2 + spaces * (int)font.SizeInPoints;
        }

        private void codeBox_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (!_autocomplete) return;

            if (char.IsLetter(e.Ch))
            {
                string word = _codeBox.GetWordFromPosition(_codeBox.CurrentPos).ToLower();
                List<string> filter = (from s in ScriptEditPlugin.Functions where s.ToLower().Contains(word) select s.Replace(";", "")).ToList();

                if (filter.Count != 0)
                {
                    _codeBox.AutoComplete.List = filter;
                    _codeBox.AutoComplete.Show(word.Length);
                }
            }
        }

        private void codeBox_ModifiedChanged(object sender, EventArgs e)
        {
            IsDirty = _codeBox.Modified;
        }

        private void codeBox_TextChanged(object sender, EventArgs e)
        {
            SetMarginSize(_codeBox.Styles[StylesCommon.LineNumber].Font);
        }
    }
}