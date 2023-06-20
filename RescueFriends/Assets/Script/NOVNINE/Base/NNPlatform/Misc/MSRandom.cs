using UnityEngine;
using System;
using System.Collections;
using NOVNINE.Diagnostics;


namespace NOVNINE
{
    public class MSRandom 
    {
        private long seed;

#region Constructor
        public MSRandom()
        {   
            DateTime now = DateTime.UtcNow.ToLocalTime();
            DateTime StartOfEpoch = new DateTime(1970, 1, 1).ToLocalTime();
            seed = (long)((now - StartOfEpoch).TotalSeconds);
        }

        public MSRandom(long _seed)
        {
            seed = _seed;
        }
#endregion

#region public 
        public int Next()
        {
            return (int)RandomTable() % Int32.MaxValue;
        }

        public int Next(int max)
        {
            return (int)RandomTable() % max;
        }

        public int Next(int min, int max)
        {
            Debugger.Assert(min <= max, "NOVNINE.MSRandom - Min can not be larget than the Max.");
            return ( (int)RandomTable() % (max - min) ) + min;
        }
#endregion

#region private
        private long RandomTable()
        {
           seed = seed * 0x343fd + 0x269EC3;  // a=214013, b=2531011
           return ( seed >> 0x10 ) & 0x7FFF; 
        }
#endregion

    }
}

