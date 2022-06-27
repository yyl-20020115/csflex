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
namespace CSFlex
{
    /**
     * Stores all rules of the specification for later access in RegExp -> NFA
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class RegExps
    {

        /** the spec line in which a regexp is used */
        private readonly PrettyArrayList<int> lines = new();

        /** the lexical states in wich the regexp is used */
        private readonly PrettyArrayList<List<int>> states = new();

        /** the regexp */
        private readonly PrettyArrayList<RegExp?> regExps = new();

        /** the action of a regexp */
        private readonly PrettyArrayList<Action> actions = new();

        /** flag if it is a BOL regexp */
        private readonly PrettyArrayList<bool> BOL = new();

        /** the lookahead expression */
        private readonly PrettyArrayList<RegExp?>  look = new();

        public RegExps()
        {
        }

        public int Insert(int line, List<int> stateList, RegExp regExp, Action action,
                           bool isBOL, RegExp lookAhead)
        {
            if (Options.DEBUG)
            {
                OutputWriter.Debug("Inserting regular expression with statelist :" + OutputWriter.NewLine + stateList);  //$NON-NLS-1$
                OutputWriter.Debug("and action code :" + OutputWriter.NewLine + action.Content + OutputWriter.NewLine);     //$NON-NLS-1$
                OutputWriter.Debug("expression :" + OutputWriter.NewLine + regExp);  //$NON-NLS-1$
            }

            states.Add(stateList);
            regExps.Add(regExp);
            actions.Add(action);
            BOL.Add(isBOL);
            look.Add(lookAhead);
            lines.Add(line);

            return states.Count - 1;
        }

        public int Insert(List<int> stateList, Action action)
        {
            if (Options.DEBUG)
            {
                OutputWriter.Debug("Inserting eofrule with statelist :" + OutputWriter.NewLine + stateList);   //$NON-NLS-1$
                OutputWriter.Debug("and action code :" + OutputWriter.NewLine + action.Content + OutputWriter.NewLine);      //$NON-NLS-1$
            }

            states.Add(stateList);
            regExps.Add(null);
            actions.Add(action);
            BOL.Add(false);
            look.Add(null);
            lines.Add(0);

            return states.Count - 1;
        }

        public void AddStates(int regNum, List<int> newStates)
        {
            var s = newStates.GetEnumerator();

            while (s.MoveNext())
                (states[regNum]).Add(s.Current);
        }

        public int Num => states.Count;

        public bool IsBOL(int num) => BOL[num];

        public RegExp? GetLookAhead(int num) => look[num];

        public bool IsEOF(int num) => num == 0;// && num < BOL.Count;// BOL[num] == null;

        public List<int> GetStates(int num) => states[num];

        public RegExp? GetRegExp(int num) => regExps[num];

        public int GetLine(int num) => lines[num];

        public void CheckActions()
        {
            if (actions[actions.Count - 1] == null)
            {
                OutputWriter.Error(ErrorMessages.NO_LAST_ACTION);
                throw new GeneratorException();
            }
        }

        public Action GetAction(int num)
        {
            while (num < actions.Count && actions[num] == null)
                num++;

            return actions[num];
        }

        public int NFASize(Macros macros)
        {
            int size = 0;
            var e = regExps.GetEnumerator();
            while (e.MoveNext())
            {
                var r = e.Current;
                if (r != null) size += r.Size(macros);
            }
            e = look.GetEnumerator();
            while (e.MoveNext())
            {
                var r = e.Current;
                if (r != null) size += r.Size(macros);
            }
            return size;
        }
    }
}
