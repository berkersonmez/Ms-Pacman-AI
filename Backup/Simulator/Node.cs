using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Pacman.Simulator
{
	public class Node
	{
		public readonly int X, Y;
		public Node Up, Down, Left, Right;
		public List<Node> PossibleDirections;
		public List<List<Node>> GhostPossibles = new List<List<Node>>(4);
		public NodeType Type;		
		private readonly Rectangle rectangle;		
		
		public PathInfo[,] ShortestPath = new PathInfo[Map.Width, Map.Height];		

		public int CenterX { get { return Map.NodeLeftDistance + X * Map.NodeDistance; } }
		public int CenterY { get { return Map.NodeTopDistance + Y * Map.NodeDistance; } }

		public bool Walkable { get { return Type != NodeType.Wall; } }

		public Node(int x, int y, NodeType type) {
			this.X = x;
			this.Y = y;
			this.Type = type;
			this.rectangle = new Rectangle(CenterX - 4, CenterY - 4, 9, 9);
		}

		public void Draw(Graphics g) {
			if( Type == NodeType.None ) {
				g.FillRectangle(Brushes.Black, rectangle);
			}/* else if( Type == NodeType.Wall ) {
				g.FillRectangle(Brushes.Red, rectangle);
			}*/
		}

		public void Draw(Graphics g, Brush brush) {
			g.FillRectangle(brush, rectangle);
		}

		public Node GetNode(Direction direction) {
			switch( direction ) {
				case Direction.Up: return Up;
				case Direction.Down: return Down;
				case Direction.Left: return Left;
				case Direction.Right: return Right;
			}
			return null;
		}

		public Direction GetDirection(Node node) {
			if( node == Up ) return Direction.Up;
			if( node == Down ) return Direction.Down;
			if( node == Left ) return Direction.Left;
			if( node == Right ) return Direction.Right;
			return Direction.None;
		}

		public bool IsSame(Node node) {
			if( node.X == X && node.Y == Y ) {
				return true;
			}
			return false;
		}

		public int ManhattenDistance(Node node) {
			return Math.Abs(X - node.X) + Math.Abs(Y - node.Y);
		}

		public enum NodeType { Pill, PowerPill, None, Wall };

		public class PathInfo
		{
			public readonly Direction Direction;
			public readonly int Distance;

			public PathInfo(Direction direction, int distance) {
				this.Direction = direction;
				this.Distance = distance;
			}

			public PathInfo Clone() {
				return new PathInfo(Direction, Distance);
			}
		}

		public Node Clone() {
			Node n = new Node(X, Y, Type);
			n.Up = Up;
			n.Down = Down;
			n.Left = Left;
			n.Right = Right;
			n.ShortestPath = ShortestPath;
			return n;
		}

		public override string ToString() {
			return "[" + X + "," + Y + "]";
		}
	}
}
