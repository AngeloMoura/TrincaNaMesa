namespace Domino
{
    [System.Serializable]
    public class DominoTile
    {
        public int A;
        public int B;

        public DominoTile(int a, int b)
        {
            A = a;
            B = b;
        }

        public DominoTile Flipped()
        {
            return new DominoTile(B, A);
        }

        public override string ToString()
        {
            return $"[{A}|{B}]";
        }
    }
}
