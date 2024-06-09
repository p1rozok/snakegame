using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.SetWindowSize(120, 40);
        Console.SetBufferSize(120, 40);
        Console.CursorVisible = false;
        Random rand = new Random();
        int highScore = 0;
        while (true)
        {
            int score = 0;
            bool gameOver = false;
            int head_x = 20;
            int head_y = 10;
            int dir = 0;
            List<(int x, int y)> snake = new List<(int, int)> { (head_x, head_y) };
            bool[,] borders = new bool[Console.WindowWidth, Console.WindowHeight];
            CreateBordersWithSymmetricalPassages(borders, rand);
            (int food_x, int food_y) = GenerateFoodPosition(snake, borders, rand);
            (int? specialFood_x, int? specialFood_y) = (null, null);
            DateTime? specialFoodSpawnTime = null;
            bool specialFoodVisible = false;
            while (!gameOver)
            {
                if (head_x >= 0 && head_x < Console.WindowWidth && head_y >= 0 && head_y < Console.WindowHeight)
                {
                    Console.SetCursorPosition(snake.Last().x, snake.Last().y);
                    Console.Write("  ");
                }
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.D && dir != 2) dir = 0;
                    if (key.Key == ConsoleKey.S && dir != 3) dir = 1;
                    if (key.Key == ConsoleKey.A && dir != 0) dir = 2;
                    if (key.Key == ConsoleKey.W && dir != 1) dir = 3;
                }
                (head_x, head_y) = MoveSnake(head_x, head_y, dir);
                head_x = (head_x + Console.WindowWidth) % Console.WindowWidth;
                head_y = (head_y + Console.WindowHeight) % Console.WindowHeight;
                if (borders[head_x, head_y] || snake.Skip(1).Any(p => p == (head_x, head_y)))
                {
                    gameOver = true;
                    continue;
                }
                if (head_x == food_x && head_y == food_y)
                {
                    score++;
                    snake.Insert(0, (head_x, head_y));
                    (food_x, food_y) = GenerateFoodPosition(snake, borders, rand);
                    if (rand.Next(100) < 5)
                    {
                        (specialFood_x, specialFood_y) = GenerateFoodPosition(snake, borders, rand);
                        specialFoodSpawnTime = DateTime.Now;
                        specialFoodVisible = true;
                    }
                }
                else
                {
                    snake.Insert(0, (head_x, head_y));
                    snake.RemoveAt(snake.Count - 1);
                }
                if (specialFood_x.HasValue && specialFood_y.HasValue && head_x == specialFood_x && head_y == specialFood_y)
                {
                    score += 5;
                    specialFood_x = null;
                    specialFood_y = null;
                    specialFoodVisible = false;
                }
                foreach (var part in snake)
                {
                    Console.SetCursorPosition(part.x, part.y);
                    Console.Write("██");
                }
                Console.SetCursorPosition(food_x, food_y);
                Console.Write("██");
                if (specialFood_x.HasValue && specialFood_y.HasValue)
                {
                    if (specialFoodVisible)
                    {
                        Console.SetCursorPosition(specialFood_x.Value, specialFood_y.Value);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("██");
                        Console.ResetColor();
                    }
                    if (DateTime.Now - specialFoodSpawnTime > TimeSpan.FromSeconds(7))
                    {
                        specialFoodVisible = !specialFoodVisible;
                    }
                    if (DateTime.Now - specialFoodSpawnTime > TimeSpan.FromSeconds(10))
                    {
                        specialFood_x = null;
                        specialFood_y = null;
                        specialFoodVisible = false;
                    }
                }
                Console.SetCursorPosition(0, 0);
                Console.Write($"Score: {score}  High Score: {highScore}");
                Thread.Sleep(50);
            }
            if (score > highScore)
            {
                highScore = score;
            }
            Console.Clear();
            Console.SetCursorPosition((Console.WindowWidth - 15) / 2, Console.WindowHeight / 2 - 1);
            Console.WriteLine("Game Over!");
            Console.SetCursorPosition((Console.WindowWidth - 24) / 2, Console.WindowHeight / 2);
            Console.WriteLine("Press any key to restart.");
            Console.SetCursorPosition((Console.WindowWidth - 23) / 2, Console.WindowHeight / 2 + 1);
            Console.WriteLine($"Your Score: {score}  High Score: {highScore}");
            Console.ReadKey();
            Console.Clear();
        }
    }
    static (int, int) MoveSnake(int head_x, int head_y, int dir)
    {
        if (dir == 0) head_x += 2;
        if (dir == 1) head_y += 1;
        if (dir == 2) head_x -= 2;
        if (dir == 3) head_y -= 1;
        return (head_x, head_y);
    }
    static (int, int) GenerateFoodPosition(List<(int, int)> snake, bool[,] borders, Random rand)
    {
        int x, y;
        do
        {
            x = rand.Next(0, Console.WindowWidth / 2) * 2;
            y = rand.Next(0, Console.WindowHeight);
        } while (snake.Contains((x, y)) || borders[x, y]);
        return (x, y);
    }
    static void CreateBordersWithSymmetricalPassages(bool[,] borders, Random rand)
    {
        int width = borders.GetLength(0);
        int height = borders.GetLength(1);
        int passageWidth = 8;
        Array.Clear(borders, 0, borders.Length);
        int passageX1 = rand.Next(passageWidth, width - passageWidth);
        int passageY1 = rand.Next(passageWidth, height - passageWidth);
        for (int x = 0; x < width; x++)
        {
            if (x < passageX1 || x >= passageX1 + passageWidth)
            {
                borders[x, 0] = true;
                borders[x, height - 1] = true;
            }
        }
        for (int y = 0; y < height; y++)
        {
            if (y < passageY1 || y >= passageY1 + passageWidth)
            {
                borders[0, y] = true;
                borders[width - 1, y] = true;
                borders[width - 2, y] = true; 
                borders[1, y] = true;         
            }
        }
        CreateOppositePassage(borders, passageX1, passageWidth, 0, height - 1, true);
        CreateOppositePassage(borders, passageY1, passageWidth, 0, width - 1, false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (borders[x, y])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write("█");
                }
            }
        }
    }
    static void CreateOppositePassage(bool[,] borders, int passageStart, int passageWidth, int primaryPos, int secondaryPos, bool horizontal)
    {
        for (int i = 0; i < passageWidth; i++)
        {
            if (horizontal)
            {
                borders[passageStart + i, secondaryPos] = false;
            }
            else
            {
                borders[secondaryPos, passageStart + i] = false;
            }
        }
    }
}
