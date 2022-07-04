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
 * Encapsulates an action in the specification.
 *
 * It stores the Java code as string together with a priority (line number in the specification).
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.5 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class Action
{
    /**
     * The Java code this Action represents
     */
    private string content;
    private string contentTrimmed;

    /**
     * The priority (i.e. line number in the specification) of this Action. 
     */
    private int priority;

    /**
     * True iff the action belongs to an lookahead expresstion 
     * (<code>a/b</code> or <code>r$</code>)
     */
    private bool lookAction;

    /**
     * Return look ahead flag.
     * 
     * @return true if this actions belongs to a lookahead rule
     */
    public string Content => content;
    public string ContentTrimmed => contentTrimmed;
    public int Priority => priority;
    public bool LookAction { get => lookAction; set => lookAction = value; }

    /**
     * Creates a new Action object with specified content and line number.
     * 
     * @param content    java code
     * @param priority   line number
     */
    public Action(string content, int priority)
    {
        this.content = (content ??= "");
        this.contentTrimmed = content.Trim();
        this.priority = priority;
    }


    /**
     * Compares the priority value of this Action with the specified action.
     *
     * @param other  the other Action to compare this Action with.
     *
     * @return this Action if it has higher priority - the specified one, if not.
     */
    public Action GetHigherPriority(Action other)
        => (other == null) ? this : (other.priority > this.priority ? this : other);


    /**
     * Returns the string representation of this object.
     * 
     * @return string representation of the action
     */
    public override string ToString() => $"Action (priority {priority}, lookahead {LookAction.ToString().ToLower()}) :{OutputWriter.NewLine}{Content}"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$


    /**
     * Returns <code>true</code> iff the parameter is an
     * Action with the same content as this one.
     *
     * @param a   the object to compare this Action with
     * @return    true if the action strings are equal
     */
    public bool IsEquiv(Action a) => this == a || ((Options.EmitCSharp == false) && this.contentTrimmed.Equals(a.contentTrimmed));
    /**
     * Calculate hash value.
     * 
     * @return a hash value for this Action
     */
    public override int GetHashCode() => contentTrimmed.GetHashCode();
    /**
     * Test for equality to another object.
     * 
     * This action equals another object if the other 
     * object is an equivalent action. 
     * 
     * @param o  the other object.
     * 
     * @see Action#isEquiv(Action)
     */
    public override bool Equals(object? o) => o is Action a && IsEquiv(a);

}