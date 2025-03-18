[System.Serializable]
public class Array2D<T>
{
    [System.Serializable]
    public class Row
    {
        public T[] row;
    }

    public Row[] Array;

    public Array2D(int rows, int cols)
    {
        Array = new Row[rows];
        for (int i = 0; i < rows; i++)
        {
            Array[i] = new Row();
            Array[i].row = new T[cols];
        }
    }

    // add an override for [][]
    public T this[int row, int col]
    {
        get { return Array[row].row[col]; }
        set { Array[row].row[col] = value; }
    }

    public int GetLength(int dimension)
    {
        if (dimension == 0)
            return Array.Length; // number of rows
        else if (dimension == 1)
            return Array[0].row.Length; // number of columns
        else
            throw new System.IndexOutOfRangeException("Invalid dimension");
    }
}