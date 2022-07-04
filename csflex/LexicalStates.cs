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
 * Simple symbol table, mapping lexical state names to integers. 
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public class LexicalStates
{
    /** maps state name to state number */
    protected readonly PrettyHashtable<string, int> states = new();
    /** codes of inclusive states (subset of states) */
    protected readonly PrettyArrayList<int> inclusive = new();
    /** number of declared states */
    protected int numStates = 0;
    /**
     * constructs a new lexical state symbol table
     */
    public LexicalStates() { }
    /**
     * insert a new state declaration
     */
    public void Insert(string name, bool is_inclusive)
    {
        if (states.ContainsKey(name)) return;

        var code = numStates++;
        states[name] = code;

        if (is_inclusive)
            inclusive.Add(code);
    }

    /**
     * returns the number (code) of a declared state, 
     * <code>null</code> if no such state has been declared.
     */
    public int? GetNumber(string? name) => name != null && states.TryGetValue(name, out var n) ? n : null;// states[name];

    /**
     * returns the number of declared states
     */
    public int CountOfDeclaredStates => numStates;

    /**
     * returns the names of all states
     */
    public IEnumerable<string> Names => states.Keys;

    /**
     * returns the code of all inclusive states
     */
    public IEnumerable<int> InclusiveStates => inclusive;
}
