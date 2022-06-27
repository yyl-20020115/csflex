﻿namespace JavaCup
{
    using System;

    public class BitSet
    {
        private uint[] bits;

        public BitSet(int capacity)
        {
            this.bits = new uint[(capacity + 0x1f) / 0x20];
        }

        public void AndNot(BitSet other)
        {
            for (int i = 0; (i < this.bits.Length) && (i < other.bits.Length); i++)
            {
                uint[] numArray;
                IntPtr ptr;
                (numArray = this.bits)[(int) (ptr = (IntPtr) i)] = numArray[(int) ptr] & ~other.bits[i];
            }
        }

        public void Clear(int idx)
        {
            uint[] numArray;
            IntPtr ptr;
            int num = idx / 0x20;
            int num2 = idx & 0x1f;
            uint num3 = ((uint) 1) << num2;
            (numArray = this.bits)[(int) (ptr = (IntPtr) num)] = numArray[(int) ptr] & ~num3;
        }

        public BitSet Clone()
        {
            BitSet set = new BitSet(this.bits.Length * 0x20);
            Array.Copy(this.bits, set.bits, this.bits.Length);
            return set;
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !(obj is BitSet set))
            {
                return false;
            }

            if (this.bits.Length != set.bits.Length)
            {
                return false;
            }
            for (int i = 0; i < this.bits.Length; i++)
            {
                if (this.bits[i] != set.bits[i])
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
            return ((this.bits[index] & num3) != 0);
        }

        public override int GetHashCode()
        {
            int num = 0;
            for (int i = 0; i < this.bits.Length; i++)
            {
                num ^= (int) this.bits[i];
            }
            return num;
        }

        public void Or(BitSet other)
        {
            for (int i = 0; (i < this.bits.Length) && (i < other.bits.Length); i++)
            {
                this.bits[i] |= other.bits[i];
            }
        }

        public void Set(int idx)
        {
            int index = idx / 0x20;
            int num2 = idx & 0x1f;
            uint num3 = ((uint) 1) << num2;
            this.bits[index] |= num3;
        }

        public void Xor(BitSet other)
        {
            for (int i = 0; (i < this.bits.Length) && (i < other.bits.Length); i++)
            {
                this.bits[i] ^= other.bits[i];
            }
        }
    }
}

