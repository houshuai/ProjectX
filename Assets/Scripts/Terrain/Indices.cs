public class Indices
{
    private int xCount;
    private int xGap, yGap;

    public Indices(int xCount, int yCount)
    {
        this.xCount = xCount;
        xGap = xCount - 1;
        yGap = yCount - 1;
    }

    public int[][] InitialIndices(int lod)
    {
        int scale = 1 << lod;
        int currXGap = xGap / scale;
        int currYGap = yGap / scale;
        if (currXGap < 4 || currYGap < 4)
        {
            return null;
        }
        int totalIndexCount = currXGap * currYGap * 6;
        int xdecrease = currXGap / 2 * 3;
        int ydecrease = currYGap / 2 * 3;
        var original = new int[totalIndexCount];
        SetCenterIndices(original, 0, 0, xGap, 0, yGap, 1 * scale);

        int index = 0;
        var leftTop = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(leftTop, index, scale, xGap, 0, yGap - scale, scale);
        index = SetLeftIndices(leftTop, index, 0, yGap - 2 * scale, scale);
        index = SetTopIndices(leftTop, index, 2 * scale, xGap, scale);
        index = SetLeftTopIndices(leftTop, index, scale);

        index = 0;
        var top = new int[totalIndexCount - xdecrease];
        index = SetCenterIndices(top, index, 0, xGap, 0, yGap - scale, scale);
        index = SetTopIndices(top, index, 0, xGap, scale);

        index = 0;
        var rightTop = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(rightTop, index, 0, xGap - scale, 0, yGap - scale, scale);
        index = SetTopIndices(rightTop, index, 0, xGap - 2 * scale, scale);
        index = SetRightTopIndices(rightTop, index, scale);
        index = SetRightIndices(rightTop, index, 0, yGap - 2 * scale, scale);

        index = 0;
        var left = new int[totalIndexCount - ydecrease];
        index = SetCenterIndices(left, index, scale, xGap, 0, yGap, scale);
        index = SetLeftIndices(left, index, 0, yGap, scale);

        index = 0;
        var center = new int[totalIndexCount - (currXGap + currYGap) * 3];
        index = SetCenterIndices(center, index, scale, xGap - scale, scale, yGap - scale, scale);
        index = SetLeftIndices(center, index, 2 * scale, yGap - 2 * scale, scale);
        index = SetTopIndices(center, index, 2 * scale, xGap - 2 * scale, scale);
        index = SetRightIndices(center, index, 2 * scale, yGap - 2 * scale, scale);
        index = SetBottomIndices(center, index, 2 * scale, xGap - 2 * scale, scale);
        index = SetLeftTopIndices(center, index, scale);
        index = SetRightTopIndices(center, index, scale);
        index = SetLeftBottomIndices(center, index, scale);
        index = SetRightBottomIndices(center, index, scale);

        index = 0;
        var right = new int[totalIndexCount - ydecrease];
        index = SetCenterIndices(right, index, 0, xGap - scale, 0, yGap, scale);
        index = SetRightIndices(right, index, 0, yGap, scale);

        index = 0;
        var leftBottom = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(leftBottom, index, scale, xGap, scale, yGap, scale);
        index = SetLeftIndices(leftBottom, index, 2 * scale, yGap, scale);
        index = SetLeftBottomIndices(leftBottom, index, scale);
        index = SetBottomIndices(leftBottom, index, 2 * scale, xGap, scale);

        index = 0;
        var bottom = new int[totalIndexCount - xdecrease];
        index = SetCenterIndices(bottom, index, 0, xGap, scale, yGap, scale);
        index = SetBottomIndices(bottom, index, 0, xGap, scale);

        index = 0;
        var rightBottom = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(rightBottom, index, 0, xGap - scale, scale, yGap, scale);
        index = SetBottomIndices(rightBottom, index, 0, xGap - 2 * scale, scale);
        index = SetRightBottomIndices(rightBottom, index, scale);
        index = SetRightIndices(rightBottom, index, 2 * scale, yGap, scale);

        var allIndices = new int[(int)MeshType.Count][];
        allIndices[(int)MeshType.Original] = original;
        allIndices[(int)MeshType.LeftTop] = leftTop;
        allIndices[(int)MeshType.Top] = top;
        allIndices[(int)MeshType.RightTop] = rightTop;
        allIndices[(int)MeshType.Left] = left;
        allIndices[(int)MeshType.Center] = center;
        allIndices[(int)MeshType.Right] = right;
        allIndices[(int)MeshType.LeftBottom] = leftBottom;
        allIndices[(int)MeshType.Bottom] = bottom;
        allIndices[(int)MeshType.RightBottom] = rightBottom;

        return allIndices;
    }

    private int SetCenterIndices(int[] indices, int startIndex, int xStart, int xEnd, int yStart, int yEnd, int offset)
    {
        for (int i = xStart; i < xEnd; i += offset)
        {
            for (int j = yStart; j < yEnd; j += offset)
            {
                int self = i + (j * xCount);
                int next = i + ((j + offset) * xCount);
                indices[startIndex++] = self;
                indices[startIndex++] = next;
                indices[startIndex++] = self + offset;
                indices[startIndex++] = self + offset;
                indices[startIndex++] = next;
                indices[startIndex++] = next + offset;
            }
        }
        return startIndex;
    }

    private int SetLeftIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int j = start; j < end; j += offset * 2)
        {
            int self = 0 + j * xCount;
            int next = 0 + (j + offset) * xCount;
            int nextNext = 0 + (j + 2 * offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = self;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = nextNext + offset;
        }
        return startIndex;
    }

    private int SetTopIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int i = start; i < end; i += offset * 2)
        {
            int self = i + (yGap - offset) * xCount;
            int next = i + (yGap - offset + offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = next + 2 * offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next + 2 * offset;
            indices[startIndex++] = self + 2 * offset;
        }
        return startIndex;
    }

    private int SetRightIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int j = start; j < end; j += offset * 2)
        {
            int self = xGap - offset + j * xCount;
            int next = xGap - offset + (j + offset) * xCount;
            int nextNext = xGap - offset + (j + 2 * offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = nextNext + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = nextNext + offset;
        }
        return startIndex;
    }

    private int SetBottomIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int i = start; i < end; i += offset * 2)
        {
            int self = i + 0 * xCount;
            int next = i + (0 + offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self + 2 * offset;
            indices[startIndex++] = self + 2 * offset;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = next + 2 * offset;
        }
        return startIndex;
    }

    private int SetLeftTopIndices(int[] indices, int startIndex, int offset)
    {
        int self = 0 + (yGap - 2 * offset) * xCount;
        int next = 0 + (yGap - offset) * xCount;
        int nextNext = 0 + (yGap) * xCount;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + 2 * offset;

        return startIndex;
    }

    private int SetRightTopIndices(int[] indices, int startIndex, int offset)
    {
        int self = xGap - 2 * offset + (yGap - 2 * offset) * xCount;
        int next = xGap - 2 * offset + (yGap - offset) * xCount;
        int nextNext = xGap - 2 * offset + (yGap) * xCount;
        indices[startIndex++] = next;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        return startIndex;
    }

    private int SetLeftBottomIndices(int[] indices, int startIndex, int offset)
    {
        int self = 0 + 0 * xCount;
        int next = 0 + offset * xCount;
        int nextNext = 0 + 2 * offset * xCount;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + 2 * offset;
        return startIndex;
    }

    private int SetRightBottomIndices(int[] indices, int startIndex, int offset)
    {
        int self = xGap - 2 * offset + 0 * xCount;
        int next = xGap - 2 * offset + offset * xCount;
        int nextNext = xGap - 2 * offset + 2 * offset * xCount;
        indices[startIndex++] = self;
        indices[startIndex++] = next;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        return startIndex;
    }
}
