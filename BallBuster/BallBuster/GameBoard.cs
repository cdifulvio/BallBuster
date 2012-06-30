using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;


namespace BallBuster
{
    class GameBoard
    {
        private Dictionary<string, Texture2D> mColorDictionary =
            new Dictionary<string, Texture2D>();

        private int numberOfRows;
        private int numberOfColumns;
        private Ball[,] ballMatrix = null;
        private List<Ball> lastPopulatedListOfBalls = new List<Ball>();

        private int selectedHorizontalIndex;
        private int previouslySelectedHorizontalIndex;
        private int selectedVerticalIndex;
        private int previouslySelectedVerticalIndex;

        private bool firstTap;
        private bool showScore;
        private bool isLost;

        private int totalScore;
        private int selectionScore;


        public bool IsLost
        {
            get { return isLost; }
        }

        public int Score
        {
            get { return totalScore; }
        }

        public int SelectedScore
        {
            get { return selectionScore; }
        }

        public bool ShowScore
        {
            get { return showScore; }
        }

        public GameBoard()
        {
        }

        public void Initialize()
        {
            selectedHorizontalIndex = -1;
            selectedVerticalIndex = -1;
            previouslySelectedHorizontalIndex = -1;
            previouslySelectedVerticalIndex = -1;
            firstTap = false;
            showScore = false;
            totalScore = 0;
            selectionScore = 0;
            isLost = false;
            
            //Clear the matrix before reinstatiating it
            if (ballMatrix != null)
            for (int i = 0; i < numberOfColumns; ++i)
            {
                for (int j = 0; j < numberOfRows; ++j)
                {
                    ballMatrix[i, j] = null;
                }
            }

            numberOfColumns = 480 / mColorDictionary["blue"].Width;
            numberOfRows = 660 / mColorDictionary["blue"].Height;
            ballMatrix = new Ball[numberOfColumns, numberOfRows];
            Random random = new Random();

            //Load all the balls into the game.
            for (int i = 0; i < numberOfColumns; ++i)
            {
                for (int j = 0; j < numberOfRows; ++j)
                {
                    Ball ball = new Ball();
                    int number = random.Next(0, 6);
                    switch (number)
                    {
                        case 0:
                            ball.Initialize(mColorDictionary["blue"], i, j, Ball.BallColor.Blue);
                            ballMatrix[i, j] = ball;
                            break;
                        case 1:
                            ball.Initialize(mColorDictionary["green"], i, j, Ball.BallColor.Green);
                            ballMatrix[i, j] = ball;
                            break;
                        case 2:
                            ball.Initialize(mColorDictionary["purple"], i, j, Ball.BallColor.Purple);
                            ballMatrix[i, j] = ball;
                            break;
                        case 3:
                            ball.Initialize(mColorDictionary["red"], i, j, Ball.BallColor.Red);
                            ballMatrix[i, j] = ball;
                            break;
                        case 4:
                            ball.Initialize(mColorDictionary["teal"], i, j, Ball.BallColor.Teal);
                            ballMatrix[i, j] = ball;
                            break;
                        case 5:
                            ball.Initialize(mColorDictionary["yellow"], i, j, Ball.BallColor.Yellow);
                            ballMatrix[i, j] = ball;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool Update(float xCoordinate, float yCoordinate)
        {
            bool removedBalls = false;

            if (yCoordinate >= 100 && yCoordinate <= 770)
            {
                previouslySelectedHorizontalIndex = selectedHorizontalIndex;
                previouslySelectedVerticalIndex = selectedVerticalIndex;

                selectedHorizontalIndex = (int)(xCoordinate / 60f);
                selectedVerticalIndex = (int)((yCoordinate - 100f) / 60f);

                //Make sure that the position is within the limits of the matrix
                if (selectedHorizontalIndex >= 0 && selectedVerticalIndex >= 0 && 
                    selectedVerticalIndex < numberOfRows && 
                    selectedHorizontalIndex < numberOfColumns && 
                    ballMatrix[selectedHorizontalIndex, selectedVerticalIndex].IsAlive)
                {
                    Ball selectedBall = ballMatrix[selectedHorizontalIndex, selectedVerticalIndex];

                    //If already tapped once and the second tap is in the same place, continue with removing the balls
                    if (previouslySelectedHorizontalIndex == selectedHorizontalIndex && 
                        previouslySelectedVerticalIndex == selectedVerticalIndex && 
                        firstTap)
                    {
                        firstTap = false;

                        RemoveAdjacentBalls(selectedBall, "");
                        removedBalls = true;

                        totalScore += selectionScore;
                        showScore = false;
                        UpdateColumns();
                        UpdateRows();

                        if (IsGameLost())
                        {
                            isLost = true;
                        }
                    }
                    //Else this is the first tap
                    else
                    {
                        //Get the score of the current selection
                        selectionScore = GetScore(selectedBall);

                        if (selectionScore != 1)
                        {
                            firstTap = true;
                            showScore = true;
                        }
                        else
                        {
                            firstTap = false;
                            showScore = false;
                        }
                    }
                }
            }

            return removedBalls;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < numberOfColumns; ++i)
            {
                for (int j = 0; j < numberOfRows; ++j)
                {
                    if (ballMatrix[i, j].IsAlive)
                    {
                        ballMatrix[i, j].Draw(spriteBatch);
                    }
                }
            }
        }

        public void AddBallTexture(string color, Texture2D texture)
        {
            mColorDictionary.Add(color, texture);
        }

        private void UpdateRows()
        {
            for (int i = 0; i < numberOfColumns; ++i)
            {
                for (int j = numberOfRows - 2; j >= 0; --j)
                {
                    if (!ballMatrix[i, j + 1].IsAlive)
                    {
                        //Shift the dead ball we just found to the top of the column
                        for (int k = j; k >= 0; --k)
                        {
                            Ball deadBall = ballMatrix[i, k + 1];
                            Ball aliveBall = null;

                            int counter = k;

                            //Swap the dead ball with the first alive ball it can find in the same
                            //column
                            while (aliveBall == null && counter >= 0)
                            {
                                if (ballMatrix[i, counter].IsAlive)
                                {
                                    aliveBall = ballMatrix[i, counter];
                                }
                                else
                                {
                                    --counter;
                                }
                            }

                            if (counter >= 0)
                            {
                                deadBall.Y = counter;
                                aliveBall.Y = k + 1;
                                ballMatrix[i, k + 1] = aliveBall;
                                ballMatrix[i, counter] = deadBall;
                            }
                        }
                    }
                }
            }
        }

        //Checks to see if there are any collumns which are completely empty. If so, move
        //all columns that are left of the dead one to the right so that they stick to existing balls
        private void UpdateColumns()
        {
            for (int i = numberOfColumns - 1; i > 0; --i)
            {
                bool allDead = true;

                //Check to see if all balls are dead in this column, if so contine the method
                for (int j = 0; j < numberOfRows; ++j)
                {
                    if (ballMatrix[i, j].IsAlive)
                    {
                        allDead = false;
                        break;
                    }
                }

                //If they are all dead, find the column to the left that is not completely dead 
                //and switch these two columns
                if (allDead)
                {
                    int columnToSwitchWith = 0;
                    bool isFound = false;

                    //Find the next non-dead column
                    for (int j = i - 1; j >= 0; --j)
                    {
                        if (isFound)
                        {
                            break;
                        }

                        for (int k = 0; k < numberOfRows; ++k)
                        {
                            if (ballMatrix[j, k].IsAlive)
                            {
                                columnToSwitchWith = j;
                                isFound = true;
                                break;
                            }
                        }
                    }

                    for (int j = 0; j < numberOfRows; ++j)
                    {
                        Ball deadBall = ballMatrix[i, j];
                        Ball aliveBall = ballMatrix[columnToSwitchWith, j];

                        deadBall.X = columnToSwitchWith;
                        aliveBall.X = i;
                        ballMatrix[i, j] = aliveBall;
                        ballMatrix[columnToSwitchWith, j] = deadBall;
                    }
                }
            }
        }

        private int GetScore(Ball ball)
        {
            lastPopulatedListOfBalls.Clear();
            PopulateListOfAdjacentBalls(ball, "");
            lastPopulatedListOfBalls.ForEach(b => b.IsUsed = false);

            return (int)Math.Pow(lastPopulatedListOfBalls.Count, 2);
        }

        private void PopulateListOfAdjacentBalls(Ball ball, string previousDirection)
        {
            if (ball.IsUsed)
            {
                return;
            }

            ball.IsUsed = true;

            int i = ball.X;
            int j = ball.Y;
            Ball.BallColor color = ball.color;

            Ball leftBall = null;
            Ball upBall = null;
            Ball rightBall = null;
            Ball downBall = null;

            if (i != 0)
            {
                leftBall = ballMatrix[i - 1, j];
            }

            if (j != 0)
            {
                upBall = ballMatrix[i, j - 1];
            }

            if (i != numberOfColumns - 1)
            {
                rightBall = ballMatrix[i + 1, j];
            }

            if (j != numberOfRows - 1)
            {
                downBall = ballMatrix[i, j + 1];
            }

            //If not, we will check adjacents and see if they are good to remove
            if (leftBall != null && leftBall.color == color && previousDirection != "right")
            {
                PopulateListOfAdjacentBalls(leftBall, "left");
            }
            if (rightBall != null && rightBall.color == color && previousDirection != "left")
            {
                PopulateListOfAdjacentBalls(rightBall, "right");
            }
            if (upBall != null && upBall.color == color && previousDirection != "down")
            {
                PopulateListOfAdjacentBalls(upBall, "up");
            }
            if (downBall != null && downBall.color == color && previousDirection != "up")
            {
                PopulateListOfAdjacentBalls(downBall, "down");
            }

            lastPopulatedListOfBalls.Add(ball);
        }

        private void RemoveAdjacentBalls(Ball ball, string previousDirection)
        {
            lastPopulatedListOfBalls.Clear();
            PopulateListOfAdjacentBalls(ball, "");
            lastPopulatedListOfBalls.ForEach(b => b.IsAlive = false);
        }

        //The user loses when there are no more combinations of balls left
        private bool IsGameLost()
        {
            for (int i = 0; i < numberOfColumns; ++i)
            {
                for (int j = 0; j < numberOfRows; ++j)
                {
                    lastPopulatedListOfBalls.Clear();
                    PopulateListOfAdjacentBalls(ballMatrix[i, j], "");

                    foreach (Ball b in lastPopulatedListOfBalls)
                    {
                        b.IsUsed = false;
                    }

                    //Found at least one combination, so the game is not lost
                    if (lastPopulatedListOfBalls.Count > 1)
                    {
                        return false;
                    }
                }
            }

            //No more combinations, end the game
            return true;
        }
    }
}
