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
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CSFlex
{


    /**
     * Convenience class for C# Flex stdout, redirects output to a RichTextBox
     * if in GUI mode.
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public sealed class StdOutWriter : TextWriter
    {
        private Encoding encoding = Encoding.Default;
        public override Encoding Encoding => this.Encoding;

        /** text area to write to if in gui mode, gui mode = (text != null) */
        private RichTextBox? text = null;

        /** text accumulated to be appended to the textbox */
        private StringBuilder text_queue = new();

        /** timeout to actually append incoming text to the textbox */
        private System.Threading.Timer? timer_timeout = null;

        /** 
         * approximation of the current column in the text area
         * for auto wrapping at <code>wrap</code> characters
         **/
        private int col = 0;

        /** auto wrap lines in gui mode at this value */
        private const int wrap = 78;

        private TextWriter writer;

        /** A StdOutWriter, attached to System.out, no gui mode */
        public StdOutWriter()
        {
            writer = Console.Out;
            encoding = Encoding.Default;
        }

        /** A StdOutWrite, attached to the specified output stream, no gui mode */
        public StdOutWriter(Stream stream,Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            writer = new StreamWriter(stream, this.encoding = encoding);
        }

        /**
         * Set the RichTextBox to write text to. Will continue
         * to write to System.out if text is <code>null</code>.
         *
         * @param text  the RichTextBox to write to
         */
        public void SetGUIMode(RichTextBox text)
        {
            this.text = text;
        }

        public delegate void SimpleDelegate();

        private object SyncRoot = new ();


        private void AppendStringInGUIMode(string str)
        {
            lock (SyncRoot)
            {
                if (timer_timeout == null)
                {
                    timer_timeout = new System.Threading.Timer(
                        new TimerCallback(Timer_Timeout_Callback), null, 300, Timeout.Infinite);
                }

                text_queue.Append(str);
            }
        }

        private void Timer_Timeout_Callback(object sender)
        {
            if (text_queue != null)
                text.Invoke(new SimpleDelegate(AppendStringInGUIModeProxy));
        }

        void AppendStringInGUIModeProxy()
        {
            lock (SyncRoot)
            {
                text.AppendText(text_queue.ToString());
                text.SelectionStart = text.TextLength;

                text_queue = null;

                timer_timeout.Dispose();
                timer_timeout = null;
            }
        }

        /** Write a single character. */
        public override void Write(char c)
        {
            if (text != null)
            {
                AppendStringInGUIMode(c.ToString());
                if (++col > wrap) WriteLine();
            }
            else
                writer.Write(c);
        }

        /** Write a portion of an array of characters. */
        public override void Write(char[] buf, int off, int len)
        {
            if (text != null)
            {
                AppendStringInGUIMode(new string(buf, off, len));
                if ((col += len) > wrap) WriteLine();
            }
            else
                writer.Write(buf, off, len);
        }

        /** Write a portion of a string. */
        public void Write(string s, int off, int len)
        {
            if (text != null)
            {
                AppendStringInGUIMode(s.Substring(off, len));
                if ((col += len) > wrap) WriteLine();
            }
            else
            {
                writer.Write(s.Substring(off, len));
                Flush();
            }
        }

        /**
         * Begin a new line. Which actual character/s is/are written 
         * depends on the runtime platform.
         */
        public override void WriteLine()
        {
            if (text != null)
            {
                AppendStringInGUIMode(Environment.NewLine);
                col = 0;
            }
            else
            {
                writer.WriteLine();
            }
        }

        public override void WriteLine(string str)
        {
            Write(str, 0, str.Length);
            WriteLine();
        }
    }
}