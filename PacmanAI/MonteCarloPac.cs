using System;
using System.Collections.Generic;
using System.Xml;
using Pacman.Simulator;
using PacmanAI.MonteCarlo;

namespace PacmanAI
{
    public class MonteCarloPac : BasePacman
    {
        public class Rewards
        {
            public double RGhost;
            public double RPill;
            public double RSurvival;
        }

        public string Tactic;

        public GameState Gs;

        public static bool GameOver;

        public Tree Tree;

        public MonteCarloPac() : base("Monte Carlo Tree Search Pacman")
        {
            Tree = new Tree();

        }

        public override Direction Think(GameState gs)
        {
            if (Tree != null)
                Tree.ConstructNewTree(gs);

            MCTS(Tree.Root);

            return Tree.FavoredDirection();
        }

        public void MCTS(Tree.Node p)
        {
            if (p.ParentDist > Config.DistanceLimit)
            {
                Playout(p);
                return;
            }
            else if (p.IsExpandable)
            {
                Tree.ConstructNodeChildren(p);
                Rewards rewards = Playout(p);
                Tree.Update(rewards);
            }
            else
            {
                Tree.Node nextNode = p.SelectionPolicy(Tactic);
                MCTS(nextNode);
                p.PropageteTree();
            }
        }

        public Rewards Playout(Tree.Node p)
        {
            GameState gs = new GameState();
            BasePacman controller = new SmartDijkstraPac();
            gs.StartPlay();
            GameOver = false;
            gs.GameOver += new EventHandler(GameOverHandler);
            gs.Controller = controller;
            int timePassed = 0;
            int numberOfPillsEaten = 0;
            int totalEdiblePills = Gs.Map.PillNodes.Count;
            int numberOfGhostsEaten = 0;
            float totalEdibleTime = 0; 
            while (Config.PlayoutTimeLimit > timePassed)
            {
                Direction direction = controller.Think(gs);
                gs.Pacman.SetDirection(direction);
                gs.Update();
                if (GameOver == true)
                {
                    break;
                }
            }
            Rewards rewards = new Rewards();
            rewards.RSurvival = GameOver ? 0 : 1;
            rewards.RGhost = (double) numberOfPillsEaten/totalEdiblePills;
            rewards.RGhost = (double)numberOfGhostsEaten / totalEdibleTime;
            return rewards;
        }

        private static void GameOverHandler(object sender, EventArgs args)
        {
            GameOver = true;
        }

        Random rand = new Random(DateTime.Now.Millisecond);

        public static int NumberOfPlayouts = 5000;

        public IGame.IMove MakeDecision(IGame game)
        {
            float bestResult = float.MinValue;

            List<IGame.IMove> potentialMoves = PreprocessMoves(game, game.GetValidMoves());
            if (potentialMoves.Count == 0)
            {
                List<IGame.IMove> C = game.GetValidMoves();
                return C[0];
            }

            if (potentialMoves.Count == 1)
            {
                return potentialMoves[0];
            }

            Dictionary<float, List<IGame.IMove>> moves = new Dictionary<float, List<IGame.IMove>>();

            foreach (IGame.IMove move in potentialMoves)
            {

                float currentResult = Expand(game, move);

                Console.WriteLine("Move: " + move + " Value: " + currentResult);

                List<IGame.IMove> list;
                if (moves.TryGetValue(currentResult, out list))
                    list.Add(move);
                else
                {
                    list = new List<IGame.IMove>();
                    list.Add(move);
                    moves.Add(currentResult, list);
                }

                if (currentResult > bestResult)
                    bestResult = currentResult;
            }

            // randomize best move
            int c = moves[bestResult].Count;
            if (c == 1)
                return moves[bestResult][0];
            else
                return moves[bestResult][rand.Next(c)];
        }

        float Expand(IGame game, IGame.IMove move)
        {
            int value = 0;

            for (int play = 0; play < NumberOfPlayouts; ++play)
            {
                IGame playout = game.Clone();

                playout.Move(move);

                IGame.State Winner = Simulate(playout);

                if (Winner == game.CurrentPlayer)
                    value++;
                else if (Winner == game.CurrentOpponent)
                    value--;
                else if (rand.Next(2) == 0)
                    value++;
                else
                    value--;
            }

            return (value / (float)NumberOfPlayouts);
        }

        IGame.State Simulate(IGame game)
        {
            int move;
            while (game.PollState() == IGame.State.Unknown)
            {
                List<IGame.IMove> validMoves = game.GetValidMoves();

                move = rand.Next(0, validMoves.Count - 1);

                game.Move(validMoves[move]);
            }

            return game.PollState();
        }

        public static List<IGame.IMove> PreprocessMoves(IGame game, List<IGame.IMove> TestCandidates)
        {
            List<IGame.IMove> CandidateMoves = new List<IGame.IMove>();

            foreach (IGame.IMove Move in TestCandidates)
            {
                IGame CheckWin = game.Clone();

                // test if immediate win possible
                CheckWin.Move(Move);

                if (CheckWin.PollState() == game.CurrentPlayer)
                {
                    CandidateMoves.Add(Move);
                    return CandidateMoves;
                }
                else
                {
                    // test if we make an assist for our opponent
                    List<IGame.IMove> OpponentMoves = CheckWin.GetValidMoves();
                    bool IsBad = false;
                    foreach (IGame.IMove OMove in OpponentMoves)
                    {
                        IGame Tmp = CheckWin.Clone();

                        Tmp.Move(OMove);

                        // did we make an assist?
                        if (Tmp.PollState() == game.CurrentOpponent)
                        {
                            // forget the move!
                            IsBad = true;
                            break;
                        }
                    }
                    if (!IsBad)
                    {
                        CandidateMoves.Add(Move);
                    }
                }
            }
            return CandidateMoves;
        }
    }
}