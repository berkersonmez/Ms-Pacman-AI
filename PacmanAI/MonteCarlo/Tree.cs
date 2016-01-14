using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Pacman.Simulator;

namespace PacmanAI.MonteCarlo
{
    public class Tree
    {
        public class Node
        {
            public Pacman.Simulator.Node SimulatorNode;
            public List<double> GhostRewards;
            public List<double> PillRewards;
            public List<double> SurvivalRewards;
            public int VisitCount;
            public List<Node> Children;
            public Node Parent;
            public int ParentDist;
            public bool InTree;
            
            public bool IsLeaf => Children == null || Children.Count == 0;
            public bool IsExpandable => Children == null || Children.Count == 0;

            public Node()
            {
                Children = new List<Node>();
                GhostRewards = new List<double>();
                PillRewards = new List<double>();
                SurvivalRewards = new List<double>();
            }

            // Mean(SpTactic)
            public double MeanReward(string tactic)
            {
                List<double> rewards = RewardsFromTactic(tactic);
                // SpTactic / N
                return (rewards.Sum() / rewards.Count);
            }

            public double MaximumMeanReward(string tactic)
            {
                if (IsLeaf)
                {
                    return MeanReward(tactic);
                }
                if (!InTree)
                {
                    return double.NegativeInfinity;
                }
                return MaximumMeanRewardOfChildren(tactic);
            }

            public double MaximumMeanRewardOfChildren(string tactic)
            {
                return Children.Max(c => c.MaximumMeanReward(tactic));
            }

            public List<double> RewardsFromTactic(string tactic)
            {
                if (tactic == "ghost")return GhostRewards;
                if (tactic == "pill") return PillRewards;
                if (tactic == "survival") return SurvivalRewards;
                return null;
            }

            public double V(string tactic)
            {
                if (tactic == "ghost") return MaximumMeanReward("ghost") * MaximumMeanReward("survival");
                if (tactic == "pill") return MaximumMeanReward("pill") * MaximumMeanReward("survival");
                return MaximumMeanReward("survival");
            }

            public void Decay()
            {
                for (int i = 0; i < GhostRewards.Count; i++)
                {
                    GhostRewards[i] = GhostRewards[i]*Config.ScoreDecayFactor;
                }
                for (int i = 0; i < PillRewards.Count; i++)
                {
                    PillRewards[i] = PillRewards[i] * Config.ScoreDecayFactor;
                }
                for (int i = 0; i < SurvivalRewards.Count; i++)
                {
                    SurvivalRewards[i] = SurvivalRewards[i] * Config.ScoreDecayFactor;
                }
            }

            public Node SelectionPolicy(string tactic)
            {
                if (Children.Any(c => c.VisitCount < Config.VisitCountTresholdForUct))
                {
                    // Uniform random selection
                    int r = Config.Rnd.Next(Children.Count);
                    return Children[r];
                }
                // Upper Confidence Bound
                Node currentSelection = null;
                double currentXi = 0;
                foreach (var child in Children)
                {
                    double xi = child.V(tactic) + (Config.ExplorationConstant * Math.Sqrt(Math.Log(VisitCount) / child.VisitCount));
                    if (currentSelection == null || xi > currentXi)
                    {
                        currentSelection = child;
                        currentXi = xi;
                    }
                }
                return currentSelection;
            }

            public void PropageteTree()
            {
                for (int i = 0; i < GhostRewards.Count; i++)
                {
                    GhostRewards[i] = GhostRewards[i] * Config.ScoreDecayFactor;
                }
                for (int i = 0; i < PillRewards.Count; i++)
                {
                    PillRewards[i] = PillRewards[i] * Config.ScoreDecayFactor;
                }
                for (int i = 0; i < SurvivalRewards.Count; i++)
                {
                    SurvivalRewards[i] = SurvivalRewards[i] * Config.ScoreDecayFactor;
                }
            }
        }

        public Node Root;

        public List<Node> AllNodes;

        public void ConstructNewTree(GameState gs)
        {
            AllNodes = new List<Node>();
            Root = new Node();
            Root.SimulatorNode = gs.Pacman.Node;
            AllNodes.Add(Root);
            //ConstructNodeChildren(Root, 0);
        }

        public void ReconstructTree(GameState gs)
        {
            Node newRoot = AllNodes.First(n => n.SimulatorNode == gs.Pacman.Node);
            Node oldRoot = newRoot.Parent;
            Root = newRoot;
            newRoot.Parent = null;
            newRoot.Children.Add(oldRoot);
            int distDiff = newRoot.ParentDist;
            foreach (var child in newRoot.Children.Where(c => c != oldRoot))
            {
                UpdateParentDists(child, distDiff);
            }
            UpdateParentDists(oldRoot, distDiff);
            //foreach (Node node in AllNodes.Where(n => n.IsLeaf))
            //{
            //    ConstructNodeChildren(node, node.ParentDist);
            //}
        }

        public void UpdateParentDists(Node node, int distDiff)
        {
            node.ParentDist += distDiff;
            foreach (var child in node.Children)
            {
                UpdateParentDists(child, distDiff);
            }
        }

        public Direction FavoredDirection()
        {
            return Direction.None;
        }

        public void ConstructNodeChildren(Node node)
        {
            List<Pacman.Simulator.Node> posDirs = node.SimulatorNode.PossibleDirections;
            foreach (var posDir in posDirs)
            {
                Direction prevDir;
                int dist = node.ParentDist;
                var junction = GoToJunction(node.SimulatorNode, node.SimulatorNode.GetDirection(posDir), out prevDir, ref dist);
                if (dist > Config.DistanceLimit) continue;
                if (node.Parent.SimulatorNode == junction) continue;
                var child = new Node();
                child.SimulatorNode = junction;
                child.Parent = node;
                child.ParentDist = dist;
                node.Children.Add(child);
                //ConstructNodeChildren(child, dist);
            }
        }

        public void Update(MonteCarloPac.Rewards rewards)
        {
            ConstructNodeChildren(Root);
        }

        public Pacman.Simulator.Node GoToJunction(Pacman.Simulator.Node fromNode, Direction dir, out Direction prevDir, ref int dist)
        {
            Direction currentDir = dir;
            var currentNode = fromNode.GetNode(dir);
            var lastNode = fromNode;
            prevDir = RevertDirection(dir);
            dist += Map.NodeDistance;
            while (currentNode.Type != Pacman.Simulator.Node.NodeType.Wall)
            {
                if (currentNode.PossibleDirections.Count > 2)
                {
                    return currentNode;
                }
                dist += Map.NodeDistance;
                lastNode = currentNode;
                currentNode = currentNode.GetNode(currentDir);
            }
            var nextNode = lastNode.PossibleDirections.First(ln => ln.GetDirection(lastNode) != currentDir);
            return GoToJunction(lastNode, lastNode.GetDirection(nextNode), out prevDir, ref dist);
        }

        public Direction RevertDirection(Direction dir)
        {
            switch (dir)
            {
                    case Direction.Down:
                    return Direction.Up;
                    case Direction.Up:
                    return Direction.Down;
                    case Direction.Left:
                    return Direction.Right;
                    case Direction.Right:
                    return Direction.Left;
            }
            return Direction.None;
        }
    }
}