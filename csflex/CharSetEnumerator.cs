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
 * Enumerator for the elements of a CharSet.
 *
 * Does not implement java.util.Enumeration, but supports the same protocol.
 *  
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.2 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class CharSetEnumerator
{
    private int index;
    private int offset;
    private long mask = 1;
    private CharSet set;
    public CharSetEnumerator(CharSet characters)
    {
        set = characters ?? new();

        while (index < set.Bits.Length && set.Bits[index] == 0)
            index++;

        if (index >= set.Bits.Length) return;

        while (offset <= CharSet.MOD && ((set.Bits[index] & mask) == 0))
        {
            mask <<= 1;
            offset++;
        }
    }

    private void Advance()
    {
        do
        {
            offset++;
            mask <<= 1;
        } while (offset <= CharSet.MOD && ((set.Bits[index] & mask) == 0));

        if (offset > CharSet.MOD)
        {
            do
                index++;
            while (index < set.Bits.Length && set.Bits[index] == 0);
            if (index >= set.Bits.Length) return;
            offset = 0;
            mask = 1;
            while (offset <= CharSet.MOD && ((set.Bits[index] & mask) == 0))
            {
                mask <<= 1;
                offset++;
            }
        }
    }

    public bool HasMoreElements => index < set.Bits.Length;
    public int NextElement()
    {
        int x = (index << CharSet.BITS) + offset;
        Advance();
        return x;
    }
}
