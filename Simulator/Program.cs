using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Pacman.Simulator
{
	class Program
	{
		private static Visualizer visualizer;
		private static Thread visualizerThread;

		static void Main(string[] args) {
			Console.WriteLine("Simulator started");
			//Console.WriteLine("Press Enter to exit");

			startVisualizer();
						
			while( true ) {
				string input = Console.ReadLine();
				switch(input){
					case "":
						//visualizerThread.Abort(); // buggy ... catch and close down gracefully
						//System.Threading.Thread.CurrentThread.Abort();
						break;
					case "restart":
					case "r":
						// support this
						break;
				}

			}
		}

		private static void startVisualizer() {
			visualizerThread = new System.Threading.Thread(delegate() {
				visualizer = new Visualizer();
				System.Windows.Forms.Application.Run(visualizer);
			});
			visualizerThread.Start();
		}

		private static void trace(params object[] list) {
			foreach( object o in list ) {
				try {
					Console.WriteLine(o.ToString() + ", ");
				} catch( Exception e ) {
					Console.WriteLine(e.Message + ", ");
				}				
			}
		}
	}
}
