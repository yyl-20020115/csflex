namespace CSFlex.Runtime
{
    using CSFlex;
    using System;
    using System.Text;

    public abstract class LRParser
    {
        protected const int _error_sync_size = 3;
        protected bool _done_parsing = false;
        protected int tos = 0;
        protected Symbol cur_token = new(0);
        protected JCStack<Symbol> stack = new();
        protected short[][] production_tab = Array.Empty<short[]>();
        protected short[][] action_tab = Array.Empty<short[]>();
        protected short[][] reduce_tab = Array.Empty<short[]>();
        private Scanner? _scanner = null;
        protected Symbol[] lookahead = new Symbol[0];
        protected int lookahead_pos = 0;

        public LRParser()
        {
        }

        public LRParser(Scanner s) : this()
        {
            this.SetScanner(s);
        }

        public abstract short[][] ActionTable { get; }

        protected bool AdvanceLookAhead()
        {
            this.lookahead_pos++;
            return (this.lookahead_pos < this.ErrorSyncSIze);
        }

        protected Symbol CurErrToken =>
            this.lookahead[this.lookahead_pos];

        public virtual void DebugMessage(string mess)
        {
            Console.Error.WriteLine(mess);
        }

        public Symbol? DebugParse()
        {
            Symbol? item = null;
            this.production_tab = this.ProductionTable;
            this.action_tab = this.ActionTable;
            this.reduce_tab = this.ReduceTable;
            this.DebugMessage("# Initializing parser");
            this.InitActions();
            this.UserInit();
            this.cur_token = this.Scan();
            this.DebugMessage("# Current Symbol is #" + this.cur_token.Sym);
            this.stack.Clear();
            this.stack.Push(new Symbol(0, this.StartState));
            this.tos = 0;
            this._done_parsing = false;
            while (!this._done_parsing)
            {
                if (this.cur_token.UsedByParser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }
                int num = this.GetAction(this.stack.Peek()!.ParseState, this.cur_token.Sym);
                if (num > 0)
                {
                    this.cur_token.ParseState = num - 1;
                    this.cur_token.UsedByParser = true;
                    this.DebugShift(this.cur_token);
                    this.stack.Push(this.cur_token);
                    this.tos++;
                    this.cur_token = this.Scan();
                    this.DebugMessage("# Current token is " + this.cur_token);
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.DoAction(-num - 1, this, this.stack, this.tos);
                        short num3 = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        this.DebugReduce(-num - 1, num3, num2);
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.GetReduce(this.stack.Peek()!.ParseState, num3);
                        this.DebugMessage(string.Concat(new object[] { "# Reduce rule: top state ", this.stack.Peek()!.ParseState, ", lhs sym ", num3, " -> state ", num }));
                        item.ParseState = num;
                        item.UsedByParser = true;
                        this.stack.Push(item);
                        this.tos++;
                        this.DebugMessage("# Goto state #" + num);
                        continue;
                    }
                    if (num == 0)
                    {
                        this.SyntaxError(this.cur_token);
                        if (!this.ErrorRecovery(true))
                        {
                            this.UnrecoveredSyntaxError(this.cur_token);
                            this.DoneParsing();
                            continue;
                        }
                        item = this.stack.Peek();
                    }
                }
            }
            return item;
        }

        public virtual void DebugReduce(int prod_num, int nt_num, int rhs_size)
        {
            this.DebugMessage(string.Concat(new object[] { "# Reduce with prod #", prod_num, " [NT=", nt_num, ", SZ=", rhs_size, "]" }));
        }

        public virtual void DebugShift(Symbol shift_tkn)
        {
            this.DebugMessage(string.Concat(new object[] { "# Shift under term #", shift_tkn.Sym, " to state #", shift_tkn.ParseState }));
        }

        public virtual void DebugStack()
        {
            var builder = new StringBuilder("## STACK:");
            for (int i = 0; i < this.stack.Size; i++)
            {
                var symbol = this.stack.GetAt(i);
                builder.AppendFormat(" <state {0}, sym {1}>", symbol.ParseState, symbol.Sym);
                if (((i % 3) == 2) || (i == (this.stack.Size - 1)))
                {
                    this.DebugMessage(builder.ToString());
                    builder = new StringBuilder("         ");
                }
            }
        }

        public abstract Symbol DoAction(int act_num, LRParser parser, JCStack<Symbol> stack, int top);
        public void DoneParsing()
        {
            this._done_parsing = true;
        }

        public virtual void DumpStack()
        {
            if (this.stack == null)
            {
                this.DebugMessage("# Stack dump requested, but stack is null");
            }
            else
            {
                this.DebugMessage("============ Parse Stack Dump ============");
                for (int i = 0; i < this.stack.Size; i++)
                {
                    this.DebugMessage(string.Concat(new object[] { "Symbol: ", ( this.stack.GetAt(i)).Sym, " State: ", ((Symbol) this.stack.GetAt(i)).ParseState }));
                }
                this.DebugMessage("==========================================");
            }
        }

        public abstract int EOF_Symbol { get; }

        protected bool ErrorRecovery(bool debug)
        {
            if (debug)
            {
                this.DebugMessage("# Attempting error recovery");
            }
            if (!this.FindRecoveryConfig(debug))
            {
                if (debug)
                {
                    this.DebugMessage("# Error recovery fails");
                }
                return false;
            }
            this.ReadLookAhead();
            while (true)
            {
                if (debug)
                {
                    this.DebugMessage("# Trying to parse ahead");
                }
                if (this.TryParseAhead(debug))
                {
                    break;
                }
                if (this.lookahead[0].Sym == this.EOF_Symbol)
                {
                    if (debug)
                    {
                        this.DebugMessage("# Error recovery fails at EOF");
                    }
                    return false;
                }
                if (debug)
                {
                    this.DebugMessage("# Consuming Symbol #" + this.lookahead[0].Sym);
                }
                this.RestartLookAhead();
            }
            if (debug)
            {
                this.DebugMessage("# Parse-ahead ok, going back to normal parse");
            }
            this.ParseLookAhead(debug);
            return true;
        }

        public abstract int ErrorSym { get; }

        protected int ErrorSyncSIze => 3;

        protected bool FindRecoveryConfig(bool debug)
        {
            if (debug)
            {
                this.DebugMessage("# Finding recovery state on stack");
            }
            int right = this.stack.Peek()!.Right;
            int left = this.stack.Peek()!.Left;
            while (!this.ShiftUnderError())
            {
                if (debug)
                {
                    this.DebugMessage("# Pop stack by one, state was # " + this.stack!.Peek()!.ParseState);
                }
                left = this.stack.Pop()!.Left;
                this.tos--;
                if (this.stack.IsEmpty)
                {
                    if (debug)
                    {
                        this.DebugMessage("# No recovery state found on stack");
                    }
                    return false;
                }
            }
            int num = this.GetAction(this.stack.Peek()!.ParseState, this.ErrorSym);
            if (debug)
            {
                this.DebugMessage("# Recover state found (#" + this.stack.Peek()!.ParseState + ")");
                this.DebugMessage("# Shifting on error to state #" + (num - 1));
            }
            var item = new Symbol(this.ErrorSym, left, right) {
                ParseState = num - 1,
                UsedByParser = true
            };
            this.stack.Push(item);
            this.tos++;
            return true;
        }

        protected short GetAction(int state, int sym)
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

        protected short GetReduce(int state, int sym)
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

        public Scanner? Scanner =>
            this._scanner;

        protected abstract void InitActions();
        public Symbol? Parse()
        {
            Symbol? item = null;
            this.production_tab = this.ProductionTable;
            this.action_tab = this.ActionTable;
            this.reduce_tab = this.ReduceTable;
            this.InitActions();
            this.UserInit();
            this.cur_token = this.Scan();
            this.stack.Clear();
            this.stack.Push(new (0, this.StartState));
            this.tos = 0;
            this._done_parsing = false;
            while (!this._done_parsing)
            {
                if (this.cur_token.UsedByParser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }
                int num = this.GetAction(this.stack!.Peek()!.ParseState, this.cur_token.Sym);
                if (num > 0)
                {
                    this.cur_token.ParseState = num - 1;
                    this.cur_token.UsedByParser = true;
                    this.stack.Push(this.cur_token);
                    this.tos++;
                    this.cur_token = this.Scan();
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.DoAction(-num - 1, this, this.stack, this.tos);
                        short sym = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.GetReduce(this.stack!.Peek()!.ParseState, sym);
                        item.ParseState = num;
                        item.UsedByParser = true;
                        this.stack.Push(item);
                        this.tos++;
                        continue;
                    }
                    if (num == 0)
                    {
                        this.SyntaxError(this.cur_token);
                        if (!this.ErrorRecovery(false))
                        {
                            this.UnrecoveredSyntaxError(this.cur_token);
                            this.DoneParsing();
                            continue;
                        }
                        item = this.stack.Peek();
                    }
                }
            }
            return item;
        }

        protected void ParseLookAhead(bool debug)
        {
            Symbol? item = null;
            this.lookahead_pos = 0;
            if (debug)
            {
                this.DebugMessage("# Reparsing saved input with actions");
                this.DebugMessage("# Current Symbol is #" + this.CurErrToken.Sym);
                this.DebugMessage("# Current state is #" + this.stack.Peek()!.ParseState);
            }
            while (!this._done_parsing)
            {
                int num = this.GetAction(this.stack.Peek()!.ParseState, this.CurErrToken.Sym);
                if (num > 0)
                {
                    this.CurErrToken.ParseState = num - 1;
                    this.CurErrToken.UsedByParser = true;
                    if (debug)
                    {
                        this.DebugShift(this.CurErrToken);
                    }
                    this.stack.Push(this.CurErrToken);
                    this.tos++;
                    if (!this.AdvanceLookAhead())
                    {
                        if (debug)
                        {
                            this.DebugMessage("# Completed reparse");
                        }
                        break;
                    }
                    if (debug)
                    {
                        this.DebugMessage("# Current Symbol is #" + this.CurErrToken.Sym);
                    }
                }
                else
                {
                    if (num < 0)
                    {
                        item = this.DoAction(-num - 1, this, this.stack, this.tos);
                        short num3 = this.production_tab[-num - 1][0];
                        short num2 = this.production_tab[-num - 1][1];
                        if (debug)
                        {
                            this.DebugReduce(-num - 1, num3, num2);
                        }
                        for (int i = 0; i < num2; i++)
                        {
                            this.stack.Pop();
                            this.tos--;
                        }
                        num = this.GetReduce(this.stack.Peek()!.ParseState, num3);
                        item.ParseState = num;
                        item.UsedByParser = true;
                        this.stack.Push(item);
                        this.tos++;
                        if (debug)
                        {
                            this.DebugMessage("# Goto state #" + num);
                        }
                        continue;
                    }
                    if (num == 0)
                    {
                        this.ReportFatalError("Syntax error", item!);
                        break;
                    }
                }
            }
        }

        public abstract short[][] ProductionTable { get; }

        protected void ReadLookAhead()
        {
            this.lookahead = new Symbol[this.ErrorSyncSIze];
            for (int i = 0; i < this.ErrorSyncSIze; i++)
            {
                this.lookahead[i] = this.cur_token;
                this.cur_token = this.Scan();
            }
            this.lookahead_pos = 0;
        }

        public abstract short[][] ReduceTable { get; }

        public virtual void ReportError(string message, object info)
        {
            Console.Error.Write(message);
            if (info is Symbol)
            {
                if (((Symbol) info).Left != -1)
                {
                    Console.Error.WriteLine(" at character {0} of input", ((Symbol) info).Left);
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

        public virtual void ReportFatalError(string message, object info)
        {
            this.DoneParsing();
            this.ReportError(message, info);
            throw new Exception("Can't recover from previous error(s)");
        }

        protected void RestartLookAhead()
        {
            for (int i = 1; i < this.ErrorSyncSIze; i++)
            {
                this.lookahead[i - 1] = this.lookahead[i];
            }
            this.lookahead[this.ErrorSyncSIze- 1] = this.cur_token;
            this.cur_token = this.Scan();
            this.lookahead_pos = 0;
        }

        public virtual Symbol Scan()
        {
            var symbol = this.Scanner!.NextToken();
            return symbol ?? new Symbol(this.EOF_Symbol);
        }

        public void SetScanner(Scanner s)
        {
            this._scanner = s;
        }

        protected bool ShiftUnderError() => 
            (this.GetAction(this.stack.Peek()!.ParseState, this.ErrorSym) > 0);

        public abstract int StartProduction { get; }
        public abstract int StartState { get; }

        public virtual void SyntaxError(Symbol cur_token)
        {
            this.ReportError("Syntax error", cur_token);
        }

        protected bool TryParseAhead(bool debug)
        {
            var _stack = new VIrtualParseStack(this.stack);
            while (true)
            {
                int num = this.GetAction(_stack.Top(), this.CurErrToken.Sym);
                if (num == 0)
                {
                    return false;
                }
                if (num > 0)
                {
                    _stack.Push(num - 1);
                    if (debug)
                    {
                        this.DebugMessage(string.Concat(new object[] { "# Parse-ahead shifts Symbol #", this.CurErrToken.Sym, " into state #", num - 1 }));
                    }
                    if (!this.AdvanceLookAhead())
                    {
                        return true;
                    }
                }
                else
                {
                    if ((-num - 1) == this.StartProduction)
                    {
                        if (debug)
                        {
                            this.DebugMessage("# Parse-ahead accepts");
                        }
                        return true;
                    }
                    short sym = this.production_tab[-num - 1][0];
                    short num3 = this.production_tab[-num - 1][1];
                    for (int i = 0; i < num3; i++)
                    {
                        _stack.Pop();
                    }
                    if (debug)
                    {
                        this.DebugMessage(string.Concat(new object[] { "# Parse-ahead reduces: handle size = ", num3, " lhs = #", sym, " from state #", _stack.Top() }));
                    }
                    _stack.Push(this.GetReduce(_stack.Top(), sym));
                    if (debug)
                    {
                        this.DebugMessage("# Goto state #" + _stack.Top());
                    }
                }
            }
        }

        protected static short[][] Unpack(short[] sb)
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

        protected static short[][] Unpack(string[] sa)
        {
            var builder = new StringBuilder(sa[0]);
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

        public virtual void UnrecoveredSyntaxError(Symbol cur_token)
        {
            this.ReportFatalError("Couldn't repair and continue parse", cur_token);
        }

        public virtual void UserInit()
        {
        }
    }
}

