[System.Serializable]
public class DominoTile
{
    public int SideA;
    public int SideB;

    public DominoTile(int a, int b)
    {
        SideA = a;
        SideB = b;
    }

    public bool Matches(int value)
    {
        return SideA == value || SideB == value;
    }

    public DominoTile Flipped()
    {
        return new DominoTile(SideB, SideA);
    }

    public override string ToString()
    {
        return $"{SideA}|{SideB}";
    }
}
