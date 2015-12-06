using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pacman.Simulator;
using Pacman.Simulator.Ghosts;

namespace PacmanAI
{
	public class TunnelPac : BasePacman
	{
		
		public TunnelPac() : base("TunnelPac") { }

		private const bool debug = false;

		public override Direction Think(GameState gs) {

			bool chased = false;
			foreach( Ghost ghost in gs.Ghosts ) {
				if( ghost.Node == null || !ghost.Chasing || !ghost.Entered ) {
					continue;
				}
				Node.PathInfo path = ghost.Node.ShortestPath[gs.Pacman.Node.X, gs.Pacman.Node.Y];
				if( path != null && path.Distance < 6 && path.Direction == ghost.Direction ) {
					chased = true;
				}
			}
			// powerpill taker
			foreach( Node node in gs.Map.Nodes ) {
				if( node.Type == Node.NodeType.PowerPill ) {
					Node.PathInfo path = gs.Pacman.Node.ShortestPath[node.X, node.Y];
					if( path != null ) {
						if( path.Distance < 3 || (chased && path.Distance < 7) ){
							return path.Direction;
						}
					}
				}
			}
			// danger
			double[] dist = { Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity };
			// calculate danger for each direction
			foreach( Ghost ghost in gs.Ghosts ) {
				if( ghost.Node == null ) {
					continue;
				}
				if( ghost.Node.IsSame(gs.Pacman.Node) && gs.Pacman.PossibleDirection(gs.Pacman.Direction) ) {
					return gs.Pacman.Direction;
				}
				Node.PathInfo path = gs.Pacman.Node.ShortestPath[ghost.Node.X, ghost.Node.Y];
				if( path != null ) {
					if( (ghost.Node.ShortestPath[gs.Pacman.Node.X, gs.Pacman.Node.Y].Direction != ghost.InverseDirection(ghost.Direction) || path.Distance < 2) && // heading towards pacman
						path.Distance < dist[(int)path.Direction] &&
						path.Distance < 4 && // magic number
						(ghost.Chasing || ghost.RemainingFlee < 200 || ghost.Entering) ) {
						dist[(int)path.Direction] = path.Distance;
					}				
				}
			}
			// calculate best
			List<Direction> possibleDirections = new List<Direction>();
			for( int i = 0; i < dist.Length; i++ ) {
				if( gs.Pacman.PossibleDirection((Direction)i) && dist[i] == Double.PositiveInfinity ) {
					possibleDirections.Add((Direction)i);
				}
			}
			// 0 => choose least dangerous // still needs more intelligence
			if( possibleDirections.Count == 0 ) {
				Direction bestDirection = Direction.None;
				double bestDistance = 0.0;
				for( int i = 0; i < dist.Length; i++ ) {
					if( gs.Pacman.PossibleDirection((Direction)i) && dist[i] > bestDistance ) {
						bestDirection = (Direction)i;
						bestDistance = dist[i];
					}
				}
				if( debug ) Console.WriteLine("0 options: " + bestDirection);
				return bestDirection;
			}
				// 1 => choose just that
			else if( possibleDirections.Count == 1 ) {
				if( debug ) Console.WriteLine("1 option: " + possibleDirections[0]);
				return possibleDirections[0];
			}
				// 2+ => choose ... ?
			else {
				if( debug ) Console.Write(possibleDirections.Count + " options: ");
				if( debug ) foreach( Direction d in possibleDirections ) Console.Write(d + ", ");
				if( debug ) Console.WriteLine("");
				Node.PathInfo bestPath = null;
				Node bestNode = null;
				// dijkstra avoidance
				StateInfo.PillPath avoidancePoint = null;
				LinkedList<DistanceNode> possibles;
				Prediction prediction = new Prediction(gs, 7);
				// find new target (pill furthest away)			
				StateInfo.PillPath pillPath = StateInfo.FurthestPill(gs.Pacman.Node, gs);
				if( pillPath.PathInfo != null ) {
					avoidancePoint = pillPath; // maybe nearest powerpill (if exists) would be a good target?
				}
				List<DirectionDanger> directionDangers = new List<DirectionDanger>();
				if( avoidancePoint != null ) {
					// run dijkstra to find path							
					foreach( Direction d in gs.Pacman.PossibleDirections() ) {
						// find best direction
						Direction bestDirection = Direction.None;
						float shortestRoute = float.MaxValue;
						int longestTime = int.MaxValue;
						possibles = new LinkedList<DistanceNode>();
						possibles.AddFirst(new LinkedListNode<DistanceNode>(new DistanceNode(gs.Pacman.Node.GetNode(d), 0.0f, d, 1)));
						while( possibles.Count > 0 ) {
							// find next lowest node
							DistanceNode node = possibles.First.Value;
							// found node
							if( node.Node == avoidancePoint.Target ) {
								if( node.Distance < shortestRoute ) {
									shortestRoute = node.Distance;
									longestTime = node.Time;
									bestDirection = d;
								}
								break;
							}
							// pop node
							possibles.Remove(node);
							// add adjacents
							foreach( Node gn in node.Node.GhostPossibles[(int)node.Direction] ) {
								// find danger
								float danger = 0.0f;
								if( node.Time < prediction.Iterations - 1 ) {
									danger = prediction.DangerMaps[node.Time + 1].Danger[node.Node.X, node.Node.Y] +
												prediction.DangerMaps[node.Time + 1].Danger[gn.X, gn.Y];
									if( danger > 1.0f ) {
										danger = 1.0f;
									}
								}
								// create new node
								DistanceNode newNode = new DistanceNode(gn, node.Distance + danger, node.Node.GetDirection(gn), node.Time + 1);
								// past iterations
								if( newNode.Time > prediction.Iterations || gn.Type == Node.NodeType.PowerPill || newNode.Distance == 1.0 ) {									
									if( newNode.Distance < shortestRoute || (newNode.Distance == shortestRoute && newNode.Time > longestTime) ) {
										shortestRoute = newNode.Distance;
										longestTime = newNode.Time;
										bestDirection = d;
									}
									continue;
								}
								LinkedListNode<DistanceNode> curNode = possibles.First;
								while( curNode != null && curNode.Value.Distance < newNode.Distance ) {
									curNode = curNode.Next;
								}
								if( curNode == null ) {
									possibles.AddLast(newNode);
								} else {
									possibles.AddAfter(curNode, newNode);
								}
							}
						}
						directionDangers.Add(new DirectionDanger(d, shortestRoute));
					}
				}
				if( directionDangers.Count > 0 ) {
					List<Direction> newPossibleDirections = new List<Direction>();
					directionDangers.Sort(new Comparison<DirectionDanger>(delegate(DirectionDanger dd1, DirectionDanger dd2) {
						if( dd1.Danger == dd2.Danger ) return 0;
						if( dd1.Danger > dd2.Danger ) return 1;
						return -1;
					}));
					foreach( Direction possible in possibleDirections ) {
						foreach( DirectionDanger dd in directionDangers ) {
							if( dd.Direction == possible ) {
								if( dd.Danger < 0.2 ) {
									newPossibleDirections.Add(possible);
								}
								break;
							}
						}
					}					
					if( newPossibleDirections.Count == 1 ) {
						return newPossibleDirections[0];
					}
					// 0 - return previous best
					if( newPossibleDirections.Count == 0 ) {
						foreach( DirectionDanger dd in directionDangers ) {
							foreach( Direction possible in possibleDirections ) {
								if( dd.Direction == possible ) {
									return possible;
								}
							}
						}						
					}
					possibleDirections = newPossibleDirections;
				}
				// hunt ghosts
				foreach( Ghost ghost in gs.Ghosts ) {
					if( ghost.Node == null || ghost.Chasing || !ghost.Entered ) {
						continue;
					}
					Node.PathInfo ghostPath = ghost.Node.ShortestPath[gs.Pacman.Node.X, gs.Pacman.Node.Y];
					Node.PathInfo path = gs.Pacman.Node.ShortestPath[ghost.Node.X, ghost.Node.Y];
					if( path != null && (path.Distance < 4 || (path.Distance < 7 && ghostPath.Direction == ghost.Direction)) ) {
						if( bestPath == null || path.Distance < bestPath.Distance ) {
							bestPath = path;
						}
					}
				}
				// hunt pills
				if( bestPath == null ) {
					foreach( Node node in gs.Map.Nodes ) {
						if( node.Type != Node.NodeType.Wall ) {
							if( node.Type == Node.NodeType.Pill || node.Type == Node.NodeType.PowerPill ) {
								Node.PathInfo curPath = gs.Pacman.Node.ShortestPath[node.X, node.Y];
								if( curPath != null ) {
									curPath = curPath.Clone();
								} else {
									continue;
								}
								if( curPath.Direction == gs.Pacman.InverseDirection(gs.Pacman.Direction) ) {
									curPath = new Node.PathInfo(curPath.Direction, curPath.Distance + 10);
								}
								List<Node> route = gs.Map.GetRoute(gs.Pacman.Node, node);
								foreach( Node routeNode in route ) {
									if( routeNode.X == 0 && gs.Map.Tunnels[routeNode.Y] ) {
										curPath = new Node.PathInfo(curPath.Direction, curPath.Distance - 15);
										break;
									}
								}
								if( curPath != null && (bestPath == null || curPath.Distance <= bestPath.Distance) ) {
									foreach( Direction d in possibleDirections ) {
										if( d == curPath.Direction ) {
											//if( bestPath == null || curPath.Distance < bestPath.Distance || (bestPath.Distance == curPath.Distance && GameState.Random.NextDouble() < 0.5) ) {
											bestNode = node;
											bestPath = curPath;
											if( debug ) Console.WriteLine("best pill: " + bestPath.Direction);
											break;
											//}
										}
									}
								}
							}
						}
					}
				}
				// return if found
				if( bestPath != null ) {
					if( debug ) Console.WriteLine("best: " + bestPath.Direction);
					return bestPath.Direction;
				}
				// follow same direction if it is in possibles
				foreach( Direction d in possibleDirections ) {
					if( d == gs.Pacman.Direction ) {
						if( debug ) Console.WriteLine("follow direction: " + d);
						return d;
					}
				}
				// otherwise choose randomly (?)
				int chosen = GameState.Random.Next(0, possibleDirections.Count - 1);
				if( debug ) Console.WriteLine("random: " + possibleDirections[chosen]);
				return possibleDirections[chosen];
			}
		}

		public class DirectionDanger
		{
			public readonly Direction Direction;
			public readonly float Danger;

			public DirectionDanger(Direction direction, float danger) {
				this.Direction = direction;
				this.Danger = danger;
			}
		}

		public class DistanceNode
		{
			public readonly Node Node;
			public readonly float Distance;
			public readonly Direction Direction;
			public readonly int Time;

			public DistanceNode(Node node, float distance, Direction direction, int time) {
				this.Node = node;
				this.Distance = distance;
				if( this.Distance > 1.0f ) {
					this.Distance = 1.0f;
				}
				this.Direction = direction;
				this.Time = time;
			}
		}

		public class Sector
		{
			public static GameState GameState;
			private static int indexCount = 0;
			public readonly int Index;
			public readonly int X;
			public readonly int Y;
			public readonly int Width;
			public readonly int Height;
			public int TargetX;
			public int TargetY;
			private double danger = 0;
			public double Danger {
				get { return danger; }
				set { danger = value; }
			}
			private int nodes;			

			private Sector(int x, int y, int width, int height) {
				this.X = x;
				this.Y = y;
				this.Width = width;
				this.Height = height;
				this.nodes = 0;
				for( int mx = x; mx < x + width; mx++ ) {
					for( int my = y; my < y + height; my++ ) {
						if( GameState.Map.Nodes[mx, my].Walkable ) {
							this.nodes++;
						}
					}
				}
				TargetX = x + (int)Math.Round(width / 2f);
				TargetY = y + (int)Math.Round(height / 2f);
				Index = indexCount++;
				string file = "sector" + Index + ".nn";				
			}

			public double[] GetDangerInputs() {
				double[] inputs = new double[4];
				for( int i = 0; i < 4; i++ ) {
					Ghost ghost = GameState.Ghosts[i];
					inputs[i] = (int)Math.Abs(TargetX - ghost.Node.X) + (int)Math.Abs(TargetY - ghost.Node.Y);
					inputs[i] = inputs[i] / 40.0;
				}
				return inputs;
			}

			public double GetDanger() {				
				return danger;
			}

			public static Sector GetSector(int x, int y, int width, int height) {
				return new Sector(x, y, width, height);
			}

			public static Sector Contains(List<Sector> sectors, Node n) {
				foreach( Sector sector in sectors ) {
					if( sector.Contains(n) ) {
						return sector;
					}
				}
				return null;
			}

			public bool Contains(Node n) {
				return (n.X >= X && n.X <= X + Width && n.Y >= Y && n.Y <= Y + Height);
			}

			public double GetNormalizedDanger() {
				return danger / nodes;
			}
		}
	}
}
