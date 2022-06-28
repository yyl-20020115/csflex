namespace CSFlex.Runtime
{
    public class Symbol
    {
        public int Sym = 0;
        public int ParseState = 0;
        public bool UsedByParser = false;
        public int Left = 0;
        public int Right = 0;
        public object? Value = null;

        public Symbol(int sym_num) 
            : this(sym_num, -1)
        {
            this.Left = -1;
            this.Right = -1;
            this.Value = null;
        }

        internal Symbol(int sym_num, int state)
        {
            this.UsedByParser = false;
            this.Sym = sym_num;
            this.ParseState = state;
        }

        public Symbol(int id, object o) : this(id, -1, -1, o)
        {
        }

        public Symbol(int id, int l, int r) : this(id, l, r, null)
        {
        }

        public Symbol(int id, int l, int r, object? o) : this(id)
        {
            this.Left = l;
            this.Right = r;
            this.Value = o;
        }

        public override string ToString() =>  "#" + this.Sym;
    }
}

