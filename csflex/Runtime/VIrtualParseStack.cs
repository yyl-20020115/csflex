namespace CSFlex.Runtime
{
    using CSFlex;
    using System;

    public class VIrtualParseStack
    {
        protected JCStack<Symbol> real_stack;
        protected int real_next;
        protected JCStack<int> vstack;

        public VIrtualParseStack(JCStack<Symbol> shadowing_stack)
        {
            this.real_stack = shadowing_stack ?? throw new Exception("Internal parser error: attempt to create null virtual stack");
            this.vstack = new ();
            this.real_next = 0;
            this.get_from_real();
        }

        public bool IsEmpty => this.vstack.IsEmpty;

        protected void get_from_real()
        {
            if (this.real_next < this.real_stack.Size)
            {
                Symbol symbol = this.real_stack.GetAt((this.real_stack.Size - 1) - this.real_next);
                this.real_next++;
                this.vstack.Push(symbol.parse_state);
            }
        }

        public void pop()
        {
            if (this.IsEmpty)
            {
                throw new Exception("Internal parser error: pop from empty virtual stack");
            }
            this.vstack.Pop();
            if (this.IsEmpty)
            {
                this.get_from_real();
            }
        }

        public void push(int state_num)
        {
            this.vstack.Push(state_num);
        }

        public int top()
        {
            if (this.IsEmpty)
            {
                throw new Exception("Internal parser error: top() called on empty virtual stack");
            }
            return (int) this.vstack.Peek();
        }
    }
}

