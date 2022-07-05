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
 * A simple table to store EOF actions for each lexical state.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public class EOFActions
{

    /** maps lexical states to actions */
    private PrettyDictionary<int, Action> /* Integer -> Action */ actions = new();
    private Action? defaultAction = null;
    private int numLexStates = 0;

    public void SetNumLexStates(int num)
    {
        numLexStates = num;
    }

    public void Add(List<int> stateList, Action action)
    {

        if (stateList != null && stateList.Count > 0)
        {
            foreach(var state in stateList)
            {
                this.Add(state, action);
            }
        }
        else
        {
            defaultAction = action.GetHigherPriority(defaultAction!);

            for (int i = 0; i < numLexStates; i++)
            {
                int state = i;
                if (actions[state] != null)
                {
                    Action oldAction = (Action)actions[state];
                    actions[state] = oldAction.GetHigherPriority(action);
                }
            }
        }
    }

    public void Add(int state, Action action)
    {
        if (actions[state] == null)
            actions[state] = action;
        else
        {
            var oldAction = actions[state];
            actions[state] = oldAction.GetHigherPriority(action);
        }
    }

    public bool IsEOFAction(Action a)
    {
        if (a == defaultAction) return true;

        return actions.ContainsValue(a);
    }

    public Action GetAction(int state) => actions[state];

    public Action? Default => defaultAction;

    public int NumActions => actions.Count;
}