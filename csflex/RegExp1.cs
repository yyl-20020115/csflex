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

using System;

namespace CSFlex
{
    /**
     * Stores a regular expression from the rules section of a C# Flex specification.
     *
     * This class provides storage for one Object of content.
     * It is used for all regular expressions that are constructed from one object.
     * 
     * For instance:  a*  is new RegExp1(sym.STAR, new Character ('a'));
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.1 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class RegExp1 : RegExp
    {

        /**
         * The child of this expression node in the syntax tree of a regular expression.
         */
        internal object content;


        /**
         * Constructs a new regular expression with one child object.
         *
         * @param type   a value from the cup generated class sym, defining the 
         *               kind of this regular expression
         *
         * @param content  the child of this expression
         */
        public RegExp1(int type, object content)
            : base(type)
        {
            this.content = content;
        }


        /**
         * Returns a string-representation of this regular expression
         * with the specified indentation.
         *
         * @param tab   a string that should contain only space characters and
         *              that is inserted in front of standard string-representation
         *              pf this object.
         */
        public override string Print(string tab)
        {
            if (content is RegExp _content)
            {
                return tab + "type = " + type + OutputWriter.NewLine + tab + "content :" + OutputWriter.NewLine + (_content).Print(tab + "  ");
            }
            else
                return tab + "type = " + type + OutputWriter.NewLine + tab + "content :" + OutputWriter.NewLine + tab + "  " + content;
        }


        /**
         * Returns a string-representation of this regular expression
         */
        public override string ToString() => Print("");
    }
}