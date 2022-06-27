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
     * @version JFlex 1.4, $Revision: 2.5 $, $Date: 2004/04/12 10:07:47 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class CharClasses
    {
        /** debug flag (for char classes only) */
        private static readonly bool DEBUG = false;
        /** the largest character that can be used in char classes */
        public const char MaxChar = '\uFFFF';
        public const char MinChar = '\0';
        /** the char classes */
        private readonly List<IntCharSet> classes = new();
        /** the largest character actually used in a specification */
        private char maxCharUsed = '\0';
        /**
         * Returns the greatest Unicode value of the current input character set.
         */
        public char MaxCharCode => maxCharUsed;
        /**
         * Returns the current number of character classes.
         */
        public int NumClasses => this.classes.Count;
        public List<IntCharSet> Classes => classes;
        /**
         * Constructs a new CharClass object that provides space for 
         * classes of characters from 0 to maxCharCode.
         *
         * Initially all characters are in class 0.
         *
         * @param maxCharCode the last character code to be
         *                    considered. (127 for 7bit Lexers, 
         *                    255 for 8bit Lexers and 0xFFFF
         *                    for Unicode Lexers).
         */
        public CharClasses(int maxCharCode)
        {
            if (maxCharCode < 0 || maxCharCode > 0xFFFF)
                throw new ArgumentException(nameof(maxCharCode));

            this.maxCharUsed = (char)maxCharCode;

            this.classes = new PrettyArrayList<IntCharSet>
            {
                new (new Interval(MinChar, MaxChar))
            };
        }

        /**
         * Sets the larges Unicode value of the current input character set.
         *
         * @param charCode   the largest character code, used for the scanner 
         *                   (i.e. %7bit, %8bit, %16bit etc.)
         */
        public void SetMaxCharCode(int charCode)
        {
            if (charCode < 0 || charCode > 0xFFFF)
                throw new ArgumentException(nameof(charCode));

            this.maxCharUsed = (char)charCode;
        }

        /**
         * Updates the current partition, so that the specified set of characters
         * gets a new character class.
         *
         * Characters that are elements of <code>set</code> are not in the same
         * equivalence class with characters that are not elements of <code>set</code>.
         *
         * @param set       the set of characters to distinguish from the rest    
         * @param caseless  if true upper/lower/title case are considered equivalent  
         */
        public void MakeClass(IntCharSet set, bool caseless)
        {
            if (caseless) set = set.GetCaseless();

            if (DEBUG)
            {
                OutputWriter.Dump("makeClass(" + set + ")");
                Dump();
            }

            try
            {
                int oldSize = classes.Count;
                for (int i = 0; i < oldSize; i++)
                {
                    var x = classes[i];

                    if (x.Equals(set)) return;

                    var and = x.And(set);

                    if (and.ContainsElements())
                    {
                        if (x.Equals(and))
                        {
                            set.Sub(and);
                            continue;
                        }
                        else if (set.Equals(and))
                        {
                            x.Sub(and);
                            classes.Add(and);
                            return;
                        }

                        set.Sub(and);
                        x.Sub(and);
                        classes.Add(and);
                    }
                }
            }
            finally
            {
                if (DEBUG)
                {
                    OutputWriter.Dump("makeClass(..) finished");
                    Dump();
                }
            }
        }
        /**
         * Returns the code of the character class the specified character belongs to.
         */
        public int GetClassCode(char letter)
        {
            int i = -1;
            while (true)
            {
                var x = classes[++i];
                if (x.Contains(letter)) return i;
            }
        }
        /**
         * Dump charclasses to the dump output stream
         */
        public void Dump() => OutputWriter.Dump(ToString());
        /**
         * Return a string representation of one char class
         *
         * @param theClass  the index of the class to
         */
        public string ToString(int theClass) => classes[theClass].ToString();
        /**
         * Return a string representation of the char classes
         * stored in this class. 
         *
         * Enumerates the classes by index.
         */
        public override string ToString()
        {
            var result = new StringBuilder("CharClasses:");

            result.Append(OutputWriter.NewLine);

            for (int i = 0; i < classes.Count; i++)
                result.AppendFormat("class {1}:{0}{2}{0}", OutputWriter.NewLine, i, classes[i]);

            return result.ToString();
        }
        /**
         * Creates a new character class for the single character <code>singleChar</code>.
         *    
         * @param caseless  if true upper/lower/title case are considered equivalent  
         */
        public void MakeClass(char singleChar, bool caseless) => MakeClass(new IntCharSet(singleChar), caseless);
        /**
         * Creates a new character class for each character of the specified string.
         *    
         * @param caseless  if true upper/lower/title case are considered equivalent  
         */
        public void MakeClass(string str, bool caseless)
        {
            for (int i = 0; i < str.Length; i++) MakeClass(str[i], caseless);
        }
        /**
         * Updates the current partition, so that the specified set of characters
         * gets a new character class.
         *
         * Characters that are elements of the set <code>v</code> are not in the same
         * equivalence class with characters that are not elements of the set <code>v</code>.
         *
         * @param v   a Vector of Interval objects. 
         *            This Vector represents a set of characters. The set of characters is
         *            the union of all intervalls in the Vector.
         *    
         * @param caseless  if true upper/lower/title case are considered equivalent  
         */
        public void MakeClass(List<Interval> v, bool caseless)
        {
            MakeClass(new IntCharSet(v), caseless);
        }
        /**
         * Updates the current partition, so that the set of all characters not contained in the specified 
         * set of characters gets a new character class.
         *
         * Characters that are elements of the set <code>v</code> are not in the same
         * equivalence class with characters that are not elements of the set <code>v</code>.
         *
         * This method is equivalent to <code>makeClass(v)</code>
         * 
         * @param v   a Vector of Interval objects. 
         *            This Vector represents a set of characters. The set of characters is
         *            the union of all intervalls in the Vector.
         * 
         * @param caseless  if true upper/lower/title case are considered equivalent  
         */
        public void MakeClassNot(List<Interval> v, bool caseless)
        {
            MakeClass(new IntCharSet(v), caseless);
        }
        /**
         * Returns an array that contains the character class codes of all characters
         * in the specified set of input characters.
         */
        private int[] GetClassCodes(IntCharSet set, bool negate)
        {
            if (DEBUG)
            {
                OutputWriter.Dump("getting class codes for " + set);
                if (negate)
                    OutputWriter.Dump("[negated]");
            }

            int size = classes.Count;

            // [fixme: optimize]
            var temp = new int[size];
            int length = 0;

            for (int i = 0; i < size; i++)
            {
                var x = classes[i];
                if (negate)
                {
                    if (!set.And(x).ContainsElements())
                    {
                        temp[length++] = i;
                        if (DEBUG) OutputWriter.Dump("code " + i);
                    }
                }
                else
                {
                    if (set.And(x).ContainsElements())
                    {
                        temp[length++] = i;
                        if (DEBUG) OutputWriter.Dump("code " + i);
                    }
                }
            }

            var result = new int[length];
            Array.Copy(temp, 0, result, 0, length);
            return result;
        }
        /**
         * Returns an array that contains the character class codes of all characters
         * in the specified set of input characters.
         * 
         * @param intervallVec   a Vector of Intervalls, the set of characters to get
         *                       the class codes for
         *
         * @return an array with the class codes for intervallVec
         */
        public int[] GetClassCodes(List<Interval> intervallVec) => GetClassCodes(new IntCharSet(intervallVec), false);
        /**
         * Returns an array that contains the character class codes of all characters
         * that are <strong>not</strong> in the specified set of input characters.
         * 
         * @param intervallVec   a Vector of Intervalls, the complement of the
         *                       set of characters to get the class codes for
         *
         * @return an array with the class codes for the complement of intervallVec
         */
        public int[] GetNotClassCodes(List<Interval> intervallVec) => GetClassCodes(new IntCharSet(intervallVec), true);
        /**
         * Check consistency of the stored classes [debug].
         *
         * all classes must be disjoint, checks if all characters
         * have a class assigned.
         */
        public void Check()
        {
            for (int i = 0; i < classes.Count; i++)
                for (int j = i + 1; j < classes.Count; j++)
                {
                    var x = classes[i];
                    var y = classes[j];
                    if (x.And(y).ContainsElements())
                    {
                        Console.WriteLine("Error: non disjoint char classes {0} and {1}", i, j);
                        Console.WriteLine("class {0}: {1}", i, x);
                        Console.WriteLine("class {0}: {1}", j, y);
                    }
                }

            // check if each character has a classcode 
            // (= if getClassCode terminates)
            for (char c = MinChar; c < MaxChar; c++)
            {
                GetClassCode(c);
                if (c % 100 == 0) Console.Write(".");
            }

            GetClassCode(MaxChar);
        }
        /**
         * Returns an array of all CharClassIntervalls in this
         * char class collection. 
         *
         * The array is ordered by char code, i.e.
         * <code>result[i+1].start = result[i].end+1</code>
         *
         * Each CharClassInterval contains the number of the
         * char class it belongs to.
         */
        public CharClassInterval[] GetIntervalls()
        {
            int size = classes.Count;
            int numIntervalls = 0;

            int i;
            for (i = 0; i < size; i++)
                numIntervalls += classes[i].NumIntervalls();

            var result = new CharClassInterval[numIntervalls];

            i = 0;
            int c = 0;
            while (i < numIntervalls)
            {
                int code = GetClassCode((char)c);
                var set = classes[code];
                var iv = set.GetNext();

                result[i++] = new (iv.Start, iv.End, code);
                c = iv.End + 1;
            }
            return result;
        }
    }
}
