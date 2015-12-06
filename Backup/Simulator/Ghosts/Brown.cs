using System;
using System.Collections.Generic;
using System.Text;

namespace Pacman.Simulator.Ghosts
{
	public class Brown : Ghost
	{
		public const int StartX = 127, StartY = 118;
		private const int firstWaitToEnter = 20, secondWaitToEnter = 30;

		public Brown(int x, int y, GameState gameState)
			: base(x, y, gameState) {
			this.name = "Brown";
			this.drawOffset = Height * 3;
			ResetPosition();
		}

		public override void PacmanDead() {
			waitToEnter = secondWaitToEnter;
			ResetPosition();
		}

		public override void ResetPosition() {
			x = StartX;
			y = StartY;
			waitToEnter = firstWaitToEnter;
			direction = Direction.Up;
			base.ResetPosition();
		}

		public override void Move() {
			MoveRandom();
			base.Move();
		}
	}
}
