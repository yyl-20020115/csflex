namespace CSFlex
{
    public class JCStack<T>
    {
        private List<T> data = new ();

        public void Clear()
        {
            this.data.Clear();
        }

        public T GetAt(int idx) => 
            this.data[idx];

        public bool IsEmpty =>
            (this.data.Count == 0);

        public T? Peek() => 
            this.data.Count>0 ? this.data[^1] : default(T);

        public T? Pop()
        {
            T? o;
            try
            {
                o = this.Peek();
            }
            finally
            {
                this.data.RemoveAt(this.data.Count - 1);
            }
            return o;
        }

        public void Push(T item)
        {
            this.data.Add(item);
        }

        public void SetAt(T new_item, int idx)
        {
            this.data[idx] = new_item;
        }

        public int Size =>
            this.data.Count;
    }
}

