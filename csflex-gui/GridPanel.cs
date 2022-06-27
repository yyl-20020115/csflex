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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CSFlex.GUI
{
    /**
     * Grid layout manager like GridLayout but with predefinable
     * grid size.
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class GridPanel : Control
    {
        protected int Cols;
        protected int Rows;

        protected int HGap;
        protected int VGap;

        protected List<GridPanelConstraint> Constraints = new ();
        protected Insets insets = new (0, 0, 0, 0);

        public GridPanel(int cols, int rows)
          : this(cols, rows, 0, 0)
        {
            Resize += new EventHandler(GridPanel_Resize);
        }

        public GridPanel(int cols, int rows, int hgap, int vgap)
        {
            this.Cols = cols;
            this.Rows = rows;
            this.HGap = hgap;
            this.VGap = vgap;
        }

        public void DoLayout()
        {
            var size = Size;
            size.Height -= insets.Top + insets.Bottom;
            size.Width -= insets.Left + insets.Right;

            var cellWidth = size.Width / Cols;
            var cellHeight = size.Height / Rows;

            for (int i = 0; i < Constraints.Count; i++)
            {
                var c = Constraints[i];

                float x = cellWidth * c.X + insets.Left + HGap / 2;
                float y = cellHeight * c.Y + insets.Right + VGap / 2;

                float width, height;

                if (c.Handle == Handles.FILL)
                {
                    width = (cellWidth - HGap) * c.Width;
                    height = (cellHeight - VGap) * c.Height;
                }
                else
                {
                    var d = c.Component.Size;
                    width = d.Width;
                    height = d.Height;
                }

                switch (c.Handle)
                {
                    case Handles.TOP_CENTER:
                        x += (cellWidth + width) / 2;
                        break;
                    case Handles.TOP_RIGHT:
                        x += cellWidth - width;
                        break;
                    case Handles.CENTER_LEFT:
                        y += (cellHeight + height) / 2;
                        break;
                    case Handles.CENTER:
                        x += (cellWidth + width) / 2;
                        y += (cellHeight + height) / 2;
                        break;
                    case Handles.CENTER_RIGHT:
                        y += (cellHeight + height) / 2;
                        x += cellWidth - width;
                        break;
                    case Handles.BOTTOM:
                        y += cellHeight - height;
                        break;
                    case Handles.BOTTOM_CENTER:
                        x += (cellWidth + width) / 2;
                        y += cellHeight - height;
                        break;
                    case Handles.BOTTOM_RIGHT:
                        y += cellHeight - height;
                        x += cellWidth - width;
                        break;
                }

                c.Component.Bounds = new Rectangle((int)x, (int)y, (int)width, (int)height);
            }
        }

        public Size GetPreferredSize()
        {
            float dy = 0;
            float dx = 0;

            for (int i = 0; i < Constraints.Count; i++)
            {
                var c = Constraints[i];

                var d = c.Component.Size;

                dx = Math.Max(dx, d.Width / c.Width);
                dy = Math.Max(dy, d.Height / c.Height);
            }

            dx += HGap;
            dy += VGap;

            dx *= Cols;
            dy *= Rows;

            dx += insets.Left + insets.Right;
            dy += insets.Top + insets.Bottom;

            return new ((int)dx, (int)dy);
        }

        public void SetInsets(Insets insets)
        {
            this.insets = insets;
        }

        public void Add(int x, int y, Control c)
        {
            this.Add(x, y, 1, 1, Handles.FILL, c);
        }

        public void Add(int x, int y, Handles handle, Control c)
        {
            this.Add(x, y, 1, 1, handle, c);
        }

        public void Add(int x, int y, int dx, int dy, Control c)
        {
            this.Add(x, y, dx, dy, Handles.FILL, c);
        }

        public void Add(int x, int y, int dx, int dy, Handles handle, Control c)
        {
            this.Controls.Add(c);
            this.Constraints.Add(new GridPanelConstraint(x, y, dx, dy, handle, c));
        }

        private void GridPanel_Resize(object sender, EventArgs e)
        {
            this.DoLayout();
        }
    }

    public struct Insets
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public Insets(int Left, int Right, int Top, int Bottom)
        {
            this.Left = Left;
            this.Right = Right;
            this.Top = Top;
            this.Bottom = Bottom;
        }
    }
}
