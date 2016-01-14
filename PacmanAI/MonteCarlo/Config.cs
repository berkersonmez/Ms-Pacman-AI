using System;

namespace PacmanAI.MonteCarlo
{
    public static class Config
    {
        public static double DistanceLimit = 50;
        public static double ScoreDecayFactor = 0.9f;
        public static double ExplorationConstant = 1f; // C
        public static int VisitCountTresholdForUct = 15;
        public static int PlayoutTimeLimit = 100000;

        public static Random Rnd = new Random();
    }
}