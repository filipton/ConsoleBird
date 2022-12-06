using System.Diagnostics;

namespace ConsoleBird;

public static class ConsoleBird
{
    private const int TargetFps = 30;
    
    // Map
    const int BoardSize = 20;
    const int HoleSize = 7;
    const int ObstacleDistance = 16;
    const int ObstacleInitialX = 25;
        
    static readonly Queue<Obstacle> Obstacles = new ();
    
    
    // Player
    const float JumpHeight = 3f;
    const float Gravity = -10f;
    const float MaxJumpVelocity = 12f;
    const int PlayerX = 5;

    public static float PlayerY = BoardSize * 2/3;
    public static float PlayerVelocity;

    
    // Stats
    public static int Score = 0;
    

    static readonly Stopwatch GameLoopSw = new ();
    static bool _gameOver = false;
    
    public static void Main(string[] args)
    {
        GenerateObstacles(10);
        SetupWindow();
        
        long lastMs = 0;
        GameLoopSw.Start();


        int lastWw = Console.WindowWidth;
        int lastWh = Console.WindowHeight;
        
        while (GameLoopSw.IsRunning && !_gameOver)
        {
            while(GameLoopSw.ElapsedMilliseconds - lastMs < 1000 / TargetFps){}
            
            
            if(lastWh != Console.WindowHeight || lastWw != Console.WindowWidth)
            {
                CheckWindowSize();

                lastWh = Console.WindowHeight;
                lastWw = Console.WindowWidth;
                SetupWindow();
            }
            
            // clear buffer
            _buffer = new char[Console.WindowWidth, BoardSize];
            
            GameLoop(GameLoopSw.ElapsedMilliseconds - lastMs);
            RenderBuffer();
            
            lastMs = GameLoopSw.ElapsedMilliseconds;
        }
        
        Console.Clear();
        WriteCentered("Game Over!");
        WriteCentered($"Score: {Score}", 1);
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
    }

    static void SetupWindow()
    {
        Console.Clear();
        _buffer = new char[Console.WindowWidth,BoardSize];
        
        // clear buffer could contain a static ui elements (borders etc.)
        // _clearBuffer = new char[Console.WindowWidth,BoardSize];
    }
    
    
    static char[,] _buffer = new char[0,0];
    static void GameLoop(float frameTime)
    {
        PlayerLogic(frameTime);
        ObstaclesLogic(frameTime);
    }

    static void PlayerLogic(float frameTime)
    {
        PlayerVelocity += Gravity * frameTime / 1000;
        if (Console.KeyAvailable && 
            Console.ReadKey(false).Key == ConsoleKey.Spacebar)
        {
            PlayerVelocity += (float) Math.Sqrt(2 * -Gravity * JumpHeight);
        }

        PlayerVelocity = PlayerVelocity > MaxJumpVelocity ? MaxJumpVelocity : PlayerVelocity;
        PlayerY += PlayerVelocity * frameTime / 1000;

        if (PlayerY is < 0 or > BoardSize)
        {
            _gameOver = PlayerY < 0;
            return;
        }
        
        _buffer[PlayerX, (int)PlayerY] = '●';
    }


    static float _obstaclesXOffset = 0;
    static void ObstaclesLogic(float frameTime)
    {
        _obstaclesXOffset += frameTime / 250;

        foreach (Obstacle o in Obstacles.ToArray())
        {
            int localX = ObstacleInitialX + o.X - (int)_obstaclesXOffset;
            if(localX >= Console.WindowWidth || localX < 0) continue;

            for (int i = 0; i < BoardSize; i++)
            {
                if(i > o.Hole && i < o.Hole + HoleSize) continue;
                _buffer[localX, i] = '▊';   
            }

            // check collision
            if (localX == PlayerX)
            {
                if (PlayerY < o.Hole || PlayerY > o.Hole + HoleSize)
                {
                    _gameOver = true;
                    return;
                }

                Obstacles.Dequeue();
                GenerateObstacles(1);
                Score++;
            }
        }
    }
        
    private static string _tmpStrBuff = "";
    static void RenderBuffer()
    {
        _tmpStrBuff = "";
        for (int y = BoardSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < _buffer.GetLength(0); x++)
            {
                _tmpStrBuff += _buffer[x, y] == '\0' ? ' ' : _buffer[x, y];
            }

            _tmpStrBuff += "\n";
        }

        _tmpStrBuff += new string('▇', Console.WindowWidth);
        
        Console.SetCursorPosition(0, 0);
        Console.Write(_tmpStrBuff);
    }
    

    static void CheckWindowSize()
    {
        bool windowTooSmall = Console.WindowWidth < BoardSize || Console.WindowHeight < BoardSize;
        if(windowTooSmall)
        {
            Console.Clear();
            WriteCentered("Window too small!");
        }
                
        // wait for window to be resized
        while(Console.WindowWidth < BoardSize || Console.WindowHeight < BoardSize) { }
    }
    
    static void WriteCentered(string text, int fromCenter = 0)
    {
        Console.SetCursorPosition(Console.WindowWidth / 2 - text.Length / 2, Console.WindowHeight / 2 + fromCenter);
        Console.Write(text);
    }


    private static int _obstaclesGenerated = 0;
    static void GenerateObstacles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Obstacles.Enqueue(new Obstacle(_obstaclesGenerated * ObstacleDistance, Random.Shared.Next(1, BoardSize - HoleSize)));
            _obstaclesGenerated++;
        }
    }
    
    record Obstacle(int X, int Hole);
}