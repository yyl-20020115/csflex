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
 * Simple pair of integers.
 *
 * Used in NFA to represent a partial NFA by its start and end state.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class IntPair
{
    private int start;
    private int end;
    public int Start => start;
    public int End => end;
    public IntPair(int start, int end)
    {
        this.start = start;
        this.end = end;
    }

    public override int GetHashCode() => end + (start << 8);
    public override bool Equals(object? o) => o is IntPair p && start == p.start && end == p.end;
    public override string ToString() => $"({start},{end})";
}
