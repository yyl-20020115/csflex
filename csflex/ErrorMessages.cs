/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * C# Flex                                                                 *
 * Copyright © 2021 Christian Klauser <christianklauser@outlook.com>       *
 * Derived from:                                                           *
 *                                                                         *
 *   C# Flex 1.4                                                           *
 *   Copyright (C) 2004-2005  Jonathan Gilbert <logic@deltaq.org>          *
 *                                                                         * 
 *   JFlex 1.4                                                             *
 *   Copyright (C) 1998-2004  Gerwin Klein <lsf@jflex.de>                  *
 *   All rights reserved.                                                  *
 *                                                                         *
 * This program is free software); you can redistribute it and/or modify    *
 * it under the terms of the GNU General Public License. See the file      *
 * COPYRIGHT for more information.                                         *
 *                                                                         *
 * This program is distributed in the hope that it will be useful,         *
 * but WITHOUT ANY WARRANTY); without even the implied warranty of          *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
 * GNU General Public License for more details.                            *
 *                                                                         *
 * You should have received a copy of the GNU General Public License along *
 * with this program); if not, write to the Free Software Foundation, Inc., *
 * 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA                 *
 *                                                                         *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
namespace CSFlex
{
    using System.Resources;

    /**
     * <summary>
     * Central class for all kinds of C# Flex messages.
     * </summary>
     * <remarks>
     * [Is not yet used exclusively, but should be]
     * </remarks>
     * @author Gerwin Klein
     * @version JFlex 1.4, $Revision: 2.7 $, $Date: 2004/04/12 10:07:47 $
     * @author Jonathan Gilbert
     * @version CSFlex 1.4
     */
    public class ErrorMessages
    {
        private readonly string key;

        private static readonly ResourceManager Resources =
          new("csflex.Messages", typeof(ErrorMessages).Assembly);

        private ErrorMessages(string key)
        {
            this.key = key;
        }

        public static string Get(ErrorMessages msg)
        {
            try
            {
                return Resources.GetString(msg.key) ?? $"?{msg.key}?";
            }
            catch
            {
                return $"!{msg.key}!";
            }
        }

        public static string Get(ErrorMessages msg, string data) => string.Format(Get(msg), data);

        public static string Get(ErrorMessages msg, string data1, string data2) => string.Format(Get(msg), data1, data2);

        public static string Get(ErrorMessages msg, int data) => string.Format(Get(msg), data);

        // typesafe enumeration (generated, do not edit)  
        public static readonly ErrorMessages UNTERMINATED_STR = new("UNTERMINATED_STR");
        public static readonly ErrorMessages EOF_WO_ACTION = new("EOF_WO_ACTION");
        public static readonly ErrorMessages EOF_SINGLERULE = new("EOF_SINGLERULE");
        public static readonly ErrorMessages UNKNOWN_OPTION = new("UNKNOWN_OPTION");
        public static readonly ErrorMessages UNEXPECTED_CHAR = new("UNEXPECTED_CHAR");
        public static readonly ErrorMessages UNEXPECTED_NL = new("UNEXPECTED_NL");
        public static readonly ErrorMessages LEXSTATE_UNDECL = new("LEXSTATE_UNDECL");
        public static readonly ErrorMessages STATE_IDENT_EXP = new("STATE_IDENT_EXP");
        public static readonly ErrorMessages REPEAT_ZERO = new("REPEAT_ZERO");
        public static readonly ErrorMessages REPEAT_GREATER = new("REPEAT_GREATER");
        public static readonly ErrorMessages REGEXP_EXPECTED = new("REGEXP_EXPECTED");
        public static readonly ErrorMessages MACRO_UNDECL = new("MACRO_UNDECL");
        public static readonly ErrorMessages CHARSET_2_SMALL = new("CHARSET_2_SMALL");
        public static readonly ErrorMessages CS2SMALL_STRING = new("CS2SMALL_STRING");
        public static readonly ErrorMessages CS2SMALL_CHAR = new("CS2SMALL_CHAR");
        public static readonly ErrorMessages CHARCLASS_MACRO = new("CHARCLASS_MACRO");
        public static readonly ErrorMessages UNKNOWN_SYNTAX = new("UNKNOWN_SYNTAX");
        public static readonly ErrorMessages SYNTAX_ERROR = new("SYNTAX_ERROR");
        public static readonly ErrorMessages NOT_AT_BOL = new("NOT_AT_BOL");
        public static readonly ErrorMessages NO_MATCHING_BR = new("NO_MATCHING_BR");
        public static readonly ErrorMessages EOF_IN_ACTION = new("EOF_IN_ACTION");
        public static readonly ErrorMessages EOF_IN_COMMENT = new("EOF_IN_COMMENT");
        public static readonly ErrorMessages EOF_IN_STRING = new("EOF_IN_STRING");
        public static readonly ErrorMessages EOF_IN_MACROS = new("EOF_IN_MACROS");
        public static readonly ErrorMessages EOF_IN_STATES = new("EOF_IN_STATES");
        public static readonly ErrorMessages EOF_IN_REGEXP = new("EOF_IN_REGEXP");
        public static readonly ErrorMessages UNEXPECTED_EOF = new("UNEXPECTED_EOF");
        public static readonly ErrorMessages NO_LEX_SPEC = new("NO_LEX_SPEC");
        public static readonly ErrorMessages NO_LAST_ACTION = new("NO_LAST_ACTION");
        public static readonly ErrorMessages LOOKAHEAD_ERROR = new("LOOKAHEAD_ERROR");
        public static readonly ErrorMessages NO_DIRECTORY = new("NO_DIRECTORY");
        public static readonly ErrorMessages NO_SKEL_FILE = new("NO_SKEL_FILE");
        public static readonly ErrorMessages WRONG_SKELETON = new("WRONG_SKELETON");
        public static readonly ErrorMessages OUT_OF_MEMORY = new("OUT_OF_MEMORY");
        public static readonly ErrorMessages QUIL_INITTHROW = new("QUIL_INITTHROW");
        public static readonly ErrorMessages QUIL_EOFTHROW = new("QUIL_EOFTHROW");
        public static readonly ErrorMessages QUIL_YYLEXTHROW = new("QUIL_YYLEXTHROW");
        public static readonly ErrorMessages ZERO_STATES = new("ZERO_STATES");
        public static readonly ErrorMessages NO_BUFFER_SIZE = new("NO_BUFFER_SIZE");
        public static readonly ErrorMessages NOT_READABLE = new("NOT_READABLE");
        public static readonly ErrorMessages FILE_CYCLE = new("FILE_CYCLE");
        public static readonly ErrorMessages FILE_WRITE = new("FILE_WRITE");
        public static readonly ErrorMessages QUIL_SCANERROR = new("QUIL_SCANERROR");
        public static readonly ErrorMessages NEVER_MATCH = new("NEVER_MATCH");
        public static readonly ErrorMessages QUIL_THROW = new("QUIL_THROW");
        public static readonly ErrorMessages EOL_IN_CHARCLASS = new("EOL_IN_CHARCLASS");
        public static readonly ErrorMessages QUIL_CUPSYM = new("QUIL_CUPSYM");
        public static readonly ErrorMessages CUPSYM_AFTER_CUP = new("CUPSYM_AFTER_CUP");
        public static readonly ErrorMessages ALREADY_RUNNING = new("ALREADY_RUNNING");
        public static readonly ErrorMessages CANNOT_READ_SKEL = new("CANNOT_READ_SKEL");
        public static readonly ErrorMessages READING_SKEL = new("READING_SKEL");
        public static readonly ErrorMessages SKEL_IO_ERROR = new("SKEL_IO_ERROR");
        public static readonly ErrorMessages SKEL_IO_ERROR_DEFAULT = new("SKEL_IO_ERROR_DEFAULT");
        public static readonly ErrorMessages READING = new("READING");
        public static readonly ErrorMessages CANNOT_OPEN = new("CANNOT_OPEN");
        public static readonly ErrorMessages NFA_IS = new("NFA_IS");
        public static readonly ErrorMessages NFA_STATES = new("NFA_STATES");
        public static readonly ErrorMessages DFA_TOOK = new("DFA_TOOK");
        public static readonly ErrorMessages DFA_IS = new("DFA_IS");
        public static readonly ErrorMessages MIN_TOOK = new("MIN_TOOK");
        public static readonly ErrorMessages MIN_DFA_IS = new("MIN_DFA_IS");
        public static readonly ErrorMessages WRITE_TOOK = new("WRITE_TOOK");
        public static readonly ErrorMessages TOTAL_TIME = new("TOTAL_TIME");
        public static readonly ErrorMessages IO_ERROR = new("IO_ERROR");
        public static readonly ErrorMessages THIS_IS_CSFLEX = new("THIS_IS_CSFLEX");
        public static readonly ErrorMessages UNKNOWN_COMMANDLINE = new("UNKNOWN_COMMANDLINE");
        public static readonly ErrorMessages MACRO_CYCLE = new("MACRO_CYCLE");
        public static readonly ErrorMessages MACRO_DEF_MISSING = new("MACRO_DEF_MISSING");
        public static readonly ErrorMessages PARSING_TOOK = new("PARSING_TOOK");
        public static readonly ErrorMessages NFA_TOOK = new("NFA_TOOK");
        public static readonly ErrorMessages NOT_CSHARP_SKELETON = new("NOT_CSHARP_SKELETON");
    }
}
