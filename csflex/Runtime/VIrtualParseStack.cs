namespace CSFlex.Runtime
{
    using CSFlex;
    using System;

    public class VIrtualParseStack
    {
        protected JCStack<Symbol> stack;
        protected int next = 0;
        protected JCStack<int> vstack = new();

        public VIrtualParseStack(JCStack<Symbol> shadowing_stack)
        {
            this.stack = shadowing_stack ?? throw new Exception("Internal parser error: attempt to create null virtual stack");
            this.LoadFromReal();
        }

        public bool IsEmpty => this.vstack.IsEmpty;

        protected void LoadFromReal()
        {
            if (this.next < this.stack.Size)
            {
                var symbol = this.stack.GetAt((this.stack.Size - 1) - this.next);
                this.next++;
                this.vstack.Push(symbol.ParseState);
            }
        }

        public void Pop()
        {
            if (this.IsEmpty)
            {
                throw new Exception("Internal parser error: pop from empty virtual stack");
            }
            this.vstack.Pop();
            if (this.IsEmpty)
            {
                this.LoadFromReal();
            }
        }

        public void Push(int state_num)
        {
            this.vstack.Push(state_num);
        }

        public int Top()
        {
            if (this.IsEmpty)
            {
                throw new Exception("Internal parser error: top() called on empty virtual stack");
            }
            return this.vstack.Peek();
        }
    }
}

