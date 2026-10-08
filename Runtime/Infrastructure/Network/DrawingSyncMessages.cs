using Mirror;

namespace _Project.Infrastructure.Network
{
    public struct StrokeBeganMessage : NetworkMessage
    {
        public byte Brush;
        public byte SizeIndex;
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
        public ushort[] Points;
    }

    public struct StrokeContinuedMessage : NetworkMessage
    {
        public ushort[] Points;
    }

    public struct StrokeEndedMessage : NetworkMessage
    {
    }

    public struct FillAppliedMessage : NetworkMessage
    {
        public ushort X;
        public ushort Y;
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
    }

    public struct UndoAppliedMessage : NetworkMessage
    {
    }
}
