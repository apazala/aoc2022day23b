class Position
{
    public int i;
    public int j;
    public Position(int i, int j)
    {
        Set(i, j);
    }

    public override bool Equals(object? obj)
    {
        return obj is Position elf &&
               i == elf.i &&
               j == elf.j;
    }

    public void Add(Position operand, Position result)
    {
        result.i = i + operand.i;
        result.j = j + operand.j;
    }

    public void Set(int i, int j)
    {
        this.i = i;
        this.j = j;
    }

    public override int GetHashCode()
    {
        return i * 31 + j;
    }
}



class Direction
{
    public static int NW = 0;
    public static int N = 1;
    public static int NE = 2;
    public static int E = 3;
    public static int SE = 4;
    public static int S = 5;
    public static int SW = 6;
    public static int W = 7;

    public static int NDIRECTIONS = 8;

    public static Position[] neighborMove = new Position[NDIRECTIONS];

    static Direction(){
        neighborMove[NW] = new Position(-1, -1);
        neighborMove[N] = new Position(-1, 0);
        neighborMove[NE] = new Position(-1, 1);
        neighborMove[E] = new Position(0, 1);
        neighborMove[SE] = new Position(1, 1);
        neighborMove[S] = new Position(1, 0);
        neighborMove[SW] = new Position(1, -1);
        neighborMove[W] = new Position(0, -1);
    }

    public Position move;
    public Direction next;
    public int[] checkPositions = new int[3];
}

class IntCounter{
    int count;
    public IntCounter()
    {
        count = 1;
    }

    public void Inc(){
        count++;
    }

    public int Value(){
        return count;
    }
}

class Elf
{
    public Position currentPosition;
    public Position nextPosition;

    public bool dontMove = true;

    private bool[] availableMovements = new bool[Direction.NDIRECTIONS];

    public Elf(int i, int j)
    {
        currentPosition = new Position(i, j);
        nextPosition = new Position(i, j);
    }

    public override bool Equals(object? obj)
    {
        return obj is Elf elf && currentPosition.Equals(elf.currentPosition);
    }

    public override int GetHashCode()
    {
        return currentPosition.GetHashCode();
    }

    public Position? nextPositionSet(Direction currentDirection, HashSet<Position> elvesPositionSet)
    {
        this.dontMove = true;
        for(int k = 0; k < Direction.NDIRECTIONS; k++)
        {
            currentPosition.Add(Direction.neighborMove[k], nextPosition);
            availableMovements[k] = !elvesPositionSet.Contains(nextPosition);
            dontMove &= availableMovements[k];
        }

        if(!dontMove)
        {
            bool validDir = false;
            for(int c = 0; c < 4 && !validDir; c++){
                validDir = true;
                for(int r = 0; validDir && r < currentDirection.checkPositions.Length; r++)
                {
                    validDir = availableMovements[currentDirection.checkPositions[r]];
                }

                if(validDir)
                {
                    currentPosition.Add(currentDirection.move, nextPosition);
                } else {
                    currentDirection = currentDirection.next;
                }
            }

            if(!validDir)
                dontMove = true;
            
        }

        if(dontMove){
            nextPosition.i = currentPosition.i;
            nextPosition.j = currentPosition.j;
            return null;
        }

        return nextPosition;
    }

    public void Move(Dictionary<Position, IntCounter> elvesNextPositionDict)
    {
        if(dontMove) return;
        if(elvesNextPositionDict[this.nextPosition].Value() != 1) return;
        currentPosition.i = nextPosition.i;
        currentPosition.j = nextPosition.j; 
    }
}

internal class Program
{
    private static void storeInitialElfPositions(List<Elf> elfList)
    {
        string[] lines = File.ReadAllLines(@"input.txt");
        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {
                if (lines[i][j] == '#')
                {
                    elfList.Add(new Elf(i, j));
                }
            }
        }
    }

    private static Direction InitDirections()
    {
        Direction north = new Direction();
        north.move = Direction.neighborMove[Direction.N];
        north.checkPositions[0] = Direction.NW;
        north.checkPositions[1] = Direction.N;
        north.checkPositions[2] = Direction.NE;


        Direction east = new Direction();
        east.move = Direction.neighborMove[Direction.E];
        east.checkPositions[0] = Direction.NE;
        east.checkPositions[1] = Direction.E;
        east.checkPositions[2] = Direction.SE;

        
        Direction south = new Direction();
        south.move = Direction.neighborMove[Direction.S];
        south.checkPositions[0] = Direction.SE;
        south.checkPositions[1] = Direction.S;
        south.checkPositions[2] = Direction.SW;

        
        Direction west = new Direction();
        west.move = Direction.neighborMove[Direction.W];
        west.checkPositions[0] = Direction.SW;
        west.checkPositions[1] = Direction.W;
        west.checkPositions[2] = Direction.NW;


        north.next = south;
        south.next = west;
        west.next = east;
        east.next = north;

        return north;
    } 
    private static void Main(string[] args)
    {
        List<Elf> elfList = new List<Elf>();
        storeInitialElfPositions(elfList);
        Direction directionLinked = InitDirections();

        int count;
        bool someMove = true; 
        for(count = 0; someMove; count++)
        {        
            HashSet<Position> elvesPositionsSet = new HashSet<Position>();
            Dictionary<Position, IntCounter> elvesNextPositionDict = new Dictionary<Position, IntCounter>();

            foreach(Elf elf in elfList)
                elvesPositionsSet.Add(elf.currentPosition);
            
            //printState(elvesPositionsSet);

            someMove = false;
            foreach(Elf elf in elfList)
            {
                Position? nextPosition = elf.nextPositionSet(directionLinked, elvesPositionsSet);
                if(nextPosition!= null)
                {
                    someMove = true;
                    IntCounter intCounter;
                    if(elvesNextPositionDict.TryGetValue(nextPosition, out intCounter)){
                        intCounter.Inc();
                    }else{
                        elvesNextPositionDict.Add(nextPosition, new IntCounter());
                    }
                }
            }

            foreach(Elf elf in elfList)
                elf.Move(elvesNextPositionDict);
            
            directionLinked = directionLinked.next;
        }

        Console.WriteLine(count);
    }


}