﻿using NPCSManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NPCSGui {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        NPCSManager.NPCSManager Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            byte[] Script = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
            Editor = new NPCSManager.NPCSManager(Script);
            listBox1.Items.Clear();
            string[] strs = Editor.Import();
            foreach (string str in strs)
                listBox1.Items.Add(str);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {
            string[] Arr = new string[listBox1.Items.Count];
            listBox1.Items.CopyTo(Arr, 0);
            System.IO.File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Arr));
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                Text = string.Format("ID: {0}/{1}", listBox1.Items.Count, listBox1.Items.Count);
                textBox1.Text = listBox1.Items[i].ToString(); 
            }
            catch { }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog FD = new OpenFileDialog();
            FD.Filter = "All MPK Files |*.mpk|Vita/Steam CPK Files |*.cpk";
            if (FD.ShowDialog() == DialogResult.OK) {
                MPKManager Reader = new MPKManager(new StreamReader(FD.FileName).BaseStream);
                NPCSManager.File[] Files = Reader.Open();
                string Dir = FD.FileName + "~\\";
                if (!System.IO.Directory.Exists(Dir))
                    System.IO.Directory.CreateDirectory(Dir);

                foreach (NPCSManager.File F in Files) {
                    string Name = Dir + F.Path;
                    if (System.IO.File.Exists(Name))
                        System.IO.File.Delete(Name);
                    Stream Out = new StreamWriter(Name).BaseStream;
                    int c = 0;
                    byte[] Buffer = new byte[(1024 * 1024) * 5];
                    do {
                        c = F.Content.Read(Buffer, 0, Buffer.Length);
                        Out.Write(Buffer, 0, c);
                    } while (c > 0);
                    Out.Close();
                }
            }
        }

        private void repackToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog FD = new OpenFileDialog();
            FD.Filter = "All MPK Files|*.mpk|Vita/Steam CPK Files |*.cpk";
            if (FD.ShowDialog() == DialogResult.OK) {
                MPKManager Writer = new MPKManager(new StreamReader(FD.FileName).BaseStream);
                string Dir = FD.FileName + "~\\";
                List<NPCSManager.File> Files = new List<NPCSManager.File>();
                foreach (string File in Directory.GetFiles(Dir))
                    Files.Add(new NPCSManager.File() {
                        Path = Path.GetFileName(File),
                        Content = new StreamReader(File).BaseStream
                    });
                Writer.InjectFiles(Files.ToArray(), new StreamWriter(FD.FileName + "-patched.mpk").BaseStream);
                foreach (NPCSManager.File f in Files)
                    f.Content.Close();

            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                } catch {

                }
            }
        }
    }
}
