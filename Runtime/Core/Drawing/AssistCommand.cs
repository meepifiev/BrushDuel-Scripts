namespace _Project.Core.Drawing
{
    public class AssistCommand : DrawingCommand
    {
        public AssistCommand(int sequence, int pixelBudget)
            : base(sequence)
        {
            PixelBudget = pixelBudget;
        }

        public int PixelBudget { get; }
    }
}
