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
using System.Threading;

namespace CSFlex.GUI
{
    /**
     * Low priority thread for code generation (low priority so
     * that gui has time for screen updates)
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.5 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class GeneratorThread
    {

        /** there must be at most one instance of this Thread running */
        protected volatile bool Running = false;

        /** input file setting from GUI */
        protected string InputFile;

        /** output directory */
        protected string OutputDir;

        /** main UI component, likes to be notified when generator finishes */
        protected MainFrame Parent;

        /**
         * Create a new GeneratorThread, but do not run it yet.
         * 
         * @param parent      the frame, main UI component
         * @param inputFile   input file from UI settings
         * @param messages    where generator messages should appear
         * @param outputDir   output directory from UI settings
         */
        public GeneratorThread(MainFrame Parent, string InputFile,string OutputDir)
        {
            this.Parent = Parent;
            this.InputFile = InputFile;
            this.OutputDir = OutputDir;
        }


        /**
         * Run the generator thread. Only one instance of it can run at any time.
         */
        protected Thread Thread;

        public void Start()
        {
            lock (this)
            {
                if (Running)
                {
                    OutputWriter.Error(ErrorMessages.ALREADY_RUNNING);
                    Parent.GenerationFinished(false);
                }
                else
                {
                    Running = true;

                    Thread = new (new ThreadStart(Run));

                    //thread.Priority = ThreadPriority.BelowNormal;
                    Thread.IsBackground = true;
                    Thread.Start();
                }
            }
        }

        public void Run()
        {
            try
            {
                if (OutputDir != "")
                    Options.SetDir(OutputDir);

                MainClass.Generate(new File(InputFile));
                OutputWriter.Statistics();
                Parent.GenerationFinished(true);
            }
            catch (GeneratorException)
            {
                OutputWriter.Statistics();
                Parent.GenerationFinished(false);
            }
            catch (ThreadInterruptedException) { }
            catch (ThreadAbortException) { }
            finally
            {
                lock (this)
                {
                    Running = false;
                    Thread = null;
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (this.Running)
                {
                    Thread?.Interrupt();
                    Thread?.Abort();
                }
            }
        }
    }
}
