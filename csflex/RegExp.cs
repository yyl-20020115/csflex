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
 * Stores a regular expression of rules section in a C# Flex specification.
 *
 * This base class has no content other than its type. 
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.3 $, $Date: 2004/04/12 10:07:47 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public class RegExp
{

    /**
     * The type of the regular expression. This field will be
     * filled with values from class sym.java (generated by cup)
     */
    internal int type;


    /**
     * Create a new regular expression of the specified type.
     *
     * @param type   a value from the cup generated class sym.
     *
     * @see CSFlex.sym
     */
    public RegExp(int type)
    {
        this.type = type;
    }

    /**
     * Returns a string-representation of this regular expression
     * with the specified indentation.
     *
     * @param tab   a string that should contain only space characters and
     *              that is inserted in front of standard string-representation
     *              pf this object.
     */
    public virtual string Print(string tab) => tab + ToString();

    /**
     * Returns a string-representation of this regular expression
     */
    public override string ToString() => "type = " + type;

    /**
     * Find out if this regexp is a char class or equivalent to one.
     * 
     * @param  macros  for macro expansion
     * @return true if the regexp is equivalent to a char class.
     */
    public bool IsCharClass(Macros macros)
    {
        switch (type)
        {
            case Symbols.CHAR:
            case Symbols.CHAR_I:
            case Symbols.CCLASS:
            case Symbols.CCLASSNOT:
                return true;

            case Symbols.BAR:
                return (this is RegExp2 _binary)
                    && _binary.r1.IsCharClass(macros)
                    && _binary.r2.IsCharClass(macros);

            case Symbols.MACROUSE:
                return (this is RegExp1 _unary)
                    && macros.GetDefinition((string)_unary.content).IsCharClass(macros);

            default: return false;
        }
    }

    /**
     * The approximate number of NFA states this expression will need (only 
     * works correctly after macro expansion and without negation)
     * 
     * @param macros  macro table for expansion   
     */
    public int Size(Macros macros)
    {
        RegExp content;

        switch (type)
        {
            case Symbols.BAR:
                return (this is RegExp2 binary1) ? binary1.r1.Size(macros) + binary1.r2.Size(macros) + 2 : 0;

            case Symbols.CONCAT:
                return (this is RegExp2 binary2) ? binary2.r1.Size(macros) + binary2.r2.Size(macros) : 0;

            case Symbols.STAR:
                return (this is RegExp1 unary && unary.content is RegExp r) ? r.Size(macros) + 2 : 0;

            case Symbols.PLUS:
                unary = (RegExp1)this;
                content = (RegExp)unary.content;
                return content.Size(macros) + 2;

            case Symbols.QUESTION:
                unary = (RegExp1)this;
                content = (RegExp)unary.content;
                return content.Size(macros);

            case Symbols.BANG:
                unary = (RegExp1)this;
                content = (RegExp)unary.content;
                return content.Size(macros) * content.Size(macros);
            // this is only a very rough estimate (worst case 2^n)
            // exact size too complicated (propably requires construction)

            case Symbols.TILDE:
                unary = (RegExp1)this;
                content = (RegExp)unary.content;
                return content.Size(macros) * content.Size(macros) * 3;
            // see sym.BANG

            case Symbols.STRING:
            case Symbols.STRING_I:
                unary = (RegExp1)this;
                return ((string)unary.content).Length + 1;

            case Symbols.CHAR:
            case Symbols.CHAR_I:
                return 2;

            case Symbols.CCLASS:
            case Symbols.CCLASSNOT:
                return 2;

            case Symbols.MACROUSE:
                unary = (RegExp1)this;
                return macros.GetDefinition((string)unary.content).Size(macros);
        }

        throw new Exception("unknown regexp type " + type);
    }
}
