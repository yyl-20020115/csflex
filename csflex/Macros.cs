// #define DEBUG_TRACE

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
namespace CSFlex;

/**
 * Symbol table and expander for macros.
 * 
 * Maps macros to their (expanded) definitions, detects cycles and 
 * unused macros.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class Macros
{

    /** Maps names of macros to their definition */
    private Dictionary<string, RegExp> macros = new();

    /** Maps names of macros to their "used" flag */
    private Dictionary<string, bool> used = new();

#if DEBUG_TRACE
System.IO.StreamWriter log = new System.IO.StreamWriter(@"x:\macros_csharp.log");
#endif // DEBUG_TRACE

    /**
     * Creates a new macro expander.
     */
    public Macros()
    {
#if DEBUG_TRACE
log.AutoFlush = true;
log.WriteLine("Macros()");
#endif // DEBUG_TRACE
    }


    /**
     * Stores a new macro and its definition.
     *
     * @param name         the name of the new macro
     * @param definition   the definition of the new macro
     *
     * @return <code>true</code>, iff the macro name has not been
     *         stored before.
     */
    public bool Insert(string? name, RegExp? definition)
    {
        if (name == null || definition == null) return false;
#if DEBUG_TRACE
log.WriteLine("insert(string name = \"{0}\", RegExp definition = {1})", name, definition);
#endif // DEBUG_TRACE

        if (Options.DEBUG)
            OutputWriter.Debug("inserting macro " + name + " with definition :" + OutputWriter.NewLine + definition); //$NON-NLS-1$ //$NON-NLS-2$

        used[name] = false;

        bool new_entry = !macros.ContainsKey(name);
        macros[name] = definition;
        return new_entry;
    }


    /**
     * Marks a makro as used.
     *
     * @return <code>true</code>, iff the macro name has been
     *         stored before.
     */
    public bool MarkUsed(string name)
    {
#if DEBUG_TRACE
log.WriteLine("markUsed(string name = \"{0}\")", name);
#endif // DEBUG_TRACE

        bool already_stored = used.ContainsKey(name);
        used[name] = true;
        return already_stored;
    }


    /**
     * Tests if a macro has been used.
     *
     * @return <code>true</code>, iff the macro has been used in 
     *         a regular expression.
     */
    public bool IsUsed(string name)
    {
#if DEBUG_TRACE
log.WriteLine("isUsed(string name = \"{0}\")", name);
#endif // DEBUG_TRACE

        return (bool)used[name];
    }


    /**
     * Returns all unused macros.
     *
     * @return the enumeration of macro names that have not been used.
     */
    public PrettyList<string> GetUnusedNames()
    {
#if DEBUG_TRACE
log.WriteLine("unused()");
#endif // DEBUG_TRACE

        var unUsed = new PrettyList<string>();
        foreach(var up in used)
        {
            if (!up.Value)
            {
                unUsed.Add(up.Key);
            }
        }
        return unUsed;
    }


    /**
     * Fetches the definition of the macro with the specified name,
     * <p>
     * The definition will either be the same as stored (expand() not 
     * called), or an equivalent one, that doesn't contain any macro 
     * usages (expand() called before).
     *
     * @param name   the name of the macro
     *
     * @return the definition of the macro, <code>null</code> if 
     *         no macro with the specified name has been stored.
     *
     * @see CSFlex.Macros#expand
     */
    public RegExp GetDefinition(string name)
    {
#if DEBUG_TRACE
log.WriteLine("getDefinition(string name = \"{0}\")", name);
#endif // DEBUG_TRACE

        return macros[name];
    }


    /**
     * Expands all stored macros, so that getDefinition always returns
     * a defintion that doesn't contain any macro usages.
     *
     * @throws MacroException   if there is a cycle in the macro usage graph.
     */
    public void Expand()
    {
#if DEBUG_TRACE
log.WriteLine("expand()");
#endif // DEBUG_TRACE

        // FIXME: ugly hack for C# since the 'macros[name]' assignment below
        // counts as a change to the collection
        // for debugging: sort the list
        var keys = new List<string>(macros.Keys);

        foreach(var name in keys)
        {
            //string name = (string)names.Current;
            if (IsUsed(name))
                macros[name] = ExpandMacro(name, GetDefinition(name));
            // this put doesn't get a new key, so only a new value
            // is set for the key "name" (without changing the enumeration 
            // "names"!)
        }
    }


    /**
     * Expands the specified macro by replacing each macro usage
     * with the stored definition. 
     *   
     * @param name        the name of the macro to expand (for detecting cycles)
     * @param definition  the definition of the macro to expand
     *
     * @return the expanded definition of the macro.
     * 
     * @throws MacroException when an error (such as a cyclic definition)
     *                              occurs during expansion
     */
    private RegExp? ExpandMacro(string name, RegExp? definition)
    {
        if (definition == null) return definition;
#if DEBUG_TRACE
log.WriteLine("expandMacro(string name = \"{0}\", RegExp definition = {1})", name, definition);
#endif // DEBUG_TRACE

        // Out.print("checking macro "+name);
        // Out.print("definition is "+definition);

        switch (definition.type)
        {
            case Symbols.BAR:
            case Symbols.CONCAT:
                var binary = (RegExp2)definition;
                binary.r1 = ExpandMacro(name, binary.r1);
                binary.r2 = ExpandMacro(name, binary.r2);
                return definition;

            case Symbols.STAR:
            case Symbols.PLUS:
            case Symbols.QUESTION:
            case Symbols.BANG:
            case Symbols.TILDE:
                var unary = (RegExp1)definition;
                unary.content = ExpandMacro(name, unary.content as RegExp);
                return definition;

            case Symbols.MACROUSE:
                var usename = ((RegExp1)definition).content is string s?s:"";

                if (name.Equals(usename))
                    throw new MacroException(ErrorMessages.Get(ErrorMessages.MACRO_CYCLE, name));

                var usedef = GetDefinition(usename);

                if (usedef == null)
                    throw new MacroException(ErrorMessages.Get(ErrorMessages.MACRO_DEF_MISSING, usename, name));

                MarkUsed(usename);

                return ExpandMacro(name, usedef);

            case Symbols.STRING:
            case Symbols.STRING_I:
            case Symbols.CHAR:
            case Symbols.CHAR_I:
            case Symbols.CCLASS:
            case Symbols.CCLASSNOT:
                return definition;

            default:
                throw new MacroException("unknown expression type " + definition.type + " in macro expansion"); //$NON-NLS-1$ //$NON-NLS-2$
        }
    }
}