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
using System.Text;

namespace CSFlex
{
    /**
     * 
     * @author Gerwin Klein 
     * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:47 $ 
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public sealed class CharSet
    {
        public const int BITS = 6;           // the number of bits to shift (2^6 = 64)
        public const int MOD = (1 << BITS) - 1;  // modulus
        private int numElements = 0;
        private long[] bits;
        public long[] Bits => bits;
        public int NumElements => numElements;

        public CharSet()
        {
            this.bits = new long[1];
        }
        public CharSet(int initialSize, int character)
        {
            this.bits = new long[(initialSize >> BITS) + 1];
            this.Add(character);
        }

        public void Add(int character)
        {
            this.Resize(character);
            if ((bits[character >> BITS] & (1L << (character & MOD))) == 0) numElements++;
            this.bits[character >> BITS] |= (1L << (character & MOD));
        }

        private int NBitsToSize(int nbits) => ((nbits >> BITS) + 1);

        private void Resize(int nbits)
        {
            var needed = NBitsToSize(nbits);
            if (needed < bits.Length) return;
            var newbits = new long[Math.Max(bits.Length * 2, needed)];
            Array.Copy(bits, 0, newbits, 0, bits.Length);
            this.bits = newbits;
        }

        public bool IsElement(int character)
        {
            int index = character >> BITS;
            if (index >= bits.Length) return false;
            return (bits[index] & (1L << (character & MOD))) != 0;
        }
        public CharSetEnumerator GetCharacters() => new (this);
        public bool ContainsElements => numElements > 0;
        public int Size => this.numElements;
        public override string ToString()
        {
            var e = GetCharacters();
            var result = new StringBuilder("{");
            if (e.HasMoreElements) result.Append(e.NextElement());
            while (e.HasMoreElements)
            {
                var i = e.NextElement();
                result.Append(", ").Append(i);
            }
            result.Append("}");
            return result.ToString();
        }
    }
}
