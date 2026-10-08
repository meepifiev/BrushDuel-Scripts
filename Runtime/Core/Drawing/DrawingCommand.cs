namespace _Project.Core.Drawing
{
    public abstract class DrawingCommand
    {
        protected DrawingCommand(int sequence)
        {
            Sequence = sequence;
        }

        public int Sequence { get; }
    }
}
