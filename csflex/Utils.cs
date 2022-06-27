/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * C# Flex 1.4                                                             *
 * Copyright (C) 2004-2005  Jonathan Gilbert <logic@deltaq.org>            *
 * All rights reserved.                                                    *
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSFlex
{
    public class RuntimeException : Exception
    {
        public RuntimeException()
        {
        }

        public RuntimeException(string msg)
          : base(msg)
        {
        }
    }

    public class File
    {
        private string name;
        public string Name => this.name;
        public File(string name)
        {
            this.name = name;
        }
        public File(string parent, string child)
        {
            this.name = Path.Combine(parent, child);
        }


        public static implicit operator string(File file) => file.name;

        public string Parent => Path.GetDirectoryName(name) is string ret ?
                    (ret.Length == 0 ? null : ret) : null;

        public bool Exists => new FileInfo(name).Exists;

        public bool Delete()
        {
            var info = new FileInfo(name);

            try
            {
                info.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RenameTo(File dest)
        {
            var info = new FileInfo(name);

            try
            {
                info.MoveTo(dest.name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static readonly char separatorChar = Path.DirectorySeparatorChar;


        public bool CanRead
        {
            get
            {
                FileStream stream = null;

                try
                {
                    stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
        }

        public bool IsFile => (new FileInfo(name).Attributes & FileAttributes.Directory) != FileAttributes.Directory;

        public bool IsDirectory => (new FileInfo(name).Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        public bool Mkdirs()
        {
            var info = new FileInfo(name);

            var needed = new Stack<DirectoryInfo>();

            var parent = info.Directory;

            try
            {
                while (!parent.Exists)
                {
                    needed.Push(parent);
                    parent = parent.Parent;
                }

                while (needed.Count > 0)
                {
                    var dir = needed.Pop();
                    dir.Create();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString() => name;
    }

    public class Integer
    {
        int v;

        public Integer(int value)
        {
            v = value;
        }

        public int intValue()
        {
            return v;
        }

        public static string ToOctalString(int c)
        {
            var ret = new StringBuilder();

            while (c > 0)
            {
                int unit_place = (c & 7);
                c >>= 3;

                ret.Insert(0, (char)(unit_place + '0'));
            }

            if (ret.Length == 0)
                return "0";
            else
                return ret.ToString();
        }

        public static string ToHexString(int c)
        {
            var ret = new StringBuilder();

            while (c > 0)
            {
                int unit_place = (c & 15);
                c >>= 4;

                if (unit_place >= 10)
                    ret.Insert(0, (char)(unit_place + 'a' - 10));
                else
                    ret.Insert(0, (char)(unit_place + '0'));
            }

            if (ret.Length == 0)
                return "0";
            else
                return ret.ToString();
        }

        public static int ParseInt(string s)
        {
            return ParseInt(s, 10);
        }

        public static int ParseInt(string s, int _base = 10)
        {
            const string alpha = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if ((_base < 2) || (_base > 36))
                throw new ArgumentException("Number base cannot be less than 2 or greater than 36", "base");

            s = s.ToUpper();

            int value = 0;

            for (int i = 0; i < s.Length; i++)
            {
                int idx = alpha.IndexOf(s[i]);

                if ((idx < 0) || (idx >= _base))
                    throw new FormatException("'" + s[i] + "' is not a valid base-" + _base + " digit");

                value = (value * _base) + idx;
            }

            return value;
        }

        public override int GetHashCode() => v.GetHashCode();

        public override bool Equals(object? obj) => obj switch
        {
            null => false,
            int i => v == i,
            Integer n => v == n.v,
            _ => false,
        };

        public override string ToString() => v.ToString();
    }

    public class Boolean
    {
        bool v;

        public Boolean(bool value)
        {
            v = value;
        }

        public bool BooleanValue => v;
    }

    public class PrettyArrayList<T> : List<T>
    {
        public PrettyArrayList(ICollection<T> c)
          : base(c)
        {
        }

        public PrettyArrayList(int capacity)
          : base(capacity)
        {
        }

        public PrettyArrayList()
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("[");

            for (int i = 0; i < Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                builder.Append(this[i]);
            }

            builder.Append("]");

            return builder.ToString();
        }
    }

    public class PrettyHashtable<T,V> : Dictionary<T,V> where T :notnull
    {
        public PrettyHashtable(IDictionary<T,V> d)
          : base(d)
        {
        }

        public PrettyHashtable(int capacity)
          : base(capacity)
        {
        }

        public PrettyHashtable()
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            var first = true;
            foreach(var v in this)
            {
                if (!first)
                {
                    builder.Append(',');
                }
                builder.Append($"{v.Key} = {v.Value}");
                first = false;
            }
            builder.Append("}");

            return builder.ToString();
        }

    }
}
