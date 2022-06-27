namespace CSFlex
{
    using System;

    public class BitSet
    {
        private uint[] bits;
        public uint[] Bits => this.Bits;

        public BitSet(int capacity)
        {
            this.bits = new uint[(capacity + 0x1f) / 0x20];
        }


        public void AndNot(BitSet other)
        {
            for (int i = 0; (i < this.Bits.Length) && (i < other.Bits.Length); i++)
            {
                uint[] numArray;
                IntPtr ptr;
                (numArray = this.Bits)[(int) (ptr = (IntPtr) i)] = numArray[(int) ptr] & ~other.Bits[i];
            }
        }

        public void Clear(int idx)
        {
            uint[] numArray;
            IntPtr ptr;
            int num = idx / 0x20;
            int num2 = idx & 0x1f;
            uint num3 = ((uint) 1) << num2;
            (numArray = this.Bits)[(int) (ptr = (IntPtr) num)] = numArray[(int) ptr] & ~num3;
        }

        public BitSet Clone()
        {
            var set = new BitSet(this.Bits.Length * 0x20);
            Array.Copy(this.Bits, set.Bits, this.Bits.Length);
            return set;
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !(obj is BitSet set))
            {
                return false;
            }

            if (this.Bits.Length != set.Bits.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Bits.Length; i++)
            {
                if (this.Bits[i] != set.Bits[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool Get(int idx)
        {
            int index = idx / 0x20;
            int num2 = idx & 0x1f;
            uint num3 = ((uint) 1) << num2;
            return ((this.Bits[index] & num3) != 0);
        }

        public override int GetHashCode()
        {
            int num = 0;
            for (int i = 0; i < this.Bits.Length; i++)
            {
                num ^= (int) this.Bits[i];
            }
            return num;
        }

        public void Or(BitSet other)
        {
            for (int i = 0; (i < this.Bits.Length) && (i < other.Bits.Length); i++)
            {
                this.Bits[i] |= other.Bits[i];
            }
        }

        public void Set(int idx)
        {
            int index = idx / 0x20;
            int num2 = idx & 0x1f;
            uint num3 = ((uint) 1) << num2;
            this.Bits[index] |= num3;
        }

        public void Xor(BitSet other)
        {
            for (int i = 0; (i < this.Bits.Length) && (i < other.Bits.Length); i++)
            {
                this.Bits[i] ^= other.Bits[i];
            }
        }
    }
}

