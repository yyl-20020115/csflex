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
     * Regular expression with two children (e.g. a | b)
     *
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.2 $, $Date: 2004/04/12 10:07:48 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class RegExp2 : RegExp
    {

        internal RegExp r1, r2;

        public RegExp2(int type, RegExp r1, RegExp r2) : base(type)
        {
            this.r1 = r1;
            this.r2 = r2;
        }

        public override string Print(string tab) => tab + "type = " + type + OutputWriter.NewLine + tab + "child 1 :" + OutputWriter.NewLine + //$NON-NLS-1$ //$NON-NLS-2$
                   r1.Print(tab + "  ") + OutputWriter.NewLine + tab + "child 2 :" + OutputWriter.NewLine + //$NON-NLS-1$ //$NON-NLS-2$
                   r2.Print(tab + "  "); //$NON-NLS-1$

        public override string ToString() => Print(""); //$NON-NLS-1$
    }

}