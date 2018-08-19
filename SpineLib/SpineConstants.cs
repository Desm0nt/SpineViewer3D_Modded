using System.Collections.Generic;

namespace SpineLib
{
    public static class SpineConstants
    {
        public static readonly List<string> SpineNames = new List<string>  { "C1", "C2", "C3", "C4", "C5", "C6", "C7",
                                                      "Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7","Th8", "Th9", "Th10", "Th11", "Th12",
                                                      "L1", "L2", "L3", "L4", "L5",
                                                      "S1", "S2", "S3", "S4", "S5",
                                                      "Co1", "Co2", "Co3", "Co4", "Co5"};

        public static readonly List<string> InterSpineNames;

        static SpineConstants() {
            List<string> lst = new List<string>();
            for (int i = 0; i < SpineNames.Count - 1; i++)
            {
                lst.Add(SpineNames[i] + "-" + SpineNames[i + 1]);
            }
            InterSpineNames = lst;
        }
    }
}
