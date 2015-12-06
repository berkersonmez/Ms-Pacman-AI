using System;
using System.Collections.Generic;
using System.Text;

namespace Pacman.Simulator.Ghosts
{
	public class Red : Ghost
	{
		public const int StartX = 111, StartY = 93;

		public Red(int x, int y, GameState gameState)
			: base(x, y, gameState) {
			this.name = "Red";
			ResetPosition();
		}

		public override void PacmanDead() {
			waitToEnter = 0;
			ResetPosition();			
		}

		public override void ResetPosition() {
			x = StartX;
			y = StartY;
			waitToEnter = 0;
			direction = Direction.Left;
			base.ResetPosition();
			entered = true;
		}

		public override void Move() {
			if( Distance(gameState.Pacman) > randomMoveDist && GameState.Random.Next(0, randomMove) == 0 ) {
				MoveRandom();
			} else {
				MoveAsRed();
			}
			base.Move();
		}
	}
}
