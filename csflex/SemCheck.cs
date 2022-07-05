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
 * Performs simple semantic analysis on regular expressions.
 *
 * (used for checking if trailing contexts are legal)
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class SemCheck
{

    // stored globally since they are used as constants in all checks
    private static Macros macros = new();
    private static char maxChar = '\0';


    /**
     * Performs semantic analysis for all expressions.
     *
     * Currently: illegal lookahead check only
     * [fixme: more checks possible]
     *
     * @param rs   the reg exps to be checked
     * @param m    the macro table (in expanded form)
     * @param max  max character of the used charset (for negation)
     * @param f    the spec file containing the rules [fixme]
     */
    public static void Check(RegExps rs, Macros m, char max, File f)
    {
        macros = m;
        maxChar = max;

        bool errors = false;
        int num = rs.Num;
        for (int i = 0; i < num; i++)
        {
            var r = rs.GetRegExp(i);
            var l = rs.GetLookAhead(i);

            if (!CheckLookAhead(r, l))
            {
                errors = true;
                OutputWriter.Error(f, ErrorMessages.LOOKAHEAD_ERROR, rs.GetLine(i), -1);
            }
        }

        if (errors) throw new GeneratorException();
    }


    /**
     * Checks for illegal lookahead expressions. 
     * 
     * Lookahead in C# Flex only works when the first expression has fixed
     * length or when the intersection of the last set of the first expression
     * and the first set of the second expression is empty.
     *
     * @param r1   first regexp
     * @param r2   second regexp (the lookahead)
     *
     * @return true iff C# Flex can generate code for the lookahead expression
     */
    private static bool CheckLookAhead(RegExp? r1, RegExp? r2)
        => r2 == null || Length(r1) > 0 || !(Last(r1).And(First(r2)).ContainsElements());


    /**
     * Returns length if expression has fixed length, -1 otherwise.   
     */
    private static int Length(RegExp? re)
    {
        if (re == null) return 0;

        switch (re.type)
        {

            case Symbols.BAR:
                {
                    var r = (re as RegExp2)!;
                    int l1 = Length(r.r1);
                    if (l1 < 0) return -1;
                    int l2 = Length(r.r2);

                    if (l1 == l2)
                        return l1;
                    else
                        return -1;
                }

            case Symbols.CONCAT:
                {
                    var r = (re as RegExp2)!;
                    int l1 = Length(r.r1);
                    if (l1 < 0) return -1;
                    int l2 = Length(r.r2);
                    if (l2 < 0) return -1;
                    return l1 + l2;
                }

            case Symbols.STAR:
            case Symbols.PLUS:
            case Symbols.QUESTION:
                return -1;

            case Symbols.CCLASS:
            case Symbols.CCLASSNOT:
            case Symbols.CHAR:
                return 1;

            case Symbols.STRING:
                {
                    string content = ((RegExp1)re).content is string s?s:"";
                    return content.Length;
                }

            case Symbols.MACROUSE:
                return Length(macros.GetDefinition(((RegExp1)re).content is string s2?s2:""));
        }

        throw new Exception("Unkown expression type " + re.type + " in " + re);   //$NON-NLS-1$ //$NON-NLS-2$
    }


    /**
     * Returns true iff the matched language contains epsilon
     */
    private static bool ContainsEpsilon(RegExp re)
    {
        RegExp2 r;

        switch (re.type)
        {

            case Symbols.BAR:
                r = (RegExp2)re;
                return ContainsEpsilon(r.r1) || ContainsEpsilon(r.r2);

            case Symbols.CONCAT:
                r = (RegExp2)re;
                if (ContainsEpsilon(r.r1))
                    return ContainsEpsilon(r.r2);
                else
                    return false;

            case Symbols.STAR:
            case Symbols.QUESTION:
                return true;

            case Symbols.PLUS:
                return ContainsEpsilon((RegExp)((RegExp1)re).content);

            case Symbols.CCLASS:
            case Symbols.CCLASSNOT:
            case Symbols.CHAR:
                return false;

            case Symbols.STRING:
                return ((string)((RegExp1)re).content).Length <= 0;

            case Symbols.MACROUSE:
                return ContainsEpsilon(macros.GetDefinition((string)((RegExp1)re).content));
        }

        throw new Exception("Unkown expression type " + re.type + " in " + re); //$NON-NLS-1$ //$NON-NLS-2$
    }


    /**
     * Returns the first set of an expression. 
     *
     * (the first-character-projection of the language)
     */
    private static IntCharSet First(RegExp re)
    {
        RegExp2 r;

        switch (re.type)
        {

            case Symbols.BAR:
                r = (RegExp2)re;
                return First(r.r1).Add(First(r.r2));

            case Symbols.CONCAT:
                r = (RegExp2)re;
                if (ContainsEpsilon(r.r1))
                    return First(r.r1).Add(First(r.r2));
                else
                    return First(r.r1);

            case Symbols.STAR:
            case Symbols.PLUS:
            case Symbols.QUESTION:
                return First((RegExp)((RegExp1)re).content);

            case Symbols.CCLASS:
                return new IntCharSet((List<Interval>)((RegExp1)re).content);

            case Symbols.CCLASSNOT:
                IntCharSet all = new IntCharSet(new Interval((char)0, maxChar));
                IntCharSet set = new IntCharSet((List<Interval>)((RegExp1)re).content);
                all.Sub(set);
                return all;

            case Symbols.CHAR:
                return new IntCharSet((char)((RegExp1)re).content);

            case Symbols.STRING:
                string content = (string)((RegExp1)re).content;
                if (content.Length > 0)
                    return new IntCharSet(content[0]);
                else
                    return new IntCharSet();

            case Symbols.MACROUSE:
                return First(macros.GetDefinition((string)((RegExp1)re).content));
        }

        throw new Exception("Unkown expression type " + re.type + " in " + re); //$NON-NLS-1$ //$NON-NLS-2$
    }


    /**
     * Returns the last set of the expression
     *
     * (the last-charater-projection of the language)
     */
    private static IntCharSet Last(RegExp? re)
    {
        switch (re!.type)
        {

            case Symbols.BAR:
                {
                    var r = (re as RegExp2)!;
                    return Last(r.r1).Add(Last(r.r2));
                }
            case Symbols.CONCAT:
                {
                    var r = (re as RegExp2)!;
                    if (ContainsEpsilon(r.r2))
                        return Last(r.r1).Add(Last(r.r2));
                    else
                        return Last(r.r2);
                }
            case Symbols.STAR:
            case Symbols.PLUS:
            case Symbols.QUESTION:
                return Last((RegExp)((RegExp1)re).content);

            case Symbols.CCLASS:
                return new IntCharSet((List<Interval>)((RegExp1)re).content);

            case Symbols.CCLASSNOT:
                var all = new IntCharSet(new Interval((char)0, maxChar));
                var set = new IntCharSet((List<Interval>)((RegExp1)re).content);
                all.Sub(set);
                return all;

            case Symbols.CHAR:
                return new IntCharSet((char)((RegExp1)re).content);

            case Symbols.STRING:
                string content = (string)((RegExp1)re).content;
                if (content.Length > 0)
                    return new IntCharSet(content[content.Length - 1]);
                else
                    return new IntCharSet();

            case Symbols.MACROUSE:
                return Last(macros.GetDefinition((string)((RegExp1)re).content));
        }

        throw new Exception("Unkown expression type " + re.type + " in " + re); //$NON-NLS-1$ //$NON-NLS-2$
    }
}