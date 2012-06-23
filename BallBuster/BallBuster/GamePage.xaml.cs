using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace BallBuster
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager contentManager;
        GameTimer timer;
        SpriteBatch spriteBatch;
        // For rendering the XAML onto a texture
        UIElementRenderer elementRenderer;

        int highScore;

        float xcoord;
        float ycoord;

        GameBoard gameBoard;

        enum GameState
        {
            Title,
            Game
        }

        GameState gameState;
        Song gameplayMusic;

        public GamePage()
        {
            // Use the LayoutUpdate event to know when the page layout 
            // has completed so that we can create the UIElementRenderer.
            LayoutUpdated += new EventHandler(GamePage_LayoutUpdated);
            InitializeComponent();
            TouchPanel.EnabledGestures = GestureType.Tap;

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            gameState = GameState.Game;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            //If there is a gesture, kill it so it doesn't break the functionality for the rest of the code
            if (TouchPanel.IsGestureAvailable)
            {
                TouchPanel.ReadGesture();
            }

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            highScore = GetHighScore();
            HighScoreNumber.Text = highScore.ToString();

            gameBoard = new GameBoard();

            gameplayMusic = contentManager.Load<Song>("Sounds/menuMusic");

            //Load the content for the textures of the 6 different types of balls
            Texture2D blue = contentManager.Load<Texture2D>("BallSprites/blue");
            Texture2D green = contentManager.Load<Texture2D>("BallSprites/green");
            Texture2D purple = contentManager.Load<Texture2D>("BallSprites/purple");
            Texture2D red = contentManager.Load<Texture2D>("BallSprites/red");
            Texture2D teal = contentManager.Load<Texture2D>("BallSprites/teal");
            Texture2D yellow = contentManager.Load<Texture2D>("BallSprites/yellow");

            gameBoard.AddBallTexture("blue", blue);
            gameBoard.AddBallTexture("green", green);
            gameBoard.AddBallTexture("purple", purple);
            gameBoard.AddBallTexture("red", red);
            gameBoard.AddBallTexture("teal", teal);
            gameBoard.AddBallTexture("yellow", yellow);

            try
            {
                MediaPlayer.Play(gameplayMusic);
                MediaPlayer.IsRepeating = true;
            }
            catch { }
                
            gameBoard.Initialize();

            // TODO: use this.content to load your game content here
            
            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            MediaPlayer.Stop();

            gameBoard = null;

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            //Check if there is a gesture available.
            if (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                //Only read it if it's a Tap gesture
                if (gesture.GestureType == GestureType.Tap)
                {
                    xcoord = gesture.Position.X;
                    ycoord = gesture.Position.Y;

                    if (gameState == GameState.Game)
                    {
                        gameBoard.Update(xcoord, ycoord);

                        if (gameBoard.IsLost)
                        {
                            SaveHighScore(gameBoard.Score);
                            MessageBox.Show("YOU LOSE HAH", "YOu lose", MessageBoxButton.OK);
                            NavigationService.GoBack();
                        }
                    }
                }
            }

            //Make sure the labels have the correct value
            TotalScoreLabel.Text = gameBoard.Score.ToString();

            if (gameBoard.ShowScore)
            {
                FloatingScoreLabel.Text = gameBoard.SelectedScore.ToString();
            }
            else
            {
                FloatingScoreLabel.Text = string.Empty;
            }
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.Black);

            // Render the Silverlight controls using the UIElementRenderer.
            elementRenderer.Render();

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            //Draw the gameboard
            gameBoard.Draw(spriteBatch);

            // Using the texture from the UIElementRenderer, 
            // draw the Silverlight controls to the screen.
            spriteBatch.Draw(elementRenderer.Texture, Vector2.Zero, Color.White);

            spriteBatch.End();
        }

        private void SaveHighScore(int score)
        {
            if (score > highScore)
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string directoryName = System.IO.Path.GetDirectoryName("HighScore");

                    if (string.IsNullOrEmpty(directoryName) || !store.DirectoryExists("HighScore"))
                    {
                        store.CreateDirectory("HighScore");
                    }

                    using (IsolatedStorageFileStream fileStream = store.OpenFile("HighScore\\ballhighscore.txt", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(score);
                            writer.Close();
                        }
                    }
                }
            }
        }

        private int GetHighScore()
        {
            int high = 0;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (store.FileExists("HighScore\\ballhighscore.txt"))
                    {
                        using (IsolatedStorageFileStream fileStream = store.OpenFile("HighScore\\ballhighscore.txt", FileMode.Open, FileAccess.Read))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                string line;
                                if ((line = reader.ReadLine()) != null)
                                {
                                    high = int.Parse(line);
                                }

                                reader.Close();
                            }
                        }
                    }
                }
                catch
                {
                    //No high score yet
                    high = 0;
                }
            }

            return high;
        }

        private void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            // Create the UIElementRenderer to draw the XAML page to a texture.

            // Check for 0 because when we navigate away the LayoutUpdate event
            // is raised but ActualWidth and ActualHeight will be 0 in that case.
            if ((ActualWidth > 0) && (ActualHeight > 0))
            {
                SharedGraphicsDeviceManager.Current.PreferredBackBufferWidth = (int)ActualWidth;
                SharedGraphicsDeviceManager.Current.PreferredBackBufferHeight = (int)ActualHeight;
            }

            if (null == elementRenderer)
            {
                elementRenderer = new UIElementRenderer(this, (int)ActualWidth, (int)ActualHeight);
            }

        }
    }
}