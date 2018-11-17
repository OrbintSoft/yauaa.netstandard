//<copyright file="MatchesList.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 7, 27, 11:20</date>
//<summary></summary>

using Antlr4.Runtime.Tree;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    [Serializable]
    public sealed class MatchesList: ICollection<MatchesList.Match>
    {
        private const int CAPACITY_INCREASE = 3;

        private int maxSize = 0;
        private Match[] allElements = null;


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

        

        public IEnumerator<Match> GetEnumerator()
        {
            return new MatchEnumerator(allElements, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatchEnumerator(allElements, Count);
        }
        

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
                result.Add("{ \"" + match.Key + "\"=\"" + match.Value + "\" }");
            }
            return result;
        }

        public void CopyTo(Match[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(allElements[i], arrayIndex);
                arrayIndex = arrayIndex + 1;
            }
        }

        public void Add(Match item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Match item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Match item)
        {
            throw new NotImplementedException();
        }

        public class MatchEnumerator : IEnumerator<Match>
        {
            private readonly int count = 0;
            private readonly Match[] matches;

            private int offset = -1;
            
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

        [Serializable]
        public class Match
        {
            private IParseTree result = null;

            public Match(string key, string value, IParseTree result)
            {
                Fill(key, value, result);
            }

            public string Key { get; private set; } = null;

            public string Value { get; private set; } = null;

            public void Fill(string nKey, string nValue, IParseTree nResult)
            {
                Key = nKey;
                Value = nValue;
                result = nResult;
            }

            public IParseTree GetResult()
            {
                return result;
            }
        }
    }
}