using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Pacman.Simulator
{
	public class Pacman : Entity
	{
		public const int StartX = 111, StartY = 189;

		private int counter;
		private int score;
		public int Score { get { return score; } }
		private int lives;
		public int Lives { get { return lives; } set { lives = value; } }
		private bool gotExtraLife;		
		private int eatGhostBonus;

		public Pacman(int x, int y, GameState gameState)
			: base(x, y, gameState) {
			Speed = 3.0f;
			Reset();
		}

		public void SetPosition(int x, int y, Direction direction) {
			this.x = x;
			this.y = y;
			this.direction = direction;
			// eat all pills on path from last node
			Node curNode = node;
			Node nextNode = gameState.Map.GetNode(X, Y);			
			if( nextNode.Type == Node.NodeType.Wall ) {
				//nextNode = curNode;
				throw new ApplicationException("You cannot set your destination to a wall");
			}
			if( curNode.ShortestPath[nextNode.X, nextNode.Y] != null ) {
				while( curNode != nextNode ) {
					curNode = curNode.GetNode(curNode.ShortestPath[nextNode.X, nextNode.Y].Direction);
					if( curNode.Type == Node.NodeType.Pill || curNode.Type == Node.NodeType.PowerPill ) {
						gameState.Map.PillsLeft--;
						curNode.Type = Node.NodeType.None;
					}
				}
				// set new node
				node = nextNode;
			}
		}

		public void SetDirection(Direction direction) {
			NextDirection = direction;
		}

		public void Die() {
			lives--;
			if( gameState.Controller != null ) {
				gameState.Controller.EatenByGhost();
			}
			ResetPosition();
		}

		public void Reset() {
			lives = 2;
			score = 0;
			gotExtraLife = false;
			counter = 0;
			ResetPosition();
		}

		public void ResetPosition() {
			x = StartX;
			y = StartY;
			node = gameState.Map.GetNode(X, Y);
			direction = Direction.Left;
			NextDirection = Direction.Left;
		}

		public void EatGhost() {		
			score += eatGhostBonus;
			eatGhostBonus *= 2;
			if( gameState.Controller != null ) {
				gameState.Controller.EatGhost();
			}
		}

		protected override void ProcessNode() {
			if( Node.Type == Node.NodeType.Pill || Node.Type == Node.NodeType.PowerPill ) {
				if( Node.Type == Node.NodeType.PowerPill ) {
					foreach( Ghosts.Ghost g in gameState.Ghosts ) {
						g.Flee();
					}
					eatGhostBonus = 200;
					score += 50;
					if( gameState.Controller != null ) {
						gameState.Controller.EatPowerPill();
					}
					gameState.ReverseGhosts();
				} else {
					score += 10;
					if( gameState.Controller != null ) {
						gameState.Controller.EatPill();
					}
				}
				Node.Type = Node.NodeType.None;
				gameState.Map.PillsLeft--;				
				if( score >= 10000 && !gotExtraLife ) {
					gotExtraLife = true;
					lives++;
				}
			}
		}

		public override void Draw(Graphics g, Image sprites) {
			int offset = 0;
			if( counter % 4 < 2 ) {
				offset = Width;
			}
			switch( Direction ) {				
				case Direction.Down: offset += Width * 2; break;
				case Direction.None:
				case Direction.Left: offset += Width * 4; break;
				case Direction.Right: offset += Width * 6; break;
			}
			g.DrawImage(sprites, new Rectangle(ImgX, ImgY, 14, 14), new Rectangle(offset, 0, 13, 14), GraphicsUnit.Pixel);
			counter++;
		}
	}
}
