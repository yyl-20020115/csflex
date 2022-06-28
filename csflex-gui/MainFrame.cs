/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * C# Flex 1.4                                                             *
 * Copyright (C) 2004-2005  Jonathan Gilbert <logic@deltaq.org>            *
 * Derived from:                                                           *
 *                                                                         *
 *   JFlex 1.4                                                             *
 *   Copyright (C) 1998-2004  Gerwin Klein <lsf@jflex.de>                  *
 *   All rights reserved.                                                  *
 *                                                                         *
 * This program is free software; you can redistribute it and/or modify    *
 * it under the terms of the GNU General Public License. See the file      *
 * COPYRIGHT for more information.                                         *
 *                                                                         *
 * This program is distributed in the hope that it will be useful,         *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of          *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
 * GNU General Public License for more details.                            *
 *                                                                         *
 * You should have received a copy of the GNU General Public License along *
 * with this program; if not, write to the Free Software Foundation, Inc., *
 * 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA                 *
 *                                                                         *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using CSFlex;

using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSFlex.GUI
{
    /**
     * C# Flex main application frame (GUI mode only)
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.6 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public sealed class MainFrame : Form
    {

        private volatile bool Choosing = false;

        private string FileName = "";
        private string DirName = "";

        private Button QuitButton;
        private Button OptionsButton;
        private Button GenerateButton;
        private Button StopButton;
        private Button SpecChooseButton;
        private Button DirChooseButton;

        private TextBox SpecTextBox;
        private TextBox DirTextBox;

        private RichTextBox MessagesTextBox;

        private GeneratorThread Thread;

        private OptionsDialog Dialog;


        public MainFrame()
        {
            this.Text = "JFlex " + MainClass.version;

            BuildContent();

            Closed += new EventHandler(MainFrame_Closed);

            Show();
        }

        private void MainFrame_Closed(object sender, EventArgs e)
        {
            DoQuit();
        }

        private int CalculateCharWidth(int num_chars, TextBox tb)
        {
            // first, actually average all the letters & numbers,
            // and eliminate that silly extra few pixels that
            // System.Drawing likes to add.

            var g = Graphics.FromHwnd(tb.Handle);

            var one_a = g.MeasureString("a", tb.Font);
            var two_a = g.MeasureString("aa", tb.Font);

            var seventy_characters = g.MeasureString(
              "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.!?'\"#-(", tb.Font);

            var width_of_an_a_glyph = two_a.Width - one_a.Width;
            var measurement_error = one_a.Width - width_of_an_a_glyph;

            var width_of_seventy_character_glyphs = seventy_characters.Width - measurement_error;
            var width_of_one_character = width_of_seventy_character_glyphs / 70.0f;

            g.Dispose();

            return (int)(width_of_one_character * num_chars);
        }

        private int CalculateCharWidth(int num_chars, RichTextBox tb)
        {
            // first, actually average all the letters & numbers,
            // and eliminate that silly extra few pixels that
            // System.Drawing likes to add.

            var g = Graphics.FromHwnd(tb.Handle);

            var one_a = g.MeasureString("a", tb.Font);
            var two_a = g.MeasureString("aa", tb.Font);

            var seventy_characters = g.MeasureString(
              "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.!?'\"#-(", tb.Font);

            var width_of_an_a_glyph = two_a.Width - one_a.Width;
            var measurement_error = one_a.Width - width_of_an_a_glyph;

            var width_of_seventy_character_glyphs = seventy_characters.Width - measurement_error;
            var width_of_one_character = width_of_seventy_character_glyphs / 70.0f;

            g.Dispose();

            return (int)(width_of_one_character * num_chars);
        }

        private int CalculateLinesHeight(int num_lines, TextBox tb)
        {
            return (num_lines * tb.Font.Height) + 4;
        }

        private int CalculateLinesHeight(int num_lines, RichTextBox tb)
        {
            return (num_lines * tb.Font.Height) + 4;
        }

        private void BuildContent()
        {
            this.BackColor = SystemColors.Control;

            this.GenerateButton = new()
            {
                Text = "Generate"
            };

            this.QuitButton = new()
            {
                Text = "Quit"
            };

            this.OptionsButton = new()
            {
                Text = "Options"
            };

            this.StopButton = new()
            {
                Text = "Stop"
            };

            this.DirChooseButton = new()
            {
                Text = "Browse"
            };

            this.DirTextBox = new ();
            this.DirTextBox.Width = CalculateCharWidth(10, DirTextBox);

            this.SpecChooseButton = new()
            {
                Text = "Browse"
            };

            this.SpecTextBox = new ();
            this.SpecTextBox.Width = CalculateCharWidth(10, SpecTextBox);

            this.MessagesTextBox = new()
            {
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both
            };
            this.MessagesTextBox.Font = new (FontFamily.GenericMonospace, MessagesTextBox.Font.Size, MessagesTextBox.Font.Style, MessagesTextBox.Font.Unit);
            this.MessagesTextBox.Width = CalculateCharWidth(80, MessagesTextBox);
            this.MessagesTextBox.Height = CalculateLinesHeight(10, MessagesTextBox);
            this.MessagesTextBox.ReadOnly = true;
            this.MessagesTextBox.MaxLength = int.MaxValue;

            this.GenerateButton.Click += new EventHandler(Generate_Click!);
            this.OptionsButton.Click += new EventHandler(Options_Click!);
            this.QuitButton.Click += new EventHandler(Quit_Click!);
            this.StopButton.Click += new EventHandler(Stop_Click!);
            this.SpecChooseButton.Click += new EventHandler(SpecChoose_Click!);
            this.DirChooseButton.Click += new EventHandler(DirChoose_Click!);
            this.SpecTextBox.KeyPress += new KeyPressEventHandler(Spec_KeyPress!);
            this.SpecTextBox.TextChanged += new EventHandler(Spec_TextChanged!);
            this.DirTextBox.KeyPress += new KeyPressEventHandler(Dir_KeyPress!);
            this.DirTextBox.TextChanged += new EventHandler(Dir_TextChanged!);

            var north = new GridPanel(5, 4, 10, 10);
            north.SetInsets(new Insets(10, 5, 5, 10));

            var lblLexicalSpecification = new Label();
            lblLexicalSpecification.AutoSize = true;
            lblLexicalSpecification.Text = "Lexical specification:";

            var lblOutputDirectory = new Label();
            lblOutputDirectory.AutoSize = true;
            lblOutputDirectory.Text = "Output directory:";

            north.Add(4, 0, QuitButton);
            north.Add(4, 1, GenerateButton);
            north.Add(4, 2, OptionsButton);
            north.Add(4, 3, StopButton);

            north.Add(0, 0, Handles.BOTTOM, lblLexicalSpecification);
            north.Add(0, 1, 2, 1, SpecTextBox);
            north.Add(2, 1, SpecChooseButton);

            north.Add(0, 2, Handles.BOTTOM, lblOutputDirectory);
            north.Add(0, 3, 2, 1, DirTextBox);
            north.Add(2, 3, DirChooseButton);

            var true_north = new Panel();

            true_north.Controls.Add(north);

            var lblMessages = new Label();
            lblMessages.TextAlign = ContentAlignment.MiddleCenter;
            lblMessages.Text = "Messages:";
            lblMessages.Dock = DockStyle.Top;

            var center = new Panel();

            center.Controls.Add(lblMessages);
            center.Controls.Add(MessagesTextBox);

            north.Size = north.GetPreferredSize();
            north.DoLayout();

            true_north.Size = north.Size;

            north.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            true_north.Dock = DockStyle.Top;

            center.Anchor = (AnchorStyles)15;
            center.Location = new Point(0, north.Height);
            center.Size = new Size(ClientSize.Width, ClientSize.Height - north.Height);

            SuspendLayout();
            Controls.Add(true_north);
            Controls.Add(center);
            ResumeLayout(false);

            MessagesTextBox.Top = lblMessages.Bottom;
            MessagesTextBox.Left = 4;
            MessagesTextBox.Width = center.Width - 8;
            MessagesTextBox.Height = center.Height - lblMessages.Height - 4;
            MessagesTextBox.Anchor = (AnchorStyles)15;

            this.ClientSize = new Size(north.Width + 8, ClientSize.Height);
            this.MinimumSize = this.ClientSize;

            SetEnabledAll(false);

            OutputWriter.SetGUIMode(MessagesTextBox);
        }

        private void ShowOptions()
        {
            if (Dialog == null)
            {
                Dialog = new OptionsDialog(this);
            }
            Dialog.ShowDialog();
        }

        private void SetEnabledAll(bool generating)
        {
            StopButton.Enabled = generating;
            QuitButton.Enabled = !generating;
            GenerateButton.Enabled = !generating;
            DirChooseButton.Enabled = !generating;
            DirTextBox.Enabled = !generating;
            SpecChooseButton.Enabled = !generating;
            SpecTextBox.Enabled = !generating;
        }

        private void DoGenerate()
        {
            // workaround for a weird AWT bug
            if (Choosing) return;

            SetEnabledAll(true);

            Thread = new GeneratorThread(this, FileName, DirName);
            Thread.Start();

            MessagesTextBox.Focus();
        }

        public void GenerationFinished(bool success)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => GenerationFinished(success)));
            }
            else
            {
                SetEnabledAll(false);

                MessagesTextBox.Focus();

                if (success)
                    MessagesTextBox.AppendText(OutputWriter.NewLine + "Generation finished successfully." + OutputWriter.NewLine);
                else
                    MessagesTextBox.AppendText(OutputWriter.NewLine + "Generation aborted." + OutputWriter.NewLine);
            }
        }

        private void DoStop()
        {
            if (Thread != null)
            {
                /* stop ok here despite deprecation (?)
                   I don't know any good way to abort generation without changing the
                   generator code */
               
                Thread.Stop();
                Thread = null;
            }
            this.GenerationFinished(false);
        }

        private void DoQuit()
        {
            this.Hide();
            Application.Exit();
        }

        private void DoDirChoose()
        {
            this.Choosing = true;

            var d = new OpenFileDialog();

            d.Title = "Choose directory";

            var result = d.ShowDialog();

            if (result != DialogResult.Cancel)
                DirTextBox.Text = Path.GetDirectoryName(Path.GetFullPath(d.FileName));

            d.Dispose();

            this.Choosing = false;
        }

        private void DoSpecChoose()
        {
            this.Choosing = true;

            var d = new OpenFileDialog();

            d.Title = "Choose file";
            d.Filter = "JFlex and CSFlex Specifications (*.flex)|*.flex|All files (*.*)|*.*";
            d.FilterIndex = 1;

            var result = d.ShowDialog();

            if (result != DialogResult.Cancel)
            {
                FileName = d.FileName;
                DirTextBox.Text = Path.GetDirectoryName(Path.GetFullPath(FileName));
                SpecTextBox.Text = FileName;
            }

            d.Dispose();

            Choosing = false;
        }

        private void Generate_Click(object sender, EventArgs e)
        {
            DoGenerate();
        }

        private void Options_Click(object sender, EventArgs e)
        {
            ShowOptions();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            DoQuit();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            DoStop();
        }

        private void SpecChoose_Click(object sender, EventArgs e)
        {
            DoSpecChoose();
        }

        private void DirChoose_Click(object sender, EventArgs e)
        {
            DoDirChoose();
        }

        private void Spec_TextChanged(object sender, EventArgs e)
        {
            FileName = SpecTextBox.Text;
        }

        private void Spec_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n')
            {
                FileName = SpecTextBox.Text;
                DoGenerate();
            }
        }

        private void Dir_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n')
            {
                this.DirName = DirTextBox.Text;
                this.DoGenerate();
            }
        }

        private void Dir_TextChanged(object sender, EventArgs e)
        {
            this.DirName = DirTextBox.Text;
        }
    }
}