using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Frogger
{
    class Engine
    {
        string text; //used to center the text in the console, don't mind it

        IConsoleRenderer renderer;
        IUserInterface userInterface;
        List<GameObject> allObjects;
        Frog playerFrog;
        Coordinates frogStartingPosition;
        bool frogOnLane;

        Random randomizer = new Random();

        uint iterations = 0; //used to control the speed of the vehicles. Will update their position only if 
        uint vehicleSpeed;   //"iteration % vehicleSpeed == 0" and this.iteration is ++ed on each iteration 
                             //of the "while(true)" cycle in this.Run()
          
        int score;
        int lives { get; set; }

        public int SleepTime { get; set; }

        public Engine(IConsoleRenderer renderer, IUserInterface userInterface, //the game will practically run on sleepTime = 0
                      int lives = 5, uint vehicleSpeed = 15, int sleepTime = 20) //but for performance reasons, we'll make it 30
        {
            this.renderer = renderer;
            this.userInterface = userInterface;
            this.lives = lives;
            this.vehicleSpeed = vehicleSpeed;
            this.SleepTime = sleepTime;
            this.allObjects = new List<GameObject>();
        }

        public void AddObject(GameObject obj)
        {
            if (obj is Vehicle)
            {
                Vehicle tempVehicle = obj as Vehicle;
                for (int i = 0; i < tempVehicle.Length; i++)
                {
                    this.allObjects.Add(new Vehicle(new Coordinates(tempVehicle.TopLeft.Row, tempVehicle.TopLeft.Col + i), tempVehicle.Speed));
                }
            }
            else if (obj is Frog)
            {
                playerFrog = obj as Frog;
                this.frogStartingPosition = playerFrog.TopLeft;
            }

            this.allObjects.Add(obj);
        }

        public void MoveFrogLeft()
        {
            if ((this.playerFrog.TopLeft.Col - 1) >= 0)
            {
                this.playerFrog.MoveLeft();
                this.AddPointsIfFrogIsOnALane();
            }
            
        }

        public void MoveFrogRight()
        {
            if ((this.playerFrog.TopLeft.Col + 1) < Console.WindowWidth - 1)
            {
                this.playerFrog.MoveRight();
                this.AddPointsIfFrogIsOnALane();
            }
        }

        public void MoveFrogUp()
        {
            if (!frogOnLane)
            {
                frogOnLane = true;
            }

            if (this.playerFrog.TopLeft.Row - 2 >= 0)
            {
                this.playerFrog.MoveUp(); //row -= 2
                this.AddPointsIfFrogIsOnALane();
            }
        }

        public void MoveFrogDown()
        {
            if (frogOnLane)
            {
                if ((this.playerFrog.TopLeft.Row + 2) < Console.WindowHeight - 4)
                {
                    this.playerFrog.MoveDown();
                    this.AddPointsIfFrogIsOnALane();
                }
            }
            else
            {
                if ((this.playerFrog.TopLeft.Row + 2) < Console.WindowHeight - 2)
                {
                    this.playerFrog.MoveDown();
                    this.AddPointsIfFrogIsOnALane();
                }

            }
        }

        public void PauseScreen()
        {
            Console.Clear();
            text = "The game is paused... Press Enter to continue";
            Console.SetCursorPosition(Console.WindowWidth / 2 - text.Length / 2, Console.WindowHeight / 2);
            Console.Write(text);

            Console.ReadLine();
        }

        private void AddPointsIfFrogIsOnALane()
        {
            if (frogOnLane)
            {
                this.score++;
            }
        }

        private void GameOverScreen()
        {
            Console.Clear();

            text = "GAME OVER! Your score: " + this.score;
            Console.SetCursorPosition(Console.WindowWidth / 2 - text.Length / 2, Console.WindowHeight / 2 - 1);
            Console.Write(text);

            text = "Play again? Y/N";
            Console.SetCursorPosition(Console.WindowWidth / 2 - text.Length / 2, Console.WindowHeight / 2 + 1);
            Console.Write(text);

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo pressedKey = Console.ReadKey();
                    if (pressedKey.Key == ConsoleKey.Y)
                    {
                        Console.Clear();
                        return;
                    }
                    else if (pressedKey.Key == ConsoleKey.N)
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private GameObject CollisionDetector()
        {
            GameObject[] objectsOnCurrentRow =
                (from obj in this.allObjects
                 where (obj.TopLeft.Row == playerFrog.TopLeft.Row) && !(obj is Frog)
                 select obj).ToArray<GameObject>();

            for (int i = 0; i < objectsOnCurrentRow.Length; i++)
            {
                if (objectsOnCurrentRow[i].TopLeft.Col == playerFrog.TopLeft.Col)
                {
                    return objectsOnCurrentRow[i];
                }
            }

            return null;
        }

        public void Run()
        {
            while (true)
            {
                iterations++;
                this.renderer.RenderAll(this.lives, this.score);

                Thread.Sleep(this.SleepTime);

                this.userInterface.ProcessInput();

                this.renderer.ClearQueue();

                for (int i = 0; i < this.allObjects.Count; i++) //for better performance
			    {
                    if (iterations % vehicleSpeed == 0)
                    {
                        allObjects[i].Update();
                    }

                    this.renderer.EnqueueForRendering(allObjects[i]);
			    }

                if (vehicleSpeed >= 0)
                {
                    if (iterations % 500 == 0)
                    {
                        vehicleSpeed--;
                    }
                }

                GameObject collision = CollisionDetector();

                if (collision is Vehicle)
                {
                    this.lives--;
                    this.playerFrog.TopLeft = frogStartingPosition;
                    this.frogOnLane = false;
                }
                else if (collision is ScoreBonus)
                {
                    this.score += 50;

                    foreach (var obj in allObjects)
                    {
                        if (obj is ScoreBonus)
                        {
                            obj.TopLeft = RandomGenerators.RandomPosition();
                        }
                    }
                }
                else
                {
                    if (collision is Hole)
                    {
                        this.lives -= 2;
                        this.playerFrog.TopLeft = frogStartingPosition;
                        this.frogOnLane = false;
                    }
                    else if (collision is OneUpBonus)
                    {
                        this.lives++;
                    }
                    else if (collision is SlowerTrafficBonus)
                    {
                        this.vehicleSpeed += 2;
                    }

                    allObjects.Remove(collision);
                }

                if (randomizer.Next(0, 1500) == 3)
                {
                    allObjects.Add(RandomGenerators.StaticObjectOnRandomPosition());
                }

                if (this.lives <= 0)
                {
                    GameOverScreen();
                    return;
                }
            }
        }

		static void Main()
		{
			//or else it won't compile
		}
    }
}