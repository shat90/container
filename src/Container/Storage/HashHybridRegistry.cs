﻿using System;
using Unity.Utility;

namespace Unity.Container.Storage
{
    public class HashHybridRegistry<TKey, TValue> : IHybridRegistry<TKey, TValue>
    {
        #region Constants

        private const float LoadFactor = 0.72f;

        #endregion


        #region Fields

        public readonly int[] Buckets;
        public readonly Entry[] Entries;
        public int Count;

        #endregion


        #region Constructors

        public HashHybridRegistry(int capacity)
        {
            var size = Prime.GetPrime(capacity);
            Buckets = new int[size];
            Entries = new Entry[size];

            for (var i = 0; i < Buckets.Length; i++) Buckets[i] = -1;
        }

        public HashHybridRegistry(HashHybridRegistry<TKey, TValue> dictionary)
        {
            var size = Prime.GetPrime(dictionary.Entries.Length * 2);

            Buckets = new int[size];
            Entries = new Entry[size];
            for (var i = 0; i < Buckets.Length; i++) Buckets[i] = -1;

            Array.Copy(dictionary.Entries, 0, Entries, 0, dictionary.Count);
            for (var i = 0; i < dictionary.Count; i++)
            {
                var hashCode = Entries[i].HashCode;
                if (hashCode < 0) continue;

                var bucket = hashCode % size;
                Entries[i].Next = Buckets[bucket];
                Buckets[bucket] = i;
            }
            Count = dictionary.Count;
            dictionary.Count = 0;
        }

        #endregion


        #region IHybridRegistry

        public TValue this[TKey key]
        {
            get
            {
                var hashCode = null == key ? 0 : key.GetHashCode() & 0x7FFFFFFF;
                for (var i = Buckets[hashCode % Buckets.Length]; i >= 0; i = Entries[i].Next)
                {
                    if (Entries[i].HashCode == hashCode && Equals(Entries[i].Key, key)) return Entries[i].Value;
                }

                return default(TValue);
            }

            set
            {
                var hashCode = null == key ? 0 : key.GetHashCode() & 0x7FFFFFFF;
                var targetBucket = hashCode % Buckets.Length;

                for (var i = Buckets[targetBucket]; i >= 0; i = Entries[i].Next)
                {
                    if (Entries[i].HashCode == hashCode && Equals(Entries[i].Key, key))
                    {
                        Entries[i].Value = value;
                        return;
                    }
                }

                Entries[Count].HashCode = hashCode;
                Entries[Count].Next = Buckets[targetBucket];
                Entries[Count].Key = key;
                Entries[Count].Value = value;
                Buckets[targetBucket] = Count;
                Count++;
            }
        }

        public bool RequireToGrow => (Entries.Length - Count) < 100 &&
                                     (float)Count / Entries.Length > LoadFactor;

        #endregion


        #region Nested Types

        public struct Entry
        {
            public int HashCode;
            public int Next;
            public TKey Key;
            public TValue Value;
        }

        #endregion
    }
}
