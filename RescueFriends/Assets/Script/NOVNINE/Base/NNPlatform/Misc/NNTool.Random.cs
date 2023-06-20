using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;

namespace NOVNINE
{
    public static partial class NNTool 
    {
        static int seed;
        public static bool UseSeed { get; set; }

        public static int Seed {
            get { return seed; }
            set { 
                UseSeed = true;
                seed = value; 
            }
        }

        public static int Rand (int min, int max) {
            if (UseSeed) {
                Debugger.Assert(min <= max, "NNTool.Rand - Min can not be larger than the Max.");

                if (min == max) {
                    return min;
                } else {
                    return (MS_Rand(ref seed) % (max - min)) + min;
                }
            } else {
                return UnityEngine.Random.Range(min, max);
            }
        }

        public static int MS_Rand(ref int _seed) {
            _seed = _seed*0x343fd+0x269EC3;  // a=214013, b=2531011
            return (_seed >> 0x10) & 0x7FFF;
        }

        public static void ExecuteForEachRandomIndex (int beginIndex, int endIndex, Func<int, bool> action) {
            Debugger.Assert((beginIndex >= 0) && (endIndex >= beginIndex),
                    "ExecuteOfEachRandomIndex - Index bigger than zero. Begin index bigger than end index.");

            List<int> indices = new List<int>();

            for (int i = beginIndex; i <= endIndex; i++) indices.Add(i);

            bool continueToRun = true;

            while ((indices.Count > 0) && continueToRun) {
                int index = UnityEngine.Random.Range(0, indices.Count);
                continueToRun = action(indices[index]);
                indices.RemoveAt(index);
            }
        }

        public static int GetUniqueIndex(int max){
            string uid = SystemInfo.deviceUniqueIdentifier;
            byte[] array = System.Text.Encoding.ASCII.GetBytes(uid);
            int sum = 0;
            foreach(var a in array)
                sum += (int)a;
            int pick = ((sum) % max);
            return pick;
        }
    }
}
