﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using KSPModAdmin.Core.Controller;
using KSPModAdmin.Core.Model;

namespace KSPModAdmin.Core.Views
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public partial class frmMain : frmBase
    {
        /// <summary>
        /// Gets or sets the known KSP install paths.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<NoteNode> KnownKSPPaths
        {
            get { return (from e in cbKSPPath.Items.Cast<string>() select new NoteNode(e, e, string.Empty)).ToList(); }
            set
            {
                cbKSPPath.Items.Clear();
                if (value != null)
                {
                    cbKSPPath.SelectedIndexChanged -= cbKSPPath_SelectedIndexChanged;
                    foreach (NoteNode path in value)
                        cbKSPPath.Items.Add(path.Name);
                    cbKSPPath.SelectedIndexChanged += cbKSPPath_SelectedIndexChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected KSP path.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedKSPPath
        {
            get
            {
                return (cbKSPPath.Items.Count > 0) ? cbKSPPath.SelectedItem.ToString() : string.Empty;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    cbKSPPath.SelectedItem = null;
                
                if (cbKSPPath.Items.Count <= 0)
                    return;

                foreach (string path in cbKSPPath.Items)
                {
                    if (value != path)
                        continue;

                    cbKSPPath.SelectedItem = path;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the MainTab control.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabControl TabControl { get { return tabControl1; } }

        /// <summary>
        /// Gets the KSPStartup user control.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ucKSPStartup UcKSPStartup { get { return ucKSPStartup1; } }

        /// <summary>
        /// Gets or sets the TabPage order by the unique identifier of the TabViews.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<string> TapOrder
        {
            get
            {
                var tabOrder = new List<string>();
                foreach (TabPage tab in tabControl1.TabPages)
                    if (tab.Tag != null)
                        tabOrder.Add(tab.Tag.ToString());

                return tabOrder;
            }
            set
            {
                if (value == null)
                    return;

                // get current TabPages
                var oldTabPages = new List<TabPage>();
                foreach (TabPage tab in tabControl1.TabPages)
                    oldTabPages.Add(tab);
                
                tabControl1.TabPages.Clear();
                
                // Add TabPages in defined order.
                foreach (string id in value)
                {
                    var tab = oldTabPages.FirstOrDefault(x => x.Tag != null && x.Tag.ToString() == id);
                    if (tab == null)
                        continue;
                    tabControl1.TabPages.Add(tab);
                    oldTabPages.Remove(tab);
                }

                // Add remaining TabPages.
                foreach (TabPage remainingTab in oldTabPages)
                    tabControl1.TabPages.Add(remainingTab);
            }
        }


        /// <summary>
        /// Creates a new instance of the frmMain class.
        /// </summary>
        public frmMain()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
                return;

            tabPageModSelection.Tag = new Guid("{F6DC12F3-BAA4-47F7-ADE4-46DF57C8ECFA}");
            tabPageOptions.Tag = new Guid("{66D27BD4-3A31-45D5-AB80-1DF61FDB48E5}");
        }


        /// <summary>
        /// Handles the Load event of the form.
        /// </summary>
        private void frmMain_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event handler for the FormClosing from frmMain.
        /// </summary>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainController.ShutDown(false);
        }

        /// <summary>
        /// Event handler for the SelectedIndexChanged from cbKSPPath.
        /// </summary>
        private void cbKSPPath_SelectedIndexChanged(object sender, EventArgs e)
        {
            OptionsController.SelectedKSPPath = cbKSPPath.SelectedItem.ToString();
        }

        /// <summary>
        /// Sets the selected KSP path without raising event SelectedIndexChanged.
        /// </summary>
        /// <param name="kspPath">The new selected KSP path.</param>
        internal void SilentSetSelectedKSPPath(string kspPath)
        {
            cbKSPPath.SelectedIndexChanged -= cbKSPPath_SelectedIndexChanged;
            SelectedKSPPath = null;
            SelectedKSPPath = kspPath;
            cbKSPPath.SelectedIndexChanged += cbKSPPath_SelectedIndexChanged;
        }
    }
}
