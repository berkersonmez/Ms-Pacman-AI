using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Pacman.Simulator;
using Pacman.Simulator.Ghosts;

namespace PacmanAI
{
    public class SmartPac : BasePacman
	{

		public SmartPac()
            : base("SmartPac"){			
		}

		private bool debug = false;

		public override Direction Think( GameState gs ) {
			double[] dist = { Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity };
			// calculate danger for each direction
			foreach (Ghost ghost in gs.Ghosts) {
				if( ghost.Node == null ) {
					continue;
				}
				Node.PathInfo path = gs.Pacman.Node.ShortestPath[ghost.Node.X, ghost.Node.Y];
				if( path != null ) {
					if( ( ghost.Node.ShortestPath[gs.Pacman.Node.X,gs.Pacman.Node.Y].Direction != ghost.InverseDirection(ghost.Direction) || path.Distance < 2 ) && // heading towards pacman
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
				// hunt ghosts
				if( false ) {
					foreach( Pacman.Simulator.Ghosts.Ghost ghost in gs.Ghosts ) {
						if( !ghost.Chasing ) { // && ghost.Entered
							//Console.WriteLine(ghost.Name + " edible");
							Node.PathInfo curPath = gs.Pacman.Node.ShortestPath[ghost.Node.X, ghost.Node.Y];
							if( curPath != null && curPath.Distance < 25 && ghost.RemainingFlee > 10
								&& (bestPath == null || curPath.Distance < bestPath.Distance) ) {
								foreach( Direction d in possibleDirections ) {
									if( d == curPath.Direction ) {
										bestPath = curPath;
										bestNode = ghost.Node;
										if( debug )
											Console.WriteLine("best ghost: " + ghost.Name + " > " + bestPath.Direction + " : " + ghost.Node.X + "," + ghost.Node.Y);
										break;
									}
								}
							}
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
									curPath = new Node.PathInfo(curPath.Direction, curPath.Distance + 1);
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
				foreach( Direction d in possibleDirections ){
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
	}
}
