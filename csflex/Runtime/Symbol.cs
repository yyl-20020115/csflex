namespace CSFlex.Runtime
{
    public class Symbol
    {
        public int sym = 0;
        public int parse_state = 0;
        internal bool used_by_parser = false;
        public int left = 0;
        public int right = 0;
        public object? value = null;

        public Symbol(int sym_num) 
            : this(sym_num, -1)
        {
            this.left = -1;
            this.right = -1;
            this.value = null;
        }

        internal Symbol(int sym_num, int state)
        {
            this.used_by_parser = false;
            this.sym = sym_num;
            this.parse_state = state;
        }

        public Symbol(int id, object o) : this(id, -1, -1, o)
        {
        }

        public Symbol(int id, int l, int r) : this(id, l, r, null)
        {
        }

        public Symbol(int id, int l, int r, object o) : this(id)
        {
            this.left = l;
            this.right = r;
            this.value = o;
        }

        public override string ToString() =>  "#" + this.sym;
    }
}

