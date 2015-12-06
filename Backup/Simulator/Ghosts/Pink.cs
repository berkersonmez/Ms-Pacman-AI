using System;
using System.Collections.Generic;
using System.Text;

namespace Pacman.Simulator.Ghosts
{
	public class Pink : Ghost
	{
		public const int StartX = 111, StartY = 118;
		private const int firstWaitToEnter = -2, secondWaitToEnter = 9;

		public Pink(int x, int y, GameState gameState)
			: base(x, y, gameState) {
			this.name = "Pink";
			this.drawOffset = Height;
			ResetPosition();
		}

		public override void PacmanDead() {			
			ResetPosition();
			waitToEnter = secondWaitToEnter;
		}

		public override void ResetPosition() {
			x = StartX;
			y = StartY;
			waitToEnter = firstWaitToEnter;
			direction = Direction.Down;
			base.ResetPosition();			
		}

		public override void  Move()
		{
			if( Distance(gameState.Pacman) > randomMoveDist && GameState.Random.Next(0, randomMove) == 0 ) {
				MoveRandom();
			} else {
				// testing pinky (stupid and mostly a bother?)
				if( Distance(gameState.Pacman) > 120 || gameState.Pacman.Direction == Direction.None ) {
					// should probably do something else for none! (read gamefaq, but good enough for now)
					MoveAsRed();
				} else {
					// this is pretty stupid ... basicly we just always try to get in front
					switch( gameState.Pacman.Direction ) {
						case Direction.Up:
							if( IsAbove(gameState.Pacman) ) {
								TryGo(Direction.Down);
								if( IsLeft(gameState.Pacman) ) {
									TryGo(Direction.Right);
									TryGo(Direction.Left);
								} else {
									TryGo(Direction.Left);
									TryGo(Direction.Right);
								}
								TryGo(Direction.Up);
							} else {
								TryGo(Direction.Up);
								if( IsLeft(gameState.Pacman) ) {
									TryGo(Direction.Right);
									TryGo(Direction.Left);
								} else {
									TryGo(Direction.Left);
									TryGo(Direction.Right);
								}
								TryGo(Direction.Down);
							}
							break;
						case Direction.Down:
							if( IsBelow(gameState.Pacman) ) {
								TryGo(Direction.Up);
								if( IsLeft(gameState.Pacman) ) {
									TryGo(Direction.Right);
									TryGo(Direction.Left);
								} else {
									TryGo(Direction.Left);
									TryGo(Direction.Right);
								}
								TryGo(Direction.Down);
							} else {
								TryGo(Direction.Down);
								if( IsLeft(gameState.Pacman) ) {
									TryGo(Direction.Right);
									TryGo(Direction.Left);
								} else {
									TryGo(Direction.Left);
									TryGo(Direction.Right);
								}
								TryGo(Direction.Up);
							}
							break;
						case Direction.Left:
							if( IsLeft(gameState.Pacman) ) {
								TryGo(Direction.Right);
								if( IsBelow(gameState.Pacman) ) {
									TryGo(Direction.Up);
									TryGo(Direction.Down);
								} else {
									TryGo(Direction.Down);
									TryGo(Direction.Up);
								}
								TryGo(Direction.Left);
							} else {
								TryGo(Direction.Left);
								if( IsBelow(gameState.Pacman) ) {
									TryGo(Direction.Up);
									TryGo(Direction.Down);
								} else {
									TryGo(Direction.Down);
									TryGo(Direction.Up);
								}
								TryGo(Direction.Right);
							}
							break;
						case Direction.Right:
							if( IsRight(gameState.Pacman) ) {
								TryGo(Direction.Left);
								if( IsBelow(gameState.Pacman) ) {
									TryGo(Direction.Up);
									TryGo(Direction.Down);
								} else {
									TryGo(Direction.Down);
									TryGo(Direction.Up);
								}
								TryGo(Direction.Right);
							} else {
								TryGo(Direction.Right);
								if( IsBelow(gameState.Pacman) ) {
									TryGo(Direction.Up);
									TryGo(Direction.Down);
								} else {
									TryGo(Direction.Down);
									TryGo(Direction.Up);
								}
								TryGo(Direction.Left);
							}
							break;
					}
				}
			}
			base.Move();
		}
	}
}
