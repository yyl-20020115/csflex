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
 * Enumerates the states of a StateSet.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.2 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class StateSetEnumerator
{

    private readonly bool DEBUG = false;

    private int index = 0;
    private int offset = 0;
    private long mask = 0;

    //private int current;
    private long[] bits = Array.Empty<long>();

    /**
     * creates a new StateSetEnumerator that is not yet associated
     * with a StateSet. hasMoreElements() and nextElement() will 
     * throw NullPointerException when used before reset()
     */
    public StateSetEnumerator()
    {
    }

    public StateSetEnumerator(StateSet states)
    {
        Reset(states);
    }

    public void Reset(StateSet states)
    {
        bits = states.Bits;
        index = 0;
        offset = 0;
        mask = 1;
        //current = 0;

        while (index < bits.Length && bits[index] == 0)
            index++;

        if (index >= bits.Length) return;

        while (offset <= StateSet.MASK && ((bits[index] & mask) == 0))
        {
            mask <<= 1;
            offset++;
        }
    }

    private void Advance()
    {
        if (DEBUG) OutputWriter.Dump("Advancing, at start, index = " + index + ", offset = " + offset); //$NON-NLS-1$ //$NON-NLS-2$

        // cache fields in local variable for faster access
        int _index = this.index;
        int _offset = this.offset;
        long _mask = this.mask;
        long[] _bits = this.bits;

        long bi = _bits[_index];

        do
        {
            _offset++;
            _mask <<= 1;
        } while (_offset <= StateSet.MASK && ((bi & _mask) == 0));

        if (_offset > StateSet.MASK)
        {
            int length = _bits.Length;

            do
                _index++;
            while (_index < length && _bits[_index] == 0);

            if (_index >= length)
            {
                this.index = length; // indicates "no more elements"
                return;
            }

            _offset = 0;
            _mask = 1;
            bi = _bits[_index];

            // terminates, because bi != 0
            while ((bi & _mask) == 0)
            {
                _mask <<= 1;
                _offset++;
            }
        }

        // write back cached values
        this.index = _index;
        this.mask = _mask;
        this.offset = _offset;
    }

    public bool HasMoreElements
    {
        get
        {
            if (DEBUG) OutputWriter.Dump("hasMoreElements, index = " + index + ", offset = " + offset); //$NON-NLS-1$ //$NON-NLS-2$
            return index < bits.Length;
        }
    }

    public int NextElement()
    {
        if (DEBUG) OutputWriter.Dump("nextElement, index = " + index + ", offset = " + offset); //$NON-NLS-1$ //$NON-NLS-2$
        int x = (index << StateSet.BITS) + offset;
        Advance();
        return x;
    }
}