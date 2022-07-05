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
namespace CSFlex.GUI;

/**
 * A dialog for setting C# Flex options
 * 
 * @author Gerwin Klein
 * @version $Revision: 1.6 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public class OptionsDialog : Form
{

    private Form Owner;

    private Button SkelBrowse;
    private TextBox SkelFile;

    private Button OkButton;
    private Button DefaultsButton;

    private CheckBox DumpCheckBox;
    private CheckBox VerboseCheckBox;
    private CheckBox JLexCheckBox;
    private CheckBox NoMinimizeCheckBox;
    private CheckBox NoBackupCheckBox;
    private CheckBox TimeCheckBox;
    private CheckBox DotCheckBox;
    private CheckBox CSharpCheckBox;

    private RadioButton TableGRadioButton;
    private RadioButton SwitchGRadioButton;
    private RadioButton PackGRadioButton;


    /**
     * Create a new options dialog
     * 
     * @param owner
     */
    public OptionsDialog(Form owner)
    {
        this.Text = "Options";

        this.Owner = owner;

        Setup();
    }

    public void Setup()
    {
        // create components
        OkButton = new Button
        {
            Text = "Ok"
        };

        DefaultsButton = new Button
        {
            Text = "Defaults"
        };

        SkelBrowse = new Button
        {
            Text = " Browse"
        };

        SkelFile = new TextBox
        {
            ReadOnly = true
        };

        DumpCheckBox = new CheckBox
        {
            Text = " Dump"
        };

        VerboseCheckBox = new CheckBox
        {
            Text = " Verbose"
        };

        JLexCheckBox = new CheckBox
        {
            Text = " JLex"
        };

        NoMinimizeCheckBox = new CheckBox
        {
            Text = " Skip Min"
        };

        NoBackupCheckBox = new CheckBox
        {
            Text = " No Backup"
        };

        TimeCheckBox = new CheckBox
        {
            Text = " Time Stats"
        };

        DotCheckBox = new CheckBox
        {
            Text = " Dot Graph"
        };

        CSharpCheckBox = new CheckBox
        {
            Text = " C# Output"
        };

        TableGRadioButton = new RadioButton
        {
            Text = " Table"
        };

        SwitchGRadioButton = new RadioButton
        {
            Text = " Switch"
        };

        PackGRadioButton = new RadioButton
        {
            Text = " pack"
        };

        switch (Options.GenMethod)
        {
            case Options.TABLE: TableGRadioButton.Checked = true; break;
            case Options.SWITCH: SwitchGRadioButton.Checked = true; break;
            case Options.PACK: PackGRadioButton.Checked = true; break;
        }

        // setup interaction
        OkButton.Click += new EventHandler(OkButton_Click!);
        DefaultsButton.Click += new EventHandler(DefaultsButton_Click!);
        SkelBrowse.Click += new EventHandler(SkelBrowse_Click!);
        TableGRadioButton.CheckedChanged += new EventHandler(TableG_CheckedChanged!);
        SwitchGRadioButton.CheckedChanged += new EventHandler(SwitchG_CheckedChanged!);
        PackGRadioButton.CheckedChanged += new EventHandler(PackG_CheckedChanged!);
        VerboseCheckBox.CheckedChanged += new EventHandler(Verbose_CheckedChanged!);
        DumpCheckBox.CheckedChanged += new EventHandler(Dump_CheckedChanged!);
        JLexCheckBox.CheckedChanged += new EventHandler(Jlex_CheckedChanged!);
        NoMinimizeCheckBox.CheckedChanged += new EventHandler(No_minimize_CheckedChanged!);
        NoBackupCheckBox.CheckedChanged += new EventHandler(No_backup_CheckedChanged!);
        DotCheckBox.CheckedChanged += new EventHandler(Dot_CheckedChanged!);
        CSharpCheckBox.CheckedChanged += new EventHandler(CSharp_CheckedChanged!);
        TimeCheckBox.CheckedChanged += new EventHandler(Time_CheckedChanged!);

        // setup layout
        var panel = new GridPanel(4, 7, 10, 10);
        panel.SetInsets(new Insets(10, 5, 5, 10));

        panel.Add(3, 0, OkButton);
        panel.Add(3, 1, DefaultsButton);

        var lblSkeletonFile = new Label();
        lblSkeletonFile.AutoSize = true;
        lblSkeletonFile.Text = "skeleton file:";

        var lblCode = new Label();
        lblCode.AutoSize = true;
        lblCode.Text = "code:";

        panel.Add(0, 0, 2, 1, Handles.BOTTOM, lblSkeletonFile);
        panel.Add(0, 1, 2, 1, SkelFile);
        panel.Add(2, 1, 1, 1, Handles.TOP, SkelBrowse);

        panel.Add(0, 2, 1, 1, Handles.BOTTOM, lblCode);
        panel.Add(0, 3, 1, 1, TableGRadioButton);
        panel.Add(0, 4, 1, 1, SwitchGRadioButton);
        panel.Add(0, 5, 1, 1, PackGRadioButton);

        panel.Add(1, 3, 1, 1, DumpCheckBox);
        panel.Add(1, 4, 1, 1, VerboseCheckBox);
        panel.Add(1, 5, 1, 1, TimeCheckBox);

        panel.Add(2, 3, 1, 1, NoMinimizeCheckBox);
        panel.Add(2, 4, 1, 1, NoBackupCheckBox);
        panel.Add(2, 5, 1, 1, CSharpCheckBox);

        panel.Add(3, 3, 1, 1, JLexCheckBox);
        panel.Add(3, 4, 1, 1, DotCheckBox);

        panel.Size = panel.GetPreferredSize();
        panel.DoLayout();

        var panel_size = panel.Size;
        var client_area_size = this.ClientSize;
        var left_over = new Size(client_area_size.Width - panel_size.Width,
            client_area_size.Height - panel_size.Height);

        Controls.Add(panel);

        panel.Location = new(0, 8);
        this.ClientSize = new(panel.Width + 8, panel.Height + 8);
        this.MaximumSize = this.MinimumSize = this.ClientSize;

        UpdateState();
    }

    private void DoskelBrowse()
    {
        var d = new OpenFileDialog
        {
            Title = "Choose file"
        };

        var result = d.ShowDialog();

        if (result != DialogResult.Cancel)
        {
            var skel = d.FileName;
            try
            {
                Skeleton.ReadSkelFile(skel);
                SkelFile.Text = skel;
            }
            catch (GeneratorException)
            {
                // do nothing
            }
        }

        d.Dispose();
    }

    private void SetGenMethod()
    {
        if (TableGRadioButton.Checked)
        {
            Options.GenMethod = Options.TABLE;
            return;
        }

        if (SwitchGRadioButton.Checked)
        {
            Options.GenMethod = Options.SWITCH;
            return;
        }

        if (PackGRadioButton.Checked)
        {
            Options.GenMethod = Options.PACK;
            return;
        }
    }

    private void UpdateState()
    {
        DumpCheckBox.Checked = Options.Dump;
        VerboseCheckBox.Checked = Options.Verbose;
        JLexCheckBox.Checked = Options.JLex;
        NoMinimizeCheckBox.Checked = Options.NoMinimize;
        NoBackupCheckBox.Checked = Options.NoBackup;
        TimeCheckBox.Checked = Options.Time;
        DotCheckBox.Checked = Options.Dot;

        switch (Options.GenMethod)
        {
            case Options.TABLE: TableGRadioButton.Checked = true; break;
            case Options.SWITCH: SwitchGRadioButton.Checked = true; break;
            case Options.PACK: PackGRadioButton.Checked = true; break;
        }
    }

    private void SetDefaults()
    {
        Options.SetDefaults();
        Skeleton.ReadDefault();
        SkelFile.Text = "";
        UpdateState();
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private void DefaultsButton_Click(object sender, EventArgs e)
    {
        SetDefaults();
    }

    private void SkelBrowse_Click(object sender, EventArgs e)
    {
        DoskelBrowse();
    }

    private void TableG_CheckedChanged(object sender, EventArgs e)
    {
        if (TableGRadioButton.Checked)
            SetGenMethod();
    }

    private void SwitchG_CheckedChanged(object sender, EventArgs e)
    {
        if (SwitchGRadioButton.Checked)
            SetGenMethod();
    }

    private void PackG_CheckedChanged(object sender, EventArgs e)
    {
        if (PackGRadioButton.Checked)
            SetGenMethod();
    }

    private void Verbose_CheckedChanged(object sender, EventArgs e)
    {
        Options.Verbose = VerboseCheckBox.Checked;
    }

    private void Dump_CheckedChanged(object sender, EventArgs e)
    {
        Options.Dump = DumpCheckBox.Checked;
    }

    private void Jlex_CheckedChanged(object sender, EventArgs e)
    {
        Options.JLex = JLexCheckBox.Checked;
    }

    private void No_minimize_CheckedChanged(object sender, EventArgs e)
    {
        Options.NoMinimize = NoMinimizeCheckBox.Checked;
    }

    private void No_backup_CheckedChanged(object sender, EventArgs e)
    {
        Options.NoBackup = NoBackupCheckBox.Checked;
    }

    private void Dot_CheckedChanged(object sender, EventArgs e)
    {
        Options.Dot = DotCheckBox.Checked;
    }

    private void Time_CheckedChanged(object sender, EventArgs e)
    {
        Options.Time = TimeCheckBox.Checked;
    }

    private void CSharp_CheckedChanged(object sender, EventArgs e)
    {
        Options.EmitCSharp = CSharpCheckBox.Checked;
    }
}
