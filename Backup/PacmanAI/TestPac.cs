using System;
using System.Collections.Generic;
using System.Text;

using Pacman.Simulator;
using Pacman.Simulator.Ghosts;

namespace PacmanAI
{
	public class TestPac : BasePacman
	{
		public TestPac() : base("TestPac") { }

		public override Direction Think(GameState gs) {
			return Direction.Left;
		}
	}
}
