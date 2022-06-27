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
    using System.Text;

    /** 
     * CharSet implemented with intervalls
     *
     * [fixme: optimizations possible]
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.6 $, $Date: 2004/04/12 10:07:47 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public sealed class IntCharSet
    {

        private static readonly bool DEBUG = false;

        /* invariant: all intervals are disjoint, ordered */
        private List<Interval> intervals = new();
        private int pos = 0;

        public IntCharSet() { }
        public IntCharSet(char c) : this(new Interval(c, c)) { }
        public IntCharSet(Interval intervall) : this()
        {
            intervals.Add(intervall);
        }

        public IntCharSet(List<Interval> chars)
        {
            int size = chars.Count;

            this.intervals = new PrettyArrayList<Interval>(size);

            for (int i = 0; i < size; i++)
                Add((Interval)chars[i]);
        }

        /**
         * returns the index of the intervall that contains
         * the character c, -1 if there is no such intevall
         *
         * @prec: true
         * @post: -1 <= return < intervalls.size() && 
         *        (return > -1 --> intervalls[return].contains(c))
         * 
         * @param c  the character
         * @return the index of the enclosing interval, -1 if no such interval  
         */
        private int IndexOf(char c)
        {
            int start = 0;
            int end = intervals.Count - 1;

            while (start <= end)
            {
                int check = (start + end) / 2;
                var i = intervals[check];

                if (start == end)
                    return i.Contains(c) ? start : -1;

                if (c < i.Start)
                {
                    end = check - 1;
                    continue;
                }

                if (c > i.End)
                {
                    start = check + 1;
                    continue;
                }

                return check;
            }

            return -1;
        }

        public IntCharSet Add(IntCharSet set)
        {
            for (int i = 0; i < set.intervals.Count; i++)
                Add(set.intervals[i]);
            return this;
        }

        public void Add(Interval interval)
        {
            int size = intervals.Count;

            for (int i = 0; i < size; i++)
            {
                var elem = intervals[i];

                if (elem.End + 1 < interval.Start) continue;

                if (elem.Contains(interval)) return;

                if (elem.Start > interval.End + 1)
                {
                    intervals.Insert(i, new (interval));
                    return;
                }

                if (interval.Start < elem.Start)
                    elem.Start = interval.Start;

                if (interval.End <= elem.End)
                    return;

                elem.End = interval.End;

                i++;
                // delete all x with x.contains( intervall.end )
                while (i < size)
                {
                    var x = intervals[i];
                    if (x.Start > elem.End + 1) return;

                    elem.End = x.End;
                    intervals.RemoveAt(i);
                    size--;
                }
                return;
            }

            intervals.Add(new (interval));
        }

        public void Add(char c)
        {
            int size = intervals.Count;

            for (int i = 0; i < size; i++)
            {
                var elem = intervals[i];
                if (elem.End + 1 < c) continue;

                if (elem.Contains(c)) return; // already there, nothing to do

                // assert(elem.end+1 >= c && (elem.start > c || elem.end < c));

                if (elem.Start > c + 1)
                {
                    intervals.Insert(i, new Interval(c, c));
                    return;
                }

                // assert(elem.end+1 >= c && elem.start <= c+1 && (elem.start > c || elem.end < c));

                if (c + 1 == elem.Start)
                {
                    elem.Start = c;
                    return;
                }

                // assert(elem.end+1 == c);
                elem.End = c;

                // merge with next interval if it contains c
                if (i >= size) return;
                var x = intervals[i + 1];
                if (x.Start <= c + 1)
                {
                    elem.End = x.End;
                    intervals.RemoveAt(i + 1);
                }
                return;
            }

            // end reached but nothing found -> append at end
            intervals.Add(new (c, c));
        }

        public bool Contains(char singleChar) => IndexOf(singleChar) >= 0;

        /**
         * prec: intervall != null
         */
        public bool Contains(Interval intervall)
        {
            int index = IndexOf(intervall.Start);
            if (index < 0) return false;
            return intervals[index].Contains(intervall);
        }

        public bool Contains(IntCharSet set)
        {
            /*
                IntCharSet test = set.copy();

                test.sub(this);

                return (test.numIntervalls() == 0);
            /*/
            int i = 0;
            int j = 0;

            while (j < set.intervals.Count)
            {
                Interval x = intervals[i];
                Interval y = set.intervals[j];

                if (x.Contains(y)) j++;

                if (x.Start > y.End) return false;
                if (x.End < y.Start) i++;
            }

            return true; /* */
        }


        /**
         * o instanceof Interval
         */
        public override bool Equals(object? o)
        {
            //IntCharSet set = (IntCharSet)o;
            if (o is not IntCharSet set|| intervals.Count != set.intervals.Count) return false;

            for (int i = 0; i < intervals.Count; i++)
            {
                if (!intervals[i].Equals(set.intervals[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < intervals.Count; i++)
            {
                var elem = intervals[i];

                ushort start = (ushort)elem.Start;
                ushort end = (ushort)elem.End;

                hash ^= unchecked((int)((end << 16) | start));
            }

            return hash;
        }


        private static char Min(char a, char b) => a <= b ? a : b;

        private static char Max(char a, char b) => a >= b ? a : b;

        /* intersection */
        public IntCharSet And(IntCharSet set)
        {
            if (DEBUG)
            {
                OutputWriter.Dump("intersection");
                OutputWriter.Dump("this  : " + this);
                OutputWriter.Dump("other : " + set);
            }

            var result = new IntCharSet();

            int i = 0;  // index in this.intervalls
            int j = 0;  // index in set.intervalls

            int size = intervals.Count;
            int setSize = set.intervals.Count;

            while (i < size && j < setSize)
            {
                var x = this.intervals[i];
                var y = set.intervals[j];

                if (x.End < y.Start)
                {
                    i++;
                    continue;
                }

                if (y.End < x.Start)
                {
                    j++;
                    continue;
                }

                result.intervals.Add(
                  new (
                    Max(x.Start, y.Start),
                    Min(x.End, y.End)
                    )
                  );

                if (x.End >= y.End) j++;
                if (y.End >= x.End) i++;
            }

            if (DEBUG)
            {
                OutputWriter.Dump("result: " + result);
            }

            return result;
        }

        /* complement */
        /* prec: this.contains(set), set != null */
        public void Sub(IntCharSet set)
        {
            if (DEBUG)
            {
                OutputWriter.Dump("complement");
                OutputWriter.Dump("this  : " + this);
                OutputWriter.Dump("other : " + set);
            }

            int i = 0;  // index in this.intervalls
            int j = 0;  // index in set.intervalls

            int setSize = set.intervals.Count;

            while (i < intervals.Count && j < setSize)
            {
                var x = this.intervals[i];
                var y = set.intervals[j];

                if (DEBUG)
                {
                    OutputWriter.Dump("this      : " + this);
                    OutputWriter.Dump("this  [" + i + "] : " + x);
                    OutputWriter.Dump("other [" + j + "] : " + y);
                }

                if (x.End < y.Start)
                {
                    i++;
                    continue;
                }

                if (y.End < x.Start)
                {
                    j++;
                    continue;
                }

                // x.end >= y.start && y.end >= x.start ->
                // x.end <= y.end && x.start >= y.start (prec)

                if (x.Start == y.Start && x.End == y.End)
                {
                    intervals.RemoveAt(i);
                    j++;
                    continue;
                }

                // x.end <= y.end && x.start >= y.start &&
                // (x.end < y.end || x.start > y.start) ->
                // x.start < x.end 

                if (x.Start == y.Start)
                {
                    x.Start = (char)(y.End + 1);
                    j++;
                    continue;
                }

                if (x.End == y.End)
                {
                    x.End = (char)(y.Start - 1);
                    i++;
                    j++;
                    continue;
                }

                intervals.Insert(i, new Interval(x.Start, (char)(y.Start - 1)));
                x.Start = (char)(y.End + 1);

                i++;
                j++;
            }

            if (DEBUG)
            {
                OutputWriter.Dump("result: " + this);
            }
        }

        public bool ContainsElements() => intervals.Count > 0;

        public int NumIntervalls() => intervals.Count;

        // beware: depends on caller protocol, single user only 
        public Interval GetNext()
        {
            if (pos == intervals.Count) pos = 0;
            return intervals[pos++];
        }

        /**
         * Create a caseless version of this charset.
         * <p>
         * The caseless version contains all characters of this char set,
         * and additionally all lower/upper/title case variants of the 
         * characters in this set.
         * 
         * @return a caseless copy of this set
         */
        public IntCharSet GetCaseless()
        {
            var n = Copy();

            int size = intervals.Count;
            for (int i = 0; i < size; i++)
            {
                var elem = intervals[i];
                for (char c = elem.Start; c <= elem.End; c++)
                {
                    n.Add(char.ToLower(c));
                    n.Add(char.ToUpper(c));
                    //n.add(char.toTitleCase(c)); 
                }
            }

            return n;
        }


        /**
         * Make a string representation of this char set.
         * 
         * @return a string representing this char set.
         */
        public override string ToString()
        {
            var result = new StringBuilder("{ ");

            for (int i = 0; i < intervals.Count; i++)
                result.Append(intervals[i]);

            result.Append(" }");

            return result.ToString();
        }


        /** 
         * Return a (deep) copy of this char set
         * 
         * @return the copy
         */
        public IntCharSet Copy()
        {
            var result = new IntCharSet();
            int size = intervals.Count;
            for (int i = 0; i < size; i++)
            {
                result.intervals.Add(
                    intervals[i].Copy()
                    );
            }
            return result;
        }
    }
}
