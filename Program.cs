using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame
{
    class Game
    {
        static readonly int x = 80; // х и у окна консоли
        static readonly int y = 26;
        private static int PeriodTime = 200; // задержка передвижения
        static Walls walls; // объявляем
        static Snake snake;
        static FoodFactory foodFactory;
        static Timer time;

        static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;

        }

        static void Main()
        {
            Console.SetWindowSize(x + 1, y + 1);
            Console.SetBufferSize(x + 1, y + 1);
            Console.CursorVisible = false;

            walls = new Walls(x, y, '#'); // инициализируем
            snake = new Snake(x / 2, y / 2, 3);

            foodFactory = new FoodFactory(x, y, '¤');
            foodFactory.CreateFood();

            time = new Timer(Loop, null, 0, PeriodTime);
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    snake.Rotation(key.Key);
                }
            }
        }

        static void Loop(object obj)
        {
            if (walls.IsHit(snake.GetHead()) || snake.IsHit(snake.GetHead()))
            {
                time.Change(0, Timeout.Infinite);
            }
            else if (snake.Eat(foodFactory.food))
            {
                foodFactory.CreateFood();
                PeriodTime -= 3; // время на которое уменьшается время, следовательно увеличивается скорость, после еды 
                PeriodTime = Clamp(PeriodTime, 50, 100); // меньше 50 тайимер не опустится 
                Walls.DrawScore(0, 0, Snake.Score);
                time.Change(0, PeriodTime);
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\Users\User\Desktop\zmeika\.idea\ConsoleApp1\cookie.wav");
                player.Play();
            }
            else
            {
                snake.Move();
            }
        }
    }

    struct Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public char ch { get; set; }

        public static implicit operator Point((int, int, char) value) =>
              new Point { x = value.Item1, y = value.Item2, ch = value.Item3 };

        public static bool operator ==(Point a, Point b) =>
                (a.x == b.x && a.y == b.y) ? true : false;
        public static bool operator !=(Point a, Point b) =>
                (a.x != b.x || a.y != b.y) ? true : false;

        public void Draw()
        {
            DrawPoint(ch);
        }
        public void Clear()
        {
            DrawPoint(' ');
        }

        private void DrawPoint(char _ch)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(_ch);
        }
    }

    class Walls
    {
        private char ch;
        private List<Point> wall = new List<Point>();

        public Walls(int x, int y, char ch) // отрисовка стен
        {
            this.ch = ch;

            DrawHorizontal(x, 0);
            DrawHorizontal(x, y);
            DrawVertical(0, y);
            DrawVertical(x, y);
        }

        private void DrawHorizontal(int x, int y) // создаем горизонтальную линию
        {
            for (int i = 0; i < x; i++)
            {
                Point p = (i, y, ch);
                p.Draw();
                wall.Add(p);
            }
        }

        private void DrawVertical(int x, int y) // создаем вертикальную
        {
            for (int i = 0; i < y; i++)
            {
                Point p = (x, i, ch);
                p.Draw();
                wall.Add(p);
            }
        }

        static public void DrawScore(int x, int y, int score)
        {
            string ScoreText = $"Score: {score}";
            foreach (var i in ScoreText)
            {
                Point p = (x, y, i);
                p.Draw();
                x++;

            }
        }

        public bool IsHit(Point p)
        {
            foreach (var w in wall)
            {
                if (p == w)
                {
                    return true;
                }
            }
            return false;
        }
    }

    enum Direction //определим перечисление направления движения змейки.
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    class Snake
    {
        private List<Point> snake;

        private Direction direction;
        private int step = 1;
        private Point tail;
        private Point head;

        bool rotate = true;

        static public int Score;


        public Snake(int x, int y, int length) //Методы движения и поворота в зависимости он направления движения змейки.
        {
            direction = Direction.RIGHT;

            snake = new List<Point>();
            for (int i = x - length; i < x; i++)
            {
                Point p = (i, y, '*');
                snake.Add(p);

                p.Draw();
            }
        }

        public Point GetHead() => snake.Last();

        public void Move()
        {
            head = GetNextPoint();
            snake.Add(head);

            tail = snake.First();
            snake.Remove(tail);

            tail.Clear();
            head.Draw();

            rotate = true;
        }

        public bool Eat(Point p)
        {
            head = GetNextPoint();
            if (head == p)
            {
                snake.Add(head);
                head.Draw();
                Score++;
                
                return true;
            }
            return false;
        }

        public Point GetNextPoint()
        {
            Point p = GetHead();

            switch (direction)
            {
                case Direction.LEFT:
                    p.x -= step;
                    break;
                case Direction.RIGHT:
                    p.x += step;
                    break;
                case Direction.UP:
                    p.y -= step;
                    break;
                case Direction.DOWN:
                    p.y += step;
                    break;
            }
            return p;
        }

        public void Rotation(ConsoleKey key)
        {
            if (rotate)
            {
                switch (direction)
                {
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        if (key == ConsoleKey.DownArrow)
                            direction = Direction.DOWN;
                        else if (key == ConsoleKey.UpArrow)
                            direction = Direction.UP;
                        break;
                    case Direction.UP:
                    case Direction.DOWN:
                        if (key == ConsoleKey.LeftArrow)
                            direction = Direction.LEFT;
                        else if (key == ConsoleKey.RightArrow)
                            direction = Direction.RIGHT;
                        break;
                }
                rotate = false;
            }

        }

        public bool IsHit(Point p)
        {
            for (int i = snake.Count - 2; i > 0; i--)
            {
                if (snake[i] == p)
                {
                    return true;
                }
            }
            return false;
        }
    }

    class FoodFactory //генерация еды, будет заниматься созданием еды внутри границ.
    {
        int x;
        int y;
        char ch;
        public Point food { get; private set; }

        Random random = new Random();

        public FoodFactory(int x, int y, char ch)
        {
            this.x = x;
            this.y = y;
            this.ch = ch;
        }

        public void CreateFood()
        {
            food = (random.Next(2, x - 2), random.Next(2, y - 2), ch);
            food.Draw();
        }
    }
}