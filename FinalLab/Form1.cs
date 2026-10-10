using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalLab
{
    public partial class Form1 : Form
    {
        private bool[,] cardTurned;
        private int[,] cardID;
        private PictureBox[,] cards;
        private bool playerTurn = true;
        private bool secondCard = false;
        private string suit;
        private string matchCoordinates;
        private int[,] originalPositionsX;
        private int[,] originalPositionsY;
        private PictureBox winPicBox;
        //Initialize the Task List (use to run all functions within it simultaneously
        private List<Task> animateAll = new List<Task>();
        private int XF = 520, YF = 270;
        private int offset = 10;

        public Form1()
        {
            InitializeComponent();
        }

        private async Task animateMove(int i, int j, int xf, int yf)
        {
            int x = cards[i, j].Location.X;
            int y = cards[i, j].Location.Y;

            double dx = xf - x;
            double dy = yf - y;
            double distance = (Math.Sqrt(Math.Pow((xf - x), 2) + Math.Pow((yf - y), 2)));

            // Normalize the vectors
            dx /= distance;
            dy /= distance;

            while (distance > offset)
            {
                cards[i, j].Location = new Point((int)(cards[i, j].Location.X + dx * offset), (int)(cards[i, j].Location.Y + dy * offset));
                distance -= offset;
                await Task.Delay(2);
            }

            // Snap into first card into position
            cards[i, j].Location = new Point(xf, yf);
            cards[i, j].BringToFront();

            // Increment for the card pile only
            if (xf == this.XF && yf == this.YF)
            {
                this.XF += 2;
            }
        }
        private void initAndShuffle()
        {
            // How random is done in C #
            Random rand = new Random();
            // rand.Next(5); // gives a number from 0 to 4
            int ID = 1;
            int lastColumn = 4;
            int counter = 0;

            // Assign column 1 and 2
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    cardID[i, j] = ID;
                }
                ID++;
            }

            // Assign column 3 and 4
            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j < 4; j++)
                {
                    cardID[i, j] = ID;
                }
                ID++;
            }

            // Assign last row
            for (int i = 0; i < 4; i++)
            {
                cardID[i, lastColumn] = ID;
                counter++;
                if (counter == 2)
                {
                    ID++;
                }
            }

            // Variables to shuffle
            int x = 0, y = 0, swapID;

            // Shuffle 
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    x = rand.Next(5);
                    y = rand.Next(4);

                    swapID = cardID[y, x];

                    cardID[y, x] = cardID[i, j];
                    cardID[i, j] = swapID;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Console.Write(cardID[i, j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Disable newGame Button initially
            btnNewGame.Enabled = false;

            // Aallocate the arrays
            cards = new PictureBox[4, 5];
            cardID = new int[4, 5];
            cardTurned = new bool[4, 5];
            originalPositionsX = new int[4, 5];
            originalPositionsY = new int[4, 5];

            // loop that sets the each element of the array to a
            // reference of each element on the form
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 5; j++)
                {
                    cards[i, j] = (PictureBox)this.Controls["p" + i.ToString() + j.ToString()];
                    cards[i, j].SizeMode = PictureBoxSizeMode.StretchImage;
                    cards[i, j].Click += Clicked_On_Card;
                    string cardBackPath = Path.Combine(Application.StartupPath, "Resources", "Cards", "red_back.png");
                    cards[i, j].Image = Image.FromFile(cardBackPath);
                    originalPositionsX[i, j] = cards[i, j].Location.X;
                    originalPositionsY[i, j] = cards[i, j].Location.Y;
                }

            //Debug for original positions
            //Console.WriteLine("Original Locations for each picturebox: ");
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 5; j++)
            //    {
            //        Console.Write("(" + originalPositionsX[i, j].ToString() + ", " + originalPositionsY[i, j].ToString() + ") ");
            //    }
            //    Console.Write("\n"); 
            //}

            //Initializes the cards and shuffles them
            initAndShuffle();
        }
        private async void btnNewGame_Click(object sender, EventArgs e)
        {
            btnNewGame.Enabled = false;
            winPicBox.Image = null;

            // Use for Shuffle Animation
            Random randI = new Random();
            Random randJ = new Random();
            int shuffle_i_1 = 0, shuffle_j_1 = 0, shuffle_i_2 = 0, shuffle_j_2 = 0;

            // Reset win message
            winMsglbl.Text = "";

            // Reset Task List
            this.animateAll.Clear();

            // Reset to original positions and clickability
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    cardTurned[i, j] = false;
                    this.animateAll.Add(animateMove(i, j, originalPositionsX[i, j], originalPositionsY[i, j]));
                }
                //animate each row of cards separately
                await Task.WhenAll(animateAll);
                animateAll.Clear();
            }
            
            // Get the directory of where red back image file
            // is located in order to set the red image of the card
            foreach (PictureBox card in cards)
            {
                // Dynamically build the path to red_back.png for the shuffle reset
                string cardBackPath = Path.Combine(Application.StartupPath, "Resources", "Cards", "red_back.png");
                card.Image = Image.FromFile(cardBackPath);
            }

            // Animate shuffle
            for (int a = 0; a < 15; a++)
            {
                // Show illusion of shuffling
                shuffle_i_1 = randI.Next(4);
                shuffle_j_1 = randJ.Next(5);
                shuffle_i_2 = randI.Next(4);
                shuffle_j_2 = randJ.Next(5);
                this.animateAll.Add(animateMove(shuffle_i_1, shuffle_j_1, originalPositionsX[shuffle_i_2, shuffle_j_2], originalPositionsY[shuffle_i_2, shuffle_j_2]));
                this.animateAll.Add(animateMove(shuffle_i_2, shuffle_j_2, originalPositionsX[shuffle_i_1, shuffle_j_1], originalPositionsY[shuffle_i_1, shuffle_j_1]));
                await Task.WhenAll(animateAll);
                animateAll.Clear();

                // Snap the shuffled cards to original positions
                cards[shuffle_i_1, shuffle_j_1].Location = new Point(originalPositionsX[shuffle_i_1, shuffle_j_1], originalPositionsY[shuffle_i_1, shuffle_j_1]);
                cards[shuffle_i_2, shuffle_j_2].Location = new Point(originalPositionsX[shuffle_i_2, shuffle_j_2], originalPositionsY[shuffle_i_2, shuffle_j_2]);

            }
            initAndShuffle();
        }

        // click event when a card is clicked
        private async void Clicked_On_Card(object sender, EventArgs e)
        {
            // get the reference of the picture box card object
            // that was clicked
            PictureBox cardSelected = (PictureBox)sender;

            // using the name of cardSelected, extract the row and column
            // coordinates of the object selcted [i, j]
            // if the card has already been turned, no work needs to be done
            int i = cardSelected.Name[1] - '0';
            int j = cardSelected.Name[2] - '0';
            int suitID;


            if (cardTurned[i, j]) { return; }

            cardTurned[i, j] = true;

            // Random used for different suits of cards
            Random suitRand = new Random();
            
            // first card: assign suit, match the second card with the suit
            if (!this.secondCard)
            {
                suitID = suitRand.Next(3);

                switch (suitID)
                {
                    case 0:
                        this.suit = "C";
                        break;
                    case 1:
                        this.suit = "D";
                        break;
                    case 2:
                        this.suit = "H";
                        break;
                    default:
                        this.suit = "S";
                        break;
                }
                this.secondCard = true;
            }
            else
            {
                this.secondCard = false;
            }

            // based on cardID [i, j]’s value, get the appropriate 
            // card image using a valid file directory path
            if (cardID[i, j] == 1)
            {
                // Dynamically build the path for Ace cards
                string filename = "A" + this.suit + ".png";
                string cardFrontPath = Path.Combine(Application.StartupPath, "Resources", "Cards", filename);
                cards[i, j].Image = Image.FromFile(cardFrontPath);
            }
            else
            {
                // Dynamically build the path for numbered cards
                string filename = cardID[i, j].ToString() + this.suit + ".png";
                string cardFrontPath = Path.Combine(Application.StartupPath, "Resources", "Cards", filename);
                cards[i, j].Image = Image.FromFile(cardFrontPath);
            }

            // if this is the first move
            if (playerTurn)
            {
                // store the coordinates of the current card turned
                matchCoordinates = i.ToString() + j.ToString();
                playerTurn = false;
            }
            // if this is the second move
            else
            {
                // since we wait for 2 seconds in the next
                // statement , we do not want any of the PictureBox
                // click events to trigger in the meantime
                foreach (PictureBox card in cards)
                    card.Enabled = false;
                // waits 2 seconds before the next line
                // of code executes
                await Task.Delay(800);
                // allow the PictureBoxes to be clickable
                foreach (PictureBox card in cards)
                    card.Enabled = true;
                // verify if the ID of this card selected
                // matches the last card selected)
                // if the cards have the same id , then set them both
                // to be invisible, else turn both of the over
                // and set both elements of cardTurned matrix
                // to be false
                //first flip coordinates
                int y = matchCoordinates[0] - '0'; //i
                int x = matchCoordinates[1] - '0'; //j
                if (cardID[y, x] != cardID[i, j])
                {
                    string cardBackPath = Path.Combine(Application.StartupPath, "Resources", "Cards", "red_back.png");

                    cards[i, j].Image = Image.FromFile(cardBackPath);
                    cards[y, x].Image = Image.FromFile(cardBackPath);

                    cardTurned[y, x] = false;
                    cardTurned[i, j] = false;
                }
                else //the cards match, play animation
                {
                    //Add the functions to Task List
                    this.animateAll.Add(animateMove(i, j, this.XF, this.YF));
                    this.animateAll.Add(animateMove(y, x, this.XF, this.YF));

                    //execute all functions in the Task List simultaneously
                    await Task.WhenAll(animateAll);
                }
                this.animateAll.Clear();
                playerTurn = true;
            }

            // check if game is over , if the game is over
            // have a message box display win and set
            bool gameIsOver = true;

            foreach (bool checkIsOver in cardTurned)
            {
                if (!checkIsOver)
                {
                    gameIsOver = false;
                }
            }
            if (gameIsOver)
            {
                winMsglbl.Text = "You Win!";
                btnNewGame.Enabled = true;
                winPicBox = (PictureBox)this.Controls["winPic"];
                winPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
                string imagePath = Path.Combine(Application.StartupPath, "Resources", "Misc", "thumbsup.png");
                winPicBox.Image = Image.FromFile(imagePath);
            }
        }
    }
}
