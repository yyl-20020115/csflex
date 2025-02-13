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
 * A list of pairs of states. Used in DFA minimization.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class StatePairList
{

    // implemented as two arrays of integers.
    // java.util classes proved too inefficient.

    private int[] p;
    private int[] q;
    private int num;
    public StatePairList()
    {
        p = new int[8];
        q = new int[8];
        num = 0;
    }

    public void AddPair(int i, int j)
    {
        for (int x = 0; x < num; x++)
            if (p[x] == i && q[x] == j) return;

        if (num >= p.Length) IncreaseSize(num);

        p[num] = i;
        q[num] = j;

        num++;
    }

    public void MarkAll(StatePairList[][] list, bool[][] equiv)
    {
        for (int x = 0; x < num; x++)
        {
            int i = p[x];
            int j = q[x];

            if (equiv[i][j])
            {
                equiv[i][j] = false;
                if (list[i][j] != null)
                    list[i][j].MarkAll(list, equiv);
            }
        }
    }

    private void IncreaseSize(int length)
    {
        length = Math.Max(length + 1, 4 * p.Length);
        OutputWriter.Debug("increasing length to " + length); //$NON-NLS-1$

        int[] pn = new int[length];
        int[] qn = new int[length];

        Array.Copy(p, 0, pn, 0, p.Length);
        Array.Copy(q, 0, qn, 0, q.Length);

        p = pn;
        q = qn;
    }
}
