/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using System.Collections;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze
{
    public class NumberRangeList: IList<int>
    {
        private readonly int start;
        private readonly int end;
        List<int> list = new List<int>();

        public NumberRangeList(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public int this[int index] { get => start + index; set => list[index] = value; }

        public int Count => end - start + 1;

        public bool IsReadOnly => false;

        public void Add(int item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(int item)
        {
            return list.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(int item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, int item)
        {
            list.Insert(index, item);
        }

        public bool Remove(int item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int GetStart()
        {
            return start;
        }

        public int GetEnd()
        {
            return end;
        }
    }
}
