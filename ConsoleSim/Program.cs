using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Pacman.Simulator;
using System.Diagnostics;

namespace PacmanAI
{
	class Program
	{
		private static int gamesPlayed = 0;
		private static int totalScore = 0;
		private static GameState gs;
		private static int gamesToPlay = 100;
		private static long longestGame = 0;
		private static int highestScore = 0;
		private static int lowestScore = int.MaxValue;
		private static long lastMs = 0;
		private static long ms = 0;
		private static MemoryStream bestGame = new MemoryStream(); // i'm an idiot ... this should be in a BinaryWriter ...
		private static MemoryStream worstGame = new MemoryStream();
		private static MemoryStream currentGame = new MemoryStream(); // it's also buggy sometimes (nodes showing off by one)
		private static List<int> scores = new List<int>();

		static void Main(string[] args) {

			int cores = System.Environment.ProcessorCount;
			int gamesForEach = gamesToPlay / cores;
			for( int i = 0; i < cores; i++ ) {
				// add multicore support				
			}

			gs = new GameState();
			gs.GameOver += new EventHandler(GameOverHandler);
			gs.StartPlay();

			// DEFINE CONTROLLER //
			//BasePacman controller = new TestPac();
			BasePacman controller = new SmartPac();
			//BasePacman controller = new SmartDijkstraPac();
			gs.Controller = controller;

			Stopwatch watch = new Stopwatch();
			int percentage = -1;
			int lastUpdate = 0;
			watch.Start();
			while( gamesPlayed < gamesToPlay ) {
				int newPercentage = (int)Math.Floor(((float)gamesPlayed / gamesToPlay) * 100);
				if( newPercentage != percentage || gamesPlayed - lastUpdate >= 100 ) {
					lastUpdate = gamesPlayed;
					percentage = newPercentage;
					Console.Clear();
					Console.WriteLine("Simulating ... " + percentage + "% (" + gamesPlayed + " : " + gamesToPlay + ")");
					Console.WriteLine(" - Elapsed: " + formatSeconds((watch.ElapsedMilliseconds / 1000.0) + "") + "s, Estimated total: " + formatSeconds(((watch.ElapsedMilliseconds / 1000.0) / percentage * 100) + "") + "s");
					Console.WriteLine(" - Current best: " + highestScore);
					Console.WriteLine(" - Current worst: " + lowestScore);
					if( gamesPlayed > 0 ) {
						Console.WriteLine(" - Current avg.: " + (totalScore / gamesPlayed));
					}
					for( int i = scores.Count - 1; i >= 0 && i > scores.Count - 100; i-- ) {
						Console.Write(scores[i] + ",");
					}
				}
				// update gamestate
				Direction direction = controller.Think(gs);
				gs.Pacman.SetDirection(direction);
				// update stream
				currentGame.WriteByte((byte)Math.Floor(gs.Pacman.Xf));
				currentGame.WriteByte((byte)Math.Floor(gs.Pacman.Yf));
				currentGame.WriteByte((byte)gs.Pacman.Direction);
				currentGame.WriteByte((byte)gs.Pacman.Lives);
				currentGame.WriteByte((byte)(gs.Pacman.Score / 255));
				currentGame.WriteByte((byte)(gs.Pacman.Score % 255));
				foreach( Pacman.Simulator.Ghosts.Ghost g in gs.Ghosts ) {
					currentGame.WriteByte((byte)g.X);
					currentGame.WriteByte((byte)g.Y);
					currentGame.WriteByte((byte)((g.Chasing == true) ? 1 : 0));
					currentGame.WriteByte((byte)((g.Entered == true) ? 1 : 0));
					currentGame.WriteByte((byte)g.Direction);
					currentGame.WriteByte((byte)((g.IsEaten == true) ? 1 : 0));
				}
				// update game
				gs.Update();
				ms += GameState.MSPF;
			}
			watch.Stop();

			// shut down controller
			controller.SimulationFinished();

			// write best/worst to disk
			using( BinaryWriter bw = new BinaryWriter(new FileStream(System.Environment.CurrentDirectory + "/best" + highestScore + ".dat", FileMode.Create)) ) {
				bestGame.WriteTo(bw.BaseStream);
			}
			using( BinaryWriter bw = new BinaryWriter(new FileStream(System.Environment.CurrentDirectory + "/worst" + lowestScore + ".dat", FileMode.Create)) ) {
				worstGame.WriteTo(bw.BaseStream);
			}

			// write results
			using( StreamWriter sw = new StreamWriter(File.Open("scores.txt", FileMode.Create)) ) {
				foreach( int s in scores ) {
					sw.Write(s + "\n");
				}
			}

			// output results
			Console.Clear();
			long seconds = ms / 1000;
			Console.WriteLine("Games played: " + gamesPlayed);
			Console.WriteLine("Avg. score: " + (totalScore / gamesPlayed));
			Console.WriteLine("Highest score: " + highestScore + " points");
			Console.WriteLine("Lowest score: " + lowestScore + " points");
			Console.WriteLine("Longest game: " + ((float)longestGame / 1000.0f) + " seconds");
			Console.WriteLine("Total simulated time: " + (seconds / 60 / 60 / 24) + "d " + ((seconds / 60 / 60) % 24) + "h " + ((seconds / 60) % 60) + "m " + (seconds % 60) + "s");
			Console.WriteLine("Avg. simulated time pr. game: " + ((float)ms / 1000.0f / gamesPlayed) + " seconds");
			Console.WriteLine("Simulation took: " + (watch.ElapsedMilliseconds / 1000.0f) + " seconds");
			Console.WriteLine("Speed: " + (ms / watch.ElapsedMilliseconds) + " (" + ((ms / watch.ElapsedMilliseconds) / 60) + "m " + ((ms / watch.ElapsedMilliseconds) % 60) + " s) simulated seconds pr. second");
			Console.WriteLine("For a total of: " + gamesPlayed / (watch.ElapsedMilliseconds / 1000.0f) + " games pr. second");
			Console.ReadLine();
		}

		private static void GameOverHandler(object sender, EventArgs args) {
			if( ms - lastMs > longestGame )
				longestGame = ms - lastMs;
			if( gs.Pacman.Score > highestScore ) {
				highestScore = gs.Pacman.Score;
				bestGame = currentGame;
			}
			if( gs.Pacman.Score < lowestScore ) {
				lowestScore = gs.Pacman.Score;
				worstGame = currentGame;
			}
			scores.Add(gs.Pacman.Score);
			currentGame = new MemoryStream();
			totalScore += gs.Pacman.Score;
			gamesPlayed++;
			lastMs = ms;
		}

		private static string formatSeconds(string s) {
			try {
				return s.Substring(0, s.IndexOf(","));
			} catch {
				return s;
			}
		}
	}
}
