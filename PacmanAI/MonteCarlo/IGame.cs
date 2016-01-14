using System;
using System.Collections.Generic;

namespace PacmanAI.MonteCarlo
{
    public abstract class IGame
    {
        public enum State
        {
            Unknown,
            PlayerOne,
            PlayerTwo,
            Draw
        };

        public abstract class IMove
        {

        }

        public State CurrentPlayer { get; protected set; }
        public State CurrentOpponent { get; protected set; }

        public abstract State PollState();

        public abstract String GetPlayerTag(State player);

        public abstract IGame Clone();

        public abstract void Move(IMove mode);
        public abstract bool IsMoveValid(IMove move);
        public abstract List<IMove> GetValidMoves();
        public abstract IMove GetMoveForCoordinate(float x, float y);
    }
}