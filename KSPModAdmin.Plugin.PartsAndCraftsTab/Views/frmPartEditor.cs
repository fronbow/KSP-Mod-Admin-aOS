﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using KSPModAdmin.Core.Views;

namespace KSPModAdmin.Plugin.PartsAndCraftsTab.Views
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public partial class frmPartEditor : frmBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Name .
        /// </summary>
        public string PartName { get { return partName; } set { tbNewName.Text = partName = value; } }
        private string partName = string.Empty;

        /// <summary>
        /// Gets the tbNewName Text property.
        /// </summary>
        public string NewName { get { return tbNewName.Text; } }

        /// <summary>
        /// Gets or sets the tbNewTitle Text property.
        /// </summary>
        public string Title { get { return title; } set { tbNewTitle.Text = title = value; } }
        private string title = string.Empty;

        /// <summary>
        /// Gets the tbNewTitle Text property.
        /// </summary>
        public string NewTitle { get { return tbNewTitle.Text; } }

        /// <summary>
        /// Gets or sets the selected part category.
        /// </summary>
        public string Category
        {
            get { return category; }
            set
            {
                foreach (string item in cbCategory.Items)
                {
                    if (item == value)
                    {
                        cbCategory.SelectedItem = item;
                        category = item;
                    }
                }
            }
        }
        private string category = string.Empty;

        /// <summary>
        /// Gets the selected part category.
        /// </summary>
        public string NewCategory
        {
            get { return (string)cbCategory.SelectedItem; }
        }

        /// <summary>
        /// Gets or sets the Description Text.
        /// </summary>
        public string Description { get { return lblNameSelectionDescription.Text; } set { lblNameSelectionDescription.Text = value; } }

        /// <summary>
        /// Gets or sets the known (forbidden) names.
        /// </summary>
        public List<string> KnownNames { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the frmNameSelection class.
        /// </summary>
        public frmPartEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handling

        /// <summary>
        /// Handles the Load event of the frmNameSelection
        /// </summary>
        private void frmNameSelection_Load(object sender, EventArgs e)
        {
            tbNewName.Select();
            tbNewName.SelectAll();
            tbNewName.Focus();

            if (cbCategory.SelectedIndex < 0)
                cbCategory.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the Click event of the btnOK
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbNewName.Text.Length == 0)
                MessageBox.Show(this, "Please enter a new name first.");

            else if (NewName != PartName && KnownNames.Contains(NewName))
                MessageBox.Show(this, "Name already exists!");

            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the KeyDown event of the tbNewName
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOK_Click(null, null);
        }

        #endregion
    }
}
