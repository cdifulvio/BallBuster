using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BallBuster
{
    public class HighScoreManager
    {
        private HighScoreManager()
        {
        }

        public static bool SaveHighScore(int score, int highScore)
        {
            bool scoreWasBetter = false;

            if (score > highScore)
            {
                scoreWasBetter = true;

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

            return scoreWasBetter;
        }

        public static int GetHighScore()
        {
            int high = 0;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (store.FileExists("HighScore\\ballhighscore.txt"))
                    {
                        using (IsolatedStorageFileStream fileStream =
                            store.OpenFile("HighScore\\ballhighscore.txt", FileMode.Open, FileAccess.Read))
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
    }
}
