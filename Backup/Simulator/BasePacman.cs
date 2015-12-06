using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;

namespace Pacman.Simulator
{
	public abstract class BasePacman
	{
		public readonly string Name;

		public BasePacman(string name) {
			this.Name = name;
		}

		public abstract Direction Think(GameState gs);

		public virtual void EatPill() { }
		public virtual void EatPowerPill() { }
		public virtual void EatGhost() { }
		public virtual void EatenByGhost() { }
		public virtual void LevelCleared() { }

		public virtual void Draw(Graphics g) { }
		public virtual void Draw(Graphics g, int[] danger) { }
		public virtual void WriteData(StreamWriter sw, int sector) { }

		public virtual void SimulationFinished() { }
	}
}
