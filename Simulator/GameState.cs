using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Pacman.Simulator.Ghosts;

namespace Pacman.Simulator
{
	public enum Maze { Red, LightBlue, Brown, DarkBlue, None };

	public class GameState
	{
		private int level = 0;
		public int Level { get { return level; } }

		public static readonly Random Random = new Random();

		public readonly Pacman Pacman;		
		public readonly Red Red;
		public readonly Pink Pink;
		public readonly Blue Blue;
		public readonly Brown Brown;
		public readonly Ghost[] Ghosts = new Ghost[4];

		public BasePacman Controller;

		public const int MSPF = 40;
		public long Timer = 0;
		public long ElapsedTime = 0;
		public long Frames = 0;

		public Image[] Mazes = new Image[4];

		private Map[] maps = new Map[4];
		private Map map;
		public Map Map { get { return map; } }

		private bool started = false;
		public bool Started { get { return started; } }

		private const int reversalTime1 = 5000, reversalTime2 = 25000; // estimates
		private int reversal1 = reversalTime1, reversal2 = reversalTime2;		

		// settings
		public bool PacmanMortal = true;
		public bool NaturalReversals = true;
		public bool Replay = false;
		public bool AutomaticLevelChange = true;

		public event EventHandler GameOver;
		public event EventHandler PacmanDead = new EventHandler(delegate(object sender, EventArgs e){ });

		public GameState() {
			loadMazes();
			map = maps[Level];
			// default position ... find out where
			Pacman = new Pacman(Pacman.StartX, Pacman.StartY, this);
			Ghosts[0] = Red = new Red(Red.StartX, Red.StartY, this);
			Ghosts[1] = Pink = new Pink(Pink.StartX, Pink.StartY, this);
			Ghosts[2] = Blue = new Blue(Blue.StartX, Blue.StartY, this);
			Ghosts[3] = Brown = new Brown(Brown.StartX, Brown.StartY, this);			
		}

		public GameState(Pacman pacman, Red red, Pink pink, Blue blue, Brown brown) {
			loadMazes();
			map = maps[Level];
			this.Pacman = pacman;
			Ghosts[0] = this.Red = red;
			Ghosts[1] = this.Pink = pink;
			Ghosts[2] = this.Blue = blue;
			Ghosts[3] = this.Brown = brown;			
		}

		private void loadMazes(){
			for( int i = 0; i < 4; i++ ) {
				Mazes[i] = Util.LoadImage("ms_pacman_maze" + (i + 1) + ".gif");
				maps[i] = new Map((Bitmap)Mazes[i],(Maze)i);
			}
		}

		public void StartPlay() {
			started = true;
			Timer = 0;
			Frames = 0;
		}

		public void PausePlay() {
			started = false;
		}

		public void ResumePlay() {
			started = true;
		}

		public void ReverseGhosts() {
			foreach( Ghost g in Ghosts ) {
				g.Reversal();
			}
		}
		
		public void LoadMaze(Maze maze) {
			map = maps[(int)maze];
			map.Reset();
			Pacman.ResetPosition();
			foreach( Ghost g in Ghosts ) {
				g.ResetPosition();
			}
		}

		public void Update() {
			if( !started ) {
				return;
			}
			Frames++;
			Timer += MSPF;
			ElapsedTime += MSPF;
			// change level
			// TODO: use levels instead of just mazes
			if( Map.PillsLeft == 0 && AutomaticLevelChange ) {
				//level = Level + 1; // test for screenplayer
				if( Level > Mazes.Length - 1 ) level = 0;
				map = maps[Level];
				map.Reset();
				resetTimes();
				Pacman.ResetPosition();
				foreach( Ghost g in Ghosts ) {
					g.ResetPosition();
				}
				if( Controller != null ) {
					Controller.LevelCleared();
				}
				return;
			}
			// ghost reversals
			if( NaturalReversals ) {
				bool ghostFleeing = false;
				foreach( Ghost g in Ghosts ) {
					if( !g.Chasing ) {
						ghostFleeing = true;
						break;
					}
				}
				if( ghostFleeing ) {
					reversal1 += MSPF;
					reversal2 += MSPF;
				} else {
					if( Timer > reversal1 ) {
						reversal1 += 1200000; // 20 min
						ReverseGhosts();
					}
					if( Timer > reversal2 ) {
						reversal2 = Int32.MaxValue;
						ReverseGhosts();
					}
				}
			}
			if( Replay ) {
				// do nothing
			}
			else {
				// move
				Pacman.Move();
				foreach( Ghost g in Ghosts ) {
					g.Move();
					// check collisions				
					if( g.Distance(Pacman) < 4.0f ) {
						if( g.Chasing ) {
							if( PacmanMortal ) {
								resetTimes();
								Pacman.Die();								
								PacmanDead(this, null);
								foreach( Ghost g2 in Ghosts ) {
									g2.PacmanDead();
								}
								if( Pacman.Lives == -1 ) {
									InvokeGameOver();
								}
								break;
							}
						} else if( g.Entered ) {
							Pacman.EatGhost();
							g.Eaten();
						}
					}
				}
			}
		}

		public void InvokeGameOver() {
			GameOver(this, null);
			ElapsedTime = 0;
			level = 0;
			map = maps[Level];
			map.Reset();
			Pacman.Reset();			
			foreach( Ghost g in Ghosts ) {
				g.PacmanDead();
			}			
		}

		private void resetTimes() {
			Timer = 0;
			reversal1 = reversalTime1; 
			reversal2 = reversalTime2;
		}

		public GameState Clone() {
			throw new NotImplementedException();			
		}		
	}
}
