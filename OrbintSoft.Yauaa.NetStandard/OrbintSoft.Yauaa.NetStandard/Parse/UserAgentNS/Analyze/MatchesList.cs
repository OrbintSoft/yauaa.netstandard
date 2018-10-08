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

using Antlr4.Runtime.Tree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    public sealed class MatchesList: ICollection<MatchesList.Match>
    {
        public class Match
        {
            private string key;
            private string value;
            private IParseTree result;

            public Match(string key, string value, IParseTree result)
            {
                Fill(key, value, result);
            }

            public void Fill(string nKey, string nValue, IParseTree nResult)
            {
                key = nKey;
                value = nValue;
                result = nResult;
            }

            public string GetKey()
            {
                return key;
            }

            public string GetValue()
            {
                return value;
            }

            public IParseTree GetResult()
            {
                return result;
            }
        }

        private int maxSize;

        private Match[] allElements;

        public MatchesList(int newMaxSize)
        {
            maxSize = newMaxSize;

            Count = 0;
            allElements = new Match[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                allElements[i] = new Match(null, null, null);
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly => true;



        public void Clear()
        {
            Count = 0;
        }

        public bool Add(string key, string value, IParseTree result)
        {
            if (Count >= maxSize)
            {
               IncreaseCapacity();               
            }

            allElements[Count].Fill(key, value, result);
            Count++;
            return true;
        }

        public class MatchEnumerator : IEnumerator<Match>
        {
            private int offset = -1;
            private readonly int count = 0;

            private readonly Match[] matches;

            public Match Current => matches[offset];

            object IEnumerator.Current => matches[offset];

            public MatchEnumerator(Match[] matches, int count)
            {
                this.matches = matches;
                this.count = count;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (offset < count - 1)
                {
                    offset++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                offset = -1;
            }
        }

        public IEnumerator<Match> GetEnumerator()
        {
            return new MatchEnumerator(allElements, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatchEnumerator(allElements, Count);
        }

        private static readonly int CAPACITY_INCREASE = 3;

        private void IncreaseCapacity()
        {
            int newMaxSize = maxSize + CAPACITY_INCREASE;
            Match[] newAllElements = new Match[newMaxSize];
            allElements.CopyTo(newAllElements, 0);
            for (int i = maxSize; i < newMaxSize; i++)
            {
                newAllElements[i] = new Match(null, null, null);
            }
            allElements = newAllElements;
            maxSize = newMaxSize;
        }

        public List<string> ToStrings()
        {
            List<string> result = new List<string>();
            foreach (Match match in this)
            {
                result.Add("{ \"" + match.GetKey() + "\"=\"" + match.GetValue() + "\" }");
            }
            return result;
        }


        public void Add(Match item)
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(Match item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(Match[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
            //allElements.CopyTo(array, arrayIndex);
        }

        public bool Remove(Match item)
        {
            throw new System.NotImplementedException();
        }  
    }
}