/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * C# Flex                                                                 *
 * Copyright © 2021 Christian Klauser <christianklauser@outlook.com>       *
 * Derived from:                                                           *
 *                                                                         *
 *   C# Flex 1.4                                                           *
 *   Copyright (C) 2004-2005  Jonathan Gilbert <logic@deltaq.org>          *
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
namespace CSFlex;

/**
 * This is the main class of C# Flex controlling the scanner generation process. 
 * It is responsible for parsing the commandline, getting input files,
 * starting up the GUI if necessary, etc. 
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.18 $, $Date: 2004/04/12 10:34:10 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public class MainClass
{
    /** C# Flex version */
    public static readonly string version = typeof(MainClass).Assembly.GetName().Version!.ToString(); //$NON-NLS-1$

    /**
     * Generates a scanner for the specified input file.
     *
     * @param inputFile  a file containing a lexical specification
     *                   to generate a scanner for.
     */
    public static void Generate(File inputFile)
    {
        OutputWriter.ResetCounters();

        var totalTime = new Timer();
        var time = new Timer();

        LexScan? scanner = null;
        LexParse? parser = null;
        TextReader? inputReader = null;

        totalTime.Start();

        try
        {
            OutputWriter.Println(ErrorMessages.READING, inputFile.ToString());
            inputReader = new StreamReader(inputFile);
            scanner = new LexScan(inputReader);
            scanner.SetFile(inputFile);
            parser = new LexParse(scanner);
        }
        catch (FileNotFoundException)
        {
            OutputWriter.Error(ErrorMessages.CANNOT_OPEN, inputFile.ToString());
            throw new GeneratorException();
        }

        try
        {
            var nfa = (parser?.Parse()?.Value as NFA)!;

            OutputWriter.CheckErrors();

            if (Options.Dump) OutputWriter.Dump(ErrorMessages.Get(ErrorMessages.NFA_IS) +
                                       OutputWriter.NewLine + nfa + OutputWriter.NewLine);

            if (Options.Dot)
                nfa.WriteDot(Emitter.Normalize("nfa.dot", null));       //$NON-NLS-1$

            OutputWriter.Println(ErrorMessages.NFA_STATES, nfa.numStates);

            time.Start();
            DFA dfa = nfa.GetDFA();
            time.Stop();
            OutputWriter.Time(ErrorMessages.DFA_TOOK, time);

            dfa.CheckActions(scanner, parser);

            nfa = null;

            if (Options.Dump) OutputWriter.Dump(ErrorMessages.Get(ErrorMessages.DFA_IS) +
                                       OutputWriter.NewLine + dfa + OutputWriter.NewLine);

            if (Options.Dot)
                dfa.WriteDot(Emitter.Normalize("dfa-big.dot", null)); //$NON-NLS-1$

            time.Start();
            dfa.Minimize();
            time.Stop();

            OutputWriter.Time(ErrorMessages.MIN_TOOK, time);

            if (Options.Dump)
                OutputWriter.Dump(ErrorMessages.Get(ErrorMessages.MIN_DFA_IS) +
                                           OutputWriter.NewLine + dfa);

            if (Options.Dot)
                dfa.WriteDot(Emitter.Normalize("dfa-min.dot", null)); //$NON-NLS-1$

            time.Start();

            var emitter = new Emitter(inputFile, parser!, dfa);
            emitter.Emit();

            time.Stop();

            OutputWriter.Time(ErrorMessages.WRITE_TOOK, time);

            totalTime.Stop();

            OutputWriter.Time(ErrorMessages.TOTAL_TIME, totalTime);
        }
        catch (ScannerException e)
        {
            OutputWriter.Error(e.file, e.message, e.line, e.column);
            throw new GeneratorException();
        }
        catch (MacroException e)
        {
            OutputWriter.Error_(e.Message);
            throw new GeneratorException();
        }
        catch (IOException e)
        {
            OutputWriter.Error(ErrorMessages.IO_ERROR, e.ToString());
            throw new GeneratorException();
        }
        catch (OutOfMemoryException)
        {
            OutputWriter.Error(ErrorMessages.OUT_OF_MEMORY);
            throw new GeneratorException();
        }
        catch (GeneratorException)
        {
            throw new GeneratorException();
        }
        catch (Exception e)
        {
            OutputWriter.Error_(e.ToString());
            throw new GeneratorException();
        }
    }

    public static List<File> ParseOptions(string[] argv)
    {
        var files = new PrettyArrayList<File>();

        for (int i = 0; i < argv.Length; i++)
        {

            if ((argv[i] == "-d") || (argv[i] == "--outdir"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                if (++i >= argv.Length)
                {
                    OutputWriter.Error(ErrorMessages.NO_DIRECTORY);
                    throw new GeneratorException();
                }
                Options.SetDir(argv[i]);
                continue;
            }

            if ((argv[i] == "--skel") || (argv[i] == "-skel"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                if (++i >= argv.Length)
                {
                    OutputWriter.Error(ErrorMessages.NO_SKEL_FILE);
                    throw new GeneratorException();
                }

                Options.SetSkeleton(new File(argv[i]));
                continue;
            }

            if ((argv[i] == "--nested-default-skeleton") || (argv[i] == "-nested"))
            {
                Options.SetSkeleton(new File("<nested>"));
                continue;
            }

            if ((argv[i] == "-jlex") || (argv[i] == "--jlex"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.JLex = true;
                continue;
            }

            if ((argv[i] == "-v") || (argv[i] == "--verbose") || (argv[i] == "-verbose"))
            { //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
                Options.Verbose = true;
                Options.progress = true;
                continue;
            }

            if ((argv[i] == "-q") || (argv[i] == "--quiet") || (argv[i] == "-quiet"))
            { //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
                Options.Verbose = false;
                Options.progress = false;
                continue;
            }

            if ((argv[i] == "--dump") || (argv[i] == "-dump"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.Dump = true;
                continue;
            }

            if ((argv[i] == "--time") || (argv[i] == "-time"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.Time = true;
                continue;
            }

            if ((argv[i] == "--version") || (argv[i] == "-version"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                OutputWriter.Println(ErrorMessages.THIS_IS_CSFLEX, version);
                throw new SilentExitException();
            }

            if ((argv[i] == "--dot") || (argv[i] == "-dot"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.Dot = true;
                continue;
            }

            if ((argv[i] == "--help") || (argv[i] == "-h") || (argv[i] == "/h"))
            { //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
                PrintUsage();
                throw new SilentExitException();
            }

            if ((argv[i] == "--info") || (argv[i] == "-info"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                OutputWriter.PrintSystemInfo();
                throw new SilentExitException();
            }

            if ((argv[i] == "--nomin") || (argv[i] == "-nomin"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.NoMinimize = true;
                continue;
            }

            if ((argv[i] == "--pack") || (argv[i] == "-pack"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.GenMethod = Options.PACK;
                continue;
            }

            if ((argv[i] == "--table") || (argv[i] == "-table"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.GenMethod = Options.TABLE;
                continue;
            }

            if ((argv[i] == "--switch") || (argv[i] == "-switch"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.GenMethod = Options.SWITCH;
                continue;
            }

            if ((argv[i] == "--nobak") || (argv[i] == "-nobak"))
            { //$NON-NLS-1$ //$NON-NLS-2$
                Options.NoBackup = true;
                continue;
            }

            if ((argv[i] == "--csharp") || (argv[i] == "-cs"))
            {
                Options.EmitCSharp = true;
                continue;
            }

            if (argv[i].StartsWith("-"))
            { //$NON-NLS-1$
                OutputWriter.Error(ErrorMessages.UNKNOWN_COMMANDLINE, argv[i]);
                PrintUsage();
                throw new SilentExitException();
            }

            // if argv[i] is not an option, try to read it as file 
            var f = new File(argv[i]);
            if (f.IsFile && f.CanRead)
                files.Add(f);
            else
            {
                OutputWriter.Error_("Sorry, couldn't open \"" + f + "\""); //$NON-NLS-2$
                throw new GeneratorException();
            }
        }

        return files;
    }


    public static void PrintUsage()
    {
        OutputWriter.Println(""); //$NON-NLS-1$
        OutputWriter.Println("Usage: csflex <options> <input-files>");
        OutputWriter.Println("");
        OutputWriter.Println("Where <options> can be one or more of");
        OutputWriter.Println("-d <directory>   write generated file to <directory>");
        OutputWriter.Println("--skel <file>    use external skeleton <file>");
        OutputWriter.Println("--switch");
        OutputWriter.Println("--table");
        OutputWriter.Println("--pack           set default code generation method");
        OutputWriter.Println("--jlex           strict JLex compatibility");
        OutputWriter.Println("--nomin          skip minimization step");
        OutputWriter.Println("--nobak          don't create backup files");
        OutputWriter.Println("--dump           display transition tables");
        OutputWriter.Println("--dot            write graphviz .dot files for the generated automata (alpha)");
        OutputWriter.Println("--nested-default-skeleton");
        OutputWriter.Println("-nested          use the skeleton with support for nesting (included files)");
        OutputWriter.Println("--csharp         ***");
        OutputWriter.Println("-csharp          * Important: Enable C# code generation");
        OutputWriter.Println("--verbose        ***");
        OutputWriter.Println("-v               display generation progress messages (default)");
        OutputWriter.Println("--quiet");
        OutputWriter.Println("-q               display errors only");
        OutputWriter.Println("--time           display generation time statistics");
        OutputWriter.Println("--version        print the version number of this copy of C# Flex");
        OutputWriter.Println("--info           print system + JDK information");
        OutputWriter.Println("--help");
        OutputWriter.Println("-h               print this message");
        OutputWriter.Println("");
        OutputWriter.Println(ErrorMessages.THIS_IS_CSFLEX, version);
        OutputWriter.Println("Have a nice day!");
    }

    public static void Generate(string[] argv)
    {
        var files = ParseOptions(argv);

        if (files.Count > 0)
        {
            foreach (var file in files)
                Generate(file);
        }
    }


    /**
     * Starts the generation process with the files in <code>argv</code> or
     * pops up a window to choose a file, when <code>argv</code> doesn't have
     * any file entries.
     *
     * @param argv the commandline.
     */
    [STAThread]
    public static void Main(string[] argv)
    {
        try
        {
            Generate(argv);
        }
        catch (GeneratorException)
        {
            OutputWriter.Statistics();
            Environment.Exit(1);
        }
        catch (SilentExitException)
        {
            Environment.Exit(1);
        }
    }
}