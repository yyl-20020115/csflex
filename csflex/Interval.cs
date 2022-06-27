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

using System.Globalization;
using System.Text;

namespace CSFlex
{
    /**
     * An intervall of characters with basic operations.
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:47 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public sealed class Interval
    {
        /* start and end of the intervall */
        private char start;
        private char end;

        public char Start { get => this.start; set => this.start = value; }
        public char End { get => this.end; set => this.end = value; }

        /**
         * Constuct a new intervall from <code>start</code> to <code>end</code>.
         *
         * @param start  first character the intervall should contain
         * @param end    last  character the intervall should contain
         */
        public Interval(char start, char end)
        {
            this.start = start;
            this.end = end;
        }


        /**
         * Copy constructor
         */
        public Interval(Interval other)
        {
            this.start = other.start;
            this.end = other.end;
        }


        /**
         * Return <code>true</code> iff <code>point</code> is contained in this intervall.
         *
         * @param point  the character to check
         */
        public bool Contains(char point) => start <= point && end >= point;


        /**
         * Return <code>true</code> iff this intervall completely contains the 
         * other one.
         *
         * @param other    the other intervall 
         */
        public bool Contains(Interval other) => this.start <= other.start && this.end >= other.end;


        /**
         * Return <code>true</code> if <code>o</code> is an intervall
         * with the same borders.
         *
         * @param o  the object to check equality with
         */
        public override bool Equals(object? o) => o == this || o is Interval other && other.start == this.start && other.end == this.end;

        public override int GetHashCode() => unchecked((end << 16) | (ushort)start);

        // The set of Unicode character categories containing non-rendering,
        // unknown, or incomplete characters.
        // !! Unicode.Format and Unicode.PrivateUse can NOT be included in
        // !! this set, because they may (private-use) or do (format)
        // !! contain at least *some* rendering characters.
        public static readonly UnicodeCategory[] nonRenderingCategories = new [] {
            UnicodeCategory.Control,
            UnicodeCategory.OtherNotAssigned,
            UnicodeCategory.Surrogate };

        // Char.IsWhiteSpace() includes the ASCII whitespace characters that
        // are categorized as control characters. Any other character is
        // printable, unless it falls into the non-rendering categories.
        public static bool IsPrintable(char c) => char.IsWhiteSpace(c) ||
          !nonRenderingCategories.Contains(char.GetUnicodeCategory(c));

        /**
         * Get a string representation of this intervall.
         *
         * @return a string <code>"[start-end]"</code> or
         *         <code>"[start]"</code> (if there is only one character in
         *         the intervall) where <code>start</code> and
         *         <code>end</code> are either a number (the character code)
         *         or something of the from <code>'a'</code>.  
         */
        public override string ToString()
        {
            var result = new StringBuilder("[");

            if (IsPrintable(start))
                result.Append("'").Append(start).Append("'");
            else
                result.Append((int)start);

            if (start != end)
            {
                result.Append("-");

                if (IsPrintable(end))
                    result.Append("'").Append(end).Append("'");
                else
                    result.Append((int)end);
            }

            result.Append("]");
            return result.ToString();
        }

        /**
         * Make a copy of this interval.
         * 
         * @return the copy
         */
        public Interval Copy() => new (start, end);
    }
}
