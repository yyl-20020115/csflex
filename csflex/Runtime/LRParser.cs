namespace CSFlex.Runtime
{
    using CSFlex;
    using System;
    using System.Text;

    public abstract class LRParser
    {
        protected const int _error_sync_size = 3;
        protected bool _done_parsing;
        protected int tos;
        protected Symbol cur_token;
        protected JCStack<Symbol> stack = new();
        protected short[][] production_tab;
        protected short[][] action_tab;
        protected short[][] reduce_tab;
        private Scanner _scanner;
        protected Symbol[] lookahead;
        protected int lookahead_pos;

        public LRParser()
        {
            this._done_parsing = false;
            this.stack = new JCStack<Symbol>();
        }

        public LRParser(Scanner s) : this()
        {
            this.setScanner(s);
        }

        public abstract short[][] action_table();
        protected bool advance_lookahead()
        {
            this.lookahead_pos++;
            return (this.lookahead_pos < this.error_sync_size());
        }

        protected Symbol cur_err_token() => 
            this.lookahead[this.lookahead_pos];

        public virtual void debug_message(string mess)
        {
            Console.Error.WriteLine(mess);
        }

        public Symbol debug_parse()
        {
            Symbol item = null;
            this.production_tab = this.production_table();
            this.action_tab = this.action_table();
            this.reduce_tab = this.reduce_table();
            this.debug_message("# Initializing parser");
            this.init_actions();
            this.user_init();
            this.cur_token = this.scan();
            this.debug_message("# Current Symbol is #" + this.cur_token.sym);
            this.stack.Clear();
            this.stack.Push(new Symbol(0, this.start_state()));
            this.tos = 0;
            this._done_parsing = false;
            while (!this._done_parsing)
            {
                if (this.cur_token.used_by_parser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }
                int num = this.get_action((this.stack.Peek()).parse_state, this.cur_token.sym);
                if (num > 0)
                {
                    this.cur_token.parse_state = num - 1;
                    this.cur_token.used_by_parser = true;
                    this.debug_shift(this.cur_token);
                    this.stack.Push(this.cur_token);
                    this.tos++;
                    this.cur_token = this.scan();
                    this.debug_message("# Current token is " + this.cur_token);
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.do_action(-num - 1, this, this.stack, this.tos);
                        short num3 = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        this.debug_reduce(-num - 1, num3, num2);
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.get_reduce(((Symbol) this.stack.Peek()).parse_state, num3);
                        this.debug_message(string.Concat(new object[] { "# Reduce rule: top state ", ((Symbol) this.stack.Peek()).parse_state, ", lhs sym ", num3, " -> state ", num }));
                        item.parse_state = num;
                        item.used_by_parser = true;
                        this.stack.Push(item);
                        this.tos++;
                        this.debug_message("# Goto state #" + num);
                        continue;
                    }
                    if (num == 0)
                    {
                        this.syntax_error(this.cur_token);
                        if (!this.error_recovery(true))
                        {
                            this.unrecovered_syntax_error(this.cur_token);
                            this.done_parsing();
                            continue;
                        }
                        item = this.stack.Peek();
                    }
                }
            }
            return item;
        }

        public virtual void debug_reduce(int prod_num, int nt_num, int rhs_size)
        {
            this.debug_message(string.Concat(new object[] { "# Reduce with prod #", prod_num, " [NT=", nt_num, ", SZ=", rhs_size, "]" }));
        }

        public virtual void debug_shift(Symbol shift_tkn)
        {
            this.debug_message(string.Concat(new object[] { "# Shift under term #", shift_tkn.sym, " to state #", shift_tkn.parse_state }));
        }

        public virtual void debug_stack()
        {
            StringBuilder builder = new StringBuilder("## STACK:");
            for (int i = 0; i < this.stack.Size; i++)
            {
                Symbol symbol = (Symbol) this.stack.GetAt(i);
                builder.AppendFormat(" <state {0}, sym {1}>", symbol.parse_state, symbol.sym);
                if (((i % 3) == 2) || (i == (this.stack.Size - 1)))
                {
                    this.debug_message(builder.ToString());
                    builder = new StringBuilder("         ");
                }
            }
        }

        public abstract Symbol do_action(int act_num, LRParser parser, JCStack<Symbol> stack, int top);
        public void done_parsing()
        {
            this._done_parsing = true;
        }

        public virtual void dump_stack()
        {
            if (this.stack == null)
            {
                this.debug_message("# Stack dump requested, but stack is null");
            }
            else
            {
                this.debug_message("============ Parse Stack Dump ============");
                for (int i = 0; i < this.stack.Size; i++)
                {
                    this.debug_message(string.Concat(new object[] { "Symbol: ", ( this.stack.GetAt(i)).sym, " State: ", ((Symbol) this.stack.GetAt(i)).parse_state }));
                }
                this.debug_message("==========================================");
            }
        }

        public abstract int EOF_sym();
        protected bool error_recovery(bool debug)
        {
            if (debug)
            {
                this.debug_message("# Attempting error recovery");
            }
            if (!this.find_recovery_config(debug))
            {
                if (debug)
                {
                    this.debug_message("# Error recovery fails");
                }
                return false;
            }
            this.read_lookahead();
            while (true)
            {
                if (debug)
                {
                    this.debug_message("# Trying to parse ahead");
                }
                if (this.try_parse_ahead(debug))
                {
                    break;
                }
                if (this.lookahead[0].sym == this.EOF_sym())
                {
                    if (debug)
                    {
                        this.debug_message("# Error recovery fails at EOF");
                    }
                    return false;
                }
                if (debug)
                {
                    this.debug_message("# Consuming Symbol #" + this.lookahead[0].sym);
                }
                this.restart_lookahead();
            }
            if (debug)
            {
                this.debug_message("# Parse-ahead ok, going back to normal parse");
            }
            this.parse_lookahead(debug);
            return true;
        }

        public abstract int error_sym();
        protected int error_sync_size() => 
            3;

        protected bool find_recovery_config(bool debug)
        {
            if (debug)
            {
                this.debug_message("# Finding recovery state on stack");
            }
            int right = ((Symbol) this.stack.Peek()).right;
            int left = ((Symbol) this.stack.Peek()).left;
            while (!this.shift_under_error())
            {
                if (debug)
                {
                    this.debug_message("# Pop stack by one, state was # " + ( this.stack.Peek()).parse_state);
                }
                left = ((Symbol) this.stack.Pop()).left;
                this.tos--;
                if (this.stack.IsEmpty)
                {
                    if (debug)
                    {
                        this.debug_message("# No recovery state found on stack");
                    }
                    return false;
                }
            }
            int num = this.get_action((this.stack.Peek()).parse_state, this.error_sym());
            if (debug)
            {
                this.debug_message("# Recover state found (#" + (this.stack.Peek()).parse_state + ")");
                this.debug_message("# Shifting on error to state #" + (num - 1));
            }
            Symbol item = new Symbol(this.error_sym(), left, right) {
                parse_state = num - 1,
                used_by_parser = true
            };
            this.stack.Push(item);
            this.tos++;
            return true;
        }

        protected short get_action(int state, int sym)
        {
            int num4;
            short[] numArray = this.action_tab[state];
            if (numArray.Length < 20)
            {
                num4 = 0;
                while (num4 < numArray.Length)
                {
                    short num = numArray[num4++];
                    if ((num == sym) || (num == -1))
                    {
                        return numArray[num4];
                    }
                    num4++;
                }
            }
            else
            {
                int num2 = 0;
                int num3 = ((numArray.Length - 1) / 2) - 1;
                while (num2 <= num3)
                {
                    num4 = (num2 + num3) / 2;
                    if (sym == numArray[num4 * 2])
                    {
                        return numArray[(num4 * 2) + 1];
                    }
                    if (sym > numArray[num4 * 2])
                    {
                        num2 = num4 + 1;
                    }
                    else
                    {
                        num3 = num4 - 1;
                    }
                }
                return numArray[numArray.Length - 1];
            }
            return 0;
        }

        protected short get_reduce(int state, int sym)
        {
            short[] numArray = this.reduce_tab[state];
            if (numArray != null)
            {
                for (int i = 0; i < numArray.Length; i++)
                {
                    short num = numArray[i++];
                    if ((num == sym) || (num == -1))
                    {
                        return numArray[i];
                    }
                }
            }
            return -1;
        }

        public Scanner getScanner() => 
            this._scanner;

        protected abstract void init_actions();
        public Symbol parse()
        {
            Symbol item = null;
            this.production_tab = this.production_table();
            this.action_tab = this.action_table();
            this.reduce_tab = this.reduce_table();
            this.init_actions();
            this.user_init();
            this.cur_token = this.scan();
            this.stack.Clear();
            this.stack.Push(new (0, this.start_state()));
            this.tos = 0;
            this._done_parsing = false;
            while (!this._done_parsing)
            {
                if (this.cur_token.used_by_parser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }
                int num = this.get_action( this.stack.Peek().parse_state, this.cur_token.sym);
                if (num > 0)
                {
                    this.cur_token.parse_state = num - 1;
                    this.cur_token.used_by_parser = true;
                    this.stack.Push(this.cur_token);
                    this.tos++;
                    this.cur_token = this.scan();
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.do_action(-num - 1, this, this.stack, this.tos);
                        short sym = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.get_reduce(((Symbol) this.stack.Peek()).parse_state, sym);
                        item.parse_state = num;
                        item.used_by_parser = true;
                        this.stack.Push(item);
                        this.tos++;
                        continue;
                    }
                    if (num == 0)
                    {
                        this.syntax_error(this.cur_token);
                        if (!this.error_recovery(false))
                        {
                            this.unrecovered_syntax_error(this.cur_token);
                            this.done_parsing();
                            continue;
                        }
                        item = this.stack.Peek();
                    }
                }
            }
            return item;
        }

        protected void parse_lookahead(bool debug)
        {
            Symbol item = null;
            this.lookahead_pos = 0;
            if (debug)
            {
                this.debug_message("# Reparsing saved input with actions");
                this.debug_message("# Current Symbol is #" + this.cur_err_token().sym);
                this.debug_message("# Current state is #" + (this.stack.Peek()).parse_state);
            }
            while (!this._done_parsing)
            {
                int num = this.get_action(this.stack.Peek().parse_state, this.cur_err_token().sym);
                if (num > 0)
                {
                    this.cur_err_token().parse_state = num - 1;
                    this.cur_err_token().used_by_parser = true;
                    if (debug)
                    {
                        this.debug_shift(this.cur_err_token());
                    }
                    this.stack.Push(this.cur_err_token());
                    this.tos++;
                    if (!this.advance_lookahead())
                    {
                        if (debug)
                        {
                            this.debug_message("# Completed reparse");
                        }
                        break;
                    }
                    if (debug)
                    {
                        this.debug_message("# Current Symbol is #" + this.cur_err_token().sym);
                    }
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.do_action(-num - 1, this, this.stack, this.tos);
                        short num3 = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        if (debug)
                        {
                            this.debug_reduce(-num - 1, num3, num2);
                        }
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.get_reduce(this.stack.Peek().parse_state, num3);
                        item.parse_state = num;
                        item.used_by_parser = true;
                        this.stack.Push(item);
                        this.tos++;
                        if (debug)
                        {
                            this.debug_message("# Goto state #" + num);
                        }
                        continue;
                    }
                    if (num == 0)
                    {
                        this.report_fatal_error("Syntax error", item);
                        break;
                    }
                }
            }
        }

        public abstract short[][] production_table();
        protected void read_lookahead()
        {
            this.lookahead = new Symbol[this.error_sync_size()];
            for (int i = 0; i < this.error_sync_size(); i++)
            {
                this.lookahead[i] = this.cur_token;
                this.cur_token = this.scan();
            }
            this.lookahead_pos = 0;
        }

        public abstract short[][] reduce_table();
        public virtual void report_error(string message, object info)
        {
            Console.Error.Write(message);
            if (info is Symbol)
            {
                if (((Symbol) info).left != -1)
                {
                    Console.Error.WriteLine(" at character {0} of input", ((Symbol) info).left);
                }
                else
                {
                    Console.Error.WriteLine();
                }
            }
            else
            {
                Console.Error.WriteLine();
            }
        }

        public virtual void report_fatal_error(string message, object info)
        {
            this.done_parsing();
            this.report_error(message, info);
            throw new Exception("Can't recover from previous error(s)");
        }

        protected void restart_lookahead()
        {
            for (int i = 1; i < this.error_sync_size(); i++)
            {
                this.lookahead[i - 1] = this.lookahead[i];
            }
            this.lookahead[this.error_sync_size() - 1] = this.cur_token;
            this.cur_token = this.scan();
            this.lookahead_pos = 0;
        }

        public virtual Symbol scan()
        {
            Symbol symbol = this.getScanner().NextToken();
            return ((symbol != null) ? symbol : new Symbol(this.EOF_sym()));
        }

        public void setScanner(Scanner s)
        {
            this._scanner = s;
        }

        protected bool shift_under_error() => 
            (this.get_action((this.stack.Peek()).parse_state, this.error_sym()) > 0);

        public abstract int start_production();
        public abstract int start_state();
        public virtual void syntax_error(Symbol cur_token)
        {
            this.report_error("Syntax error", cur_token);
        }

        protected bool try_parse_ahead(bool debug)
        {
            VIrtualParseStack _stack = new VIrtualParseStack(this.stack);
            while (true)
            {
                int num = this.get_action(_stack.top(), this.cur_err_token().sym);
                if (num == 0)
                {
                    return false;
                }
                if (num > 0)
                {
                    _stack.push(num - 1);
                    if (debug)
                    {
                        this.debug_message(string.Concat(new object[] { "# Parse-ahead shifts Symbol #", this.cur_err_token().sym, " into state #", num - 1 }));
                    }
                    if (!this.advance_lookahead())
                    {
                        return true;
                    }
                }
                else
                {
                    if ((-num - 1) == this.start_production())
                    {
                        if (debug)
                        {
                            this.debug_message("# Parse-ahead accepts");
                        }
                        return true;
                    }
                    short sym = this.production_tab[-num - 1][0];
                    short num3 = this.production_tab[-num - 1][1];
                    for (int i = 0; i < num3; i++)
                    {
                        _stack.pop();
                    }
                    if (debug)
                    {
                        this.debug_message(string.Concat(new object[] { "# Parse-ahead reduces: handle size = ", num3, " lhs = #", sym, " from state #", _stack.top() }));
                    }
                    _stack.push(this.get_reduce(_stack.top(), sym));
                    if (debug)
                    {
                        this.debug_message("# Goto state #" + _stack.top());
                    }
                }
            }
        }

        protected static short[][] unpackFromShorts(short[] sb)
        {
            int index = 0;
            int num2 = (sb[index] << 0x10) | ((ushort) sb[index + 1]);
            index += 2;
            short[][] numArray = new short[num2][];
            for (int i = 0; i < num2; i++)
            {
                int num4 = (sb[index] << 0x10) | ((ushort) sb[index + 1]);
                index += 2;
                numArray[i] = new short[num4];
                for (int j = 0; j < num4; j++)
                {
                    numArray[i][j] = (short) (sb[index++] - 2);
                }
            }
            return numArray;
        }

        protected static short[][] unpackFromStrings(string[] sa)
        {
            StringBuilder builder = new StringBuilder(sa[0]);
            for (int i = 1; i < sa.Length; i++)
            {
                builder.Append(sa[i]);
            }
            int num2 = 0;
            int num3 = (builder[num2] << 0x10) | builder[num2 + 1];
            num2 += 2;
            short[][] numArray = new short[num3][];
            for (int j = 0; j < num3; j++)
            {
                int num5 = (builder[num2] << 0x10) | builder[num2 + 1];
                num2 += 2;
                numArray[j] = new short[num5];
                for (int k = 0; k < num5; k++)
                {
                    numArray[j][k] = (short) (builder[num2++] - '\x0002');
                }
            }
            return numArray;
        }

        public virtual void unrecovered_syntax_error(Symbol cur_token)
        {
            this.report_fatal_error("Couldn't repair and continue parse", cur_token);
        }

        public virtual void user_init()
        {
        }
    }
}

