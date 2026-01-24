using System.Diagnostics;

namespace Nebula.Commons.Text
{
    [DebuggerStepThrough]
    public struct DocumentPosition
    {
        public DocumentPosition(int line, int character)
        {
            Line = line;
            Character = character;
        }

        public int Line { get; set; }
        public int Character { get; set; }
    }
}
