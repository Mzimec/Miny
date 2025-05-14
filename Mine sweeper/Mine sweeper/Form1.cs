using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mine_sweeper
{
    public partial class Form1 : Form
    {
        // Deklarace konstant jako velikost herního pole, či základní rozměry základních ovládacích prvků
        private const int pixels = 600;
        private const int menuSize = 40;
        private const int menuWidth = 150;
        
        // Deklarace proměnných a jejich nastavení pro  vstupní hru
        private int size;
        private int square = 10;
        private int mines = 20;
        private int minesInGame;
        private int winCount;
        private bool gameOver;
        private bool firstTouch;
        private bool viewSettings;
        private bool borderless;
        private Button[,] grid;
        private bool[,] mineGrid;
        private Button winButton = new Button();
        private Button menuButton = new Button();
        private Button restartButton = new Button();
        private Button applyButton = new Button();
        private Button mineCountButton = new Button();
        private Button borderlessButton = new Button();
        private Button helpButton = new Button();
        private TextBox setScale = new TextBox();
        private TextBox setMineNumber = new TextBox();
        private Label scale = new Label();
        private Label mineNumber = new Label();
        private Label helpLabel = new Label();
 
        


        public Form1()
        {
            InitializeControls();
            InitializeComponent();
            InitializeGame();
        }

        // Inicializuje herní UI
        private void InitializeControls()
        {
            // Příručka pro začátečníka
            helpLabel.Size = new Size(pixels, pixels);
            helpLabel.Location = new Point(0, menuSize);
            helpLabel.Text = "Goal of the game is to reveal all cells without mines.\n" +
                    "Revealed cells have on themselves number which describes how many mines are in neighbouring cells.\n" +
                    "Mines are shown as character X.\n" +
                    "Left mouse click reveals cell on which you clicked.\n" +
                    "Right mouse click will flag the cell, use it when you think there is a mine.\n" +
                    "On top left you have a number of remaining mines in game based on number of flagged cells.\n" +
                    "Restart button will restart the game with the same settings as last game.\n" +
                    "Menu button will open the settings where you can change size of minefield and number of mines.\n" +
                    "Do not forget to click apply button if you want your new settings to be saved.";
            helpLabel.Font = new Font("Serif", 14);
            Controls.Add(helpLabel);
                    
            // Tlačítko na otevření příručky
            helpButton.Size = new Size(menuWidth, menuSize);
            helpButton.Location = new Point(3 * menuWidth, 0);
            helpButton.Text = "Help";
            helpButton.MouseDown += HelpButtonMouseDown;
            Controls.Add(helpButton);

            // Tlačítko na přepínání mezi módy hry (normální a bez hranic)
            borderlessButton.Size = new Size(menuWidth, menuSize);
            borderlessButton.Location = new Point(3 * menuWidth / 2, 6 * menuSize);
            borderlessButton.Text = "Normal";
            borderlessButton.MouseDown += BorderlessButtonMouseDown;
            Controls.Add(borderlessButton);

            // Gratulace k vítezství
            winButton.Size = new Size(2 * menuWidth, 4 * menuSize);
            winButton.Location = new Point(menuWidth, 2 * menuWidth);
            winButton.Text = "Congratulation!\nYou Won!";
            winButton.MouseDown += RestartButtonMouseDown;
            winButton.Font = new Font("Serif", 14);
            Controls.Add(winButton);

            // Popisek pro nastavení velikosti herního pole
            scale.Size = new Size(menuWidth, menuSize);
            scale.Location = new Point(menuWidth, 2 * menuSize);
            scale.Text = "Scale";
            Controls.Add(scale);

            // Popisek pro nastavení počtu min
            mineNumber.Size = new Size(menuWidth, menuSize);
            mineNumber.Location = new Point(menuWidth, 4 * menuSize);
            mineNumber.Text = "Mines";
            Controls.Add(mineNumber);

            // Textbox, kde se nastavuje velikost pole
            setScale.Size = new Size(menuWidth, menuSize);
            setScale.Location = new Point(2 * menuWidth, 2 * menuSize);
            setScale.Text = square.ToString();
            setScale.KeyPress += TextBoxKeyDown;
            Controls.Add(setScale);

            // Textbox , kde se nastavuje počet min
            setMineNumber.Size = new Size(menuWidth, menuSize);
            setMineNumber.Location = new Point(2 * menuWidth, 4 * menuSize);
            setMineNumber.Text = mines.ToString();
            setMineNumber.KeyPress += TextBoxKeyDown;
            Controls.Add(setMineNumber);

            // Pole ukazující počet zbylých min v závislosti na počtu označených polí
            mineCountButton.Size = new Size(menuWidth, menuSize);
            mineCountButton.Location = new Point(0, 0);
            mineCountButton.Text = minesInGame.ToString();
            Controls.Add(mineCountButton);

            // Restart tlačítko pro spuštění nové hry se stejným nastavením.
            restartButton.Size = new Size(menuWidth, menuSize);
            restartButton.Location = new Point(menuWidth, 0);
            restartButton.Text = "Restart";
            restartButton.MouseDown += RestartButtonMouseDown;
            Controls.Add(restartButton);

            // Menu tlačítko otevírá nastavení
            menuButton.Size = new Size(menuWidth, menuSize);
            menuButton.Location = new Point(2 * menuWidth, 0);
            menuButton.Text = "Menu";
            menuButton.MouseDown += MenuButtonMouseDown;
            Controls.Add(menuButton);

            // Apply tlačítko potvrzuje změny provedené v nastavení
            applyButton.Size = new Size(menuWidth, menuSize);
            applyButton.Location = new Point(3 * menuWidth / 2, 8 * menuSize);
            applyButton.Text = "Apply";
            applyButton.MouseDown += ApplyButtonMouseDown;
            Controls.Add(applyButton);
        }

        // Initializuje novou hru Mine sweepera
        private void InitializeGame()
        {
            // Nastaví duležité proměnné na jejich výchozí hodnoty
            minesInGame = mines;
            size = pixels / square;
            winCount = 0;
            firstTouch = true;
            gameOver = false;
            viewSettings = false;

            // Schová nastavení
            scale.Visible = false;
            setScale.Visible = false;
            mineNumber.Visible = false;
            setMineNumber.Visible = false;
            applyButton.Visible = false;
            winButton.Visible = false;
            borderlessButton.Visible = false;
            helpLabel.Visible = false;

            // Nastaví UI informace podle správných hodnot proměnných
            setMineNumber.Text = mines.ToString();
            setScale.Text = square.ToString();
            mineCountButton.Text = minesInGame.ToString();

            // Vygeneruje herní pole
            grid = new Button[square, square];
            mineGrid = new bool[square, square];
            for (int i = 0; i < square; i++)
            {
                for (int j = 0; j < square; j++)
                {
                    grid[i, j] = new Button();
                    grid[i, j].Size = new Size(size, size);
                    grid[i, j].Location = new Point(size * j, size * i + menuSize);
                    grid[i, j].MouseDown += GridButtonMouseDown;
                    // Nastaví font správné velikosti podle velikosti herního pole
                    grid[i, j].Font = new Font("Serif",size/3);
                    Controls.Add(grid[i, j]);
                }
            }
        }

        // Náhodně vybere pozice min na herním poli 
        private void GenerateMines()
        {
            Random rand = new Random();
            for (int i = 0; i < mines; i++)
            {
                int row, col;
                do
                {
                    row = rand.Next(0, square);
                    col = rand.Next(0, square);
                } while (mineGrid[row, col]);
                mineGrid[row, col] = true;
            }
        }

        // Kontroluje, že hráč může dát pouze numerické hodnoty do herního pole.
        private void TextBoxKeyDown(object sender, KeyPressEventArgs e)
        {
            {
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            }
        }

        // Mění herní mód z borderless na normal
        private void BorderlessButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if(e.Button == MouseButtons.Left)
            {
                if (borderlessButton.Text == "Borderless") { borderlessButton.Text = "Normal"; }
                else { borderlessButton.Text = "Borderless"; }
            }
        }

        // Uloží všechny změny v nastavení do proměnných
        private void ApplyButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (e.Button == MouseButtons.Left)
            {
                RemoveGrid();
                square = Convert.ToInt32(setScale.Text);
                // Limituje velikost hrního pole do logických velikostí
                if (square < 5) { square = 5; }
                else if (square > 40) { square = 40; }
                mines = Convert.ToInt32(setMineNumber.Text);
                // Limituje počet min do logického počtu
                if (mines > (square - 1) * (square - 1)) { mines = (square - 1) * (square - 1); }
                else if (mines < 5) { mines = 5; }
                // Uloží správný mód
                if (borderlessButton.Text == "Normal") { borderless = false; }
                else { borderless = true; }
                InitializeGame();
            }           
        }

        // Řídí otevírání a zavírání příručky
        private void HelpButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if(e.Button == MouseButtons.Left)
            {
                if (restartButton.Enabled)
                {
                    OpenHelpWindow();
                }
                else
                {
                   CloseHelpWindow();
                }
            }
        }

        // Otevře příručku
        private void OpenHelpWindow()
        {
            if (!viewSettings)
            {
                for (int i = 0; i < square; i++)
                {
                    for (int j = 0; j < square; j++)
                    {
                        grid[i, j].Visible = false;
                    }
                }
            }
            else
            {
                scale.Visible = false;
                setScale.Visible = false;
                mineNumber.Visible = false;
                setMineNumber.Visible = false;
                applyButton.Visible = false;
                borderlessButton.Visible = false;
            }
            mineCountButton.Enabled = false;
            restartButton.Enabled = false;
            menuButton.Enabled = false;
            helpLabel.Visible = true;
        }

        // Zavře příručku
        private void CloseHelpWindow()
        {
            mineCountButton.Enabled = true;
            restartButton.Enabled = true;
            menuButton.Enabled = true;
            helpLabel.Visible = false;
            if (!viewSettings)
            {
                for (int i = 0; i < square; i++)
                {
                    for (int j = 0; j < square; j++)
                    {
                        grid[i, j].Visible = true;
                    }
                }
            }
            else
            {
                scale.Visible = true;
                setScale.Visible = true;
                mineNumber.Visible = true;
                setMineNumber.Visible = true;
                applyButton.Visible = true;
                borderlessButton.Visible = true;
            }
        }

        // Řídí otevírání a zavírání nastavení
        private void MenuButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (e.Button == MouseButtons.Left)
            {
                if (viewSettings) { SettingsHide(); viewSettings = false; }
                else { SettingsShow(); viewSettings = true; }
            }
        }

        // Řídí restartování hry
        private void RestartButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (e.Button == MouseButtons.Left) {
                RemoveGrid();
                InitializeGame(); 
            }
        }

        // Řídí ovládání herního pole
        private void GridButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            // Nastaví proměnné row a col podle pozice zmáčklého tlačítka
            int row = (button.Top - menuSize) / size;
            int col = button.Left / size;

            // Řídí kliknutí levým tlačítkem
            if (e.Button == MouseButtons.Left)
            {
                // Zařídí, aby první odhalené tlačítko nebylo mina a ani s ní nesousedilo
                if (firstTouch)
                {
                    firstTouch = false;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            mineGrid[(row + i+square)%square, (col + j+square)%square] = true;
                        }
                    }
                    GenerateMines();
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            mineGrid[(row + i +square)%square, (col + j + square)%square] = false;
                        }
                    }
                }
                // Volá odhalení políčka
                RevealSpot(row, col);
            }
            // Řídí klinutí pravým tlačítkem, kterým si pole označíme
            else if (e.Button == MouseButtons.Right)
            {
                if (button.Text == "") { 
                    if (minesInGame > 0)
                    {
                        button.Text = "F";
                        minesInGame--;
                        mineCountButton.Text = minesInGame.ToString();
                    }
                }
                else if (button.Text == "F") { 
                    button.Text = ""; 
                    minesInGame++;
                    mineCountButton.Text = minesInGame.ToString();
                }
            }
        }

        // Bool, kterým získáme informaci, se daná pozice nachází v herním poli
        private bool InGrid(int row, int col)
        {
            if(row >= 0 && row < square && col >= 0 && col < square)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Odhalí všechny okolní pole (Pokud některé sousední pole nesousedí s minou, volá se rekurzivně)
        private void RevealSurroundings(int row, int col, Queue<Tuple<int,int>> queue)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // Proměnné, které používáme, abychom našli sousední pole
                    int irow = -1; int jcol = -1;
                    // Nastaví správně irow a jcol podle vybraného módu
                    if (borderless) { irow = (row + i + square) % square; jcol = (col + j + square) % square; }
                    else
                    {
                        if (InGrid(row + i, col + j)) { irow = row + i; jcol = col + j; }
                    }
                    if (irow >= 0)
                    {
                        // Tuple obsahující informace o pozici pole
                        var curTuple = new Tuple<int, int>(irow, jcol);
                        // Neodkrýváme pole označené vlajkou
                        if (grid[irow, jcol].Text != "F")
                        {
                            // Kotrola jestli nepracujeme s polem, se kterým jsme již pracovali
                            if (!queue.Contains(curTuple))
                            {
                                int count = CountAdjacentMines(irow, jcol);
                                queue.Enqueue((Tuple<int, int>)curTuple);
                                if (grid[irow, jcol].Enabled) { winCount++; }
                                grid[irow, jcol].Enabled = false;
                                // Ukaže počet sousedícíh min, pokud je alespoň jedna
                                if (count > 0)
                                {
                                    grid[irow, jcol].Text = count.ToString();
                                    grid[irow, jcol].BackColor = Color.Linen;
                                }
                                // Rekurzivně volá funkci RevealSurroundings()
                                else
                                {
                                    grid[irow, jcol].BackColor = Color.Linen;
                                    RevealSurroundings(irow, jcol, queue);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Řídí odhalování pole
        private void RevealSpot(int row, int col)
        {
            // Kontroluje zda není pole označené vlajkou
            if (grid[row, col].Text != "F")
            {
                grid[row, col].Enabled = false;
                // Řídí výsledek pokud je na poli mina
                if (mineGrid[row, col])
                {
                    grid[row, col].Text = "X";
                    if (!gameOver) { GameOver();  }

                }
                // Odhali pole
                else
                {
                    winCount++;
                    int count = CountAdjacentMines(row, col);
                    if (count > 0) 
                    {
                        grid[row, col].Text = count.ToString();
                        grid[row, col].BackColor = Color.Linen;
                    }
                    // Řídí možnost, že se v okolí pole nenachází mina
                    else
                    {
                        Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();
                        queue.Enqueue(new Tuple<int, int>(row, col));
                        grid[row, col].Text = "";
                        grid[row, col].BackColor = Color.Linen;
                        RevealSurroundings(row, col, queue);
                    }
                }
                // Kontroluje zda jsme jíž nenašli všechny miny, pokud ano tak volá funkci GameWon()
                if (square * square - mines <= winCount) { GameWon(); }
            }    
        }

        // Spočítá všechny miny na sousedních polích
        private int CountAdjacentMines(int row, int col)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // Mění funkci v závislosti na zvoleném herním módu
                    if (borderless)
                    {
                        if (mineGrid[(row + i + square) % square, (col + j + square) % square])
                        {
                            count++;
                        }
                    }
                    else
                    {
                        if (InGrid(row+i, col+j) && mineGrid[row + i, col + j]) { count++; }
                    }
                }
            }
            return count;
        }

        // Řídí konec hry, pokud jsme klikli na minu
        private void GameOver()
        {
            gameOver = true;
            // Zablokuje klikaní na herní pole
            for (int i = 0; i < square; i++)
            {
                for(int j = 0; j < square; j++)
                {

                    if (mineGrid[i, j]) { RevealSpot(i, j); }
                    // Ukáže chybně položené vlajky
                    else { if (grid[i, j].Text == "F") { grid[i, j].BackColor = Color.LightPink; } }
                    grid[i, j].Enabled = false;
                }
            }
        }

        // Schová herní pole a ukáže nastavení
        private void SettingsShow()
        {
            for (int i=0; i<square; i++)
            {
                for( int j=0; j<square; j++)
                {
                    grid[i,j].Visible = false;
                }
            }

            winButton.Visible = false;
            scale.Visible = true;
            mineNumber.Visible = true;
            setMineNumber.Visible = true;
            setScale.Visible = true;
            applyButton.Visible = true;
            borderlessButton.Visible = true;
        }

        // Schová nastavení a ukáže herní pole
        private void SettingsHide()
        {
            for (int i = 0; i < square; i++)
            {
                for (int j = 0; j < square; j++)
                {
                    grid[i, j].Visible = true;
                }
            }

            scale.Visible = false;
            mineNumber.Visible = false;
            setMineNumber.Visible = false;
            setScale.Visible = false;
            applyButton.Visible = false;
            borderlessButton.Visible = false;
        }

        // Odstrání současné herní pole
        private void RemoveGrid()
        {
            for (int i = 0; i < square; i++)
            {
                for(int j=0; j<square; j++)
                {
                    mineGrid[i, j] = false;
                    grid[i,j].Dispose();
                }
            }
        }

        // Řídí vítězství ve hře
        private void GameWon()
        {
            for (int i = 0; i < square; i++)
            {
                for (int j=0; j<square; j++)
                {
                    grid[i,j].Enabled = false;
                }
            }
            winButton.Visible = true;
        }

    }
}
