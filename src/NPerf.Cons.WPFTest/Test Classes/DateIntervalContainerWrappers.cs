using System;

namespace NPerf.Cons.WPFTest.Test_Classes
{
    public class DateRBIntervalTree : Orc.Entities.IntervalTreeRB.IntervalTree<DateTime> { }
    public class DateAVLIntervalTree : Orc.Entities.IntervalTreeAVL.IntervalTree<DateTime> { }
    public class DateRangeIntervalTree : Orc.Entities.RangeTree.RangeTree<DateTime> { }
}
