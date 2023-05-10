using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Tetris
{
    class Program
    {
        const int D_X = 8; // 0,0 출력x위치
        const int D_Y = 4; // 0,0 출력y위치
        const int GRID_X = 12; // 테트리스 가로길이
        const int GRID_Y = 22; // 테트리스 세로길이
        static int[,] grid = new int[GRID_Y, GRID_X]; // 테트리스 판

        static int currentX = 0; // grid 배열 기준,,
        static int currentY = 0;
        static int currentRot = 0; // RatationNum

        static int[] bag;
        static int[] nextBag;
        static int currentIndex; // 블록모양
        static int bagIndex;

        static int maxTime = 20;
        static int timer = 0;
        static int amount = 0;

        static int holdIndex = -1;
        static int cleared = 0;

        static int score = 0;

        // const int BLOCK_START_X = 8;
        static int[,,,] positions =
        {
        {// I
        {{1,0},{1,1},{1,2},{1,3}}, // {_,_,0,0 / _,_,0,1},{ _,_,1,0 / _,_,1,1} ...
        {{0,2},{1,2},{2,2},{3,2}},
        {{2,0},{2,1},{2,2},{2,3}},
        {{0,1},{1,1},{2,1},{3,1}}
        },

        {// J
        {{0,0},{1,0},{1,1},{1,2}},
        {{0,1},{0,2},{1,1},{2,1}},
        {{1,0},{1,1},{1,2},{2,2}},
        {{0,1},{1,1},{2,0},{2,1}},
        },
        {// L
        {{0,2},{1,0},{1,1},{1,2}},
        {{0,1},{1,1},{2,1},{2,2}},
        {{1,0},{1,1},{1,2},{2,0}},
        {{0,0},{0,1},{1,1},{2,1}}
        },

        {// O
        {{0,0},{0,1},{1,0},{1,1}},
        {{0,0},{0,1},{1,0},{1,1}},
        {{0,0},{0,1},{1,0},{1,1}},
        {{0,0},{0,1},{1,0},{1,1}}
        },

        {// S
        {{0,1},{0,2},{1,0},{1,1}},
        {{0,1},{1,1},{1,2},{2,2}},
        {{1,1},{1,2},{2,0},{2,1}},
        {{0,0},{1,0},{1,1},{2,1}}
        },
        {// T
        {{0,1},{1,0},{1,1},{1,2}},
        {{0,1},{1,1},{1,2},{2,1}},
        {{1,0},{1,1},{1,2},{2,1}},
        {{0,1},{1,0},{1,1},{2,1}}
        },

        {// Z
        {{0,0},{0,1},{1,1},{1,2}},
        {{0,2},{1,1},{1,2},{2,1}},
        {{1,0},{1,1},{2,1},{2,2}},
        {{0,1},{1,0},{1,1},{2,0}}
        }
        };
        static ConsoleKeyInfo input;
        static void MakeGrid() // 완 / 그리드 테두리에 숫자 10 넣어두기...
        {
            for(int i = 0; i < grid.GetLength(1);i++)
            {
                grid[0, i] = 10;
                grid[21, i] = 10;
            }
            for(int i = 0; i < grid.GetLength(0); i++)
            {
                grid[i, 0] = 10;
                grid[i, 11] = 10;
            }
        }
        static void PrintGrid() // 완
        {
            Console.SetCursorPosition(55, 6);
            Console.Write("    ▲");
            Console.SetCursorPosition(67, 6);
            Console.Write(": 방향전환");
            Console.SetCursorPosition(55, 8);
            Console.Write("◀      ▶");
            Console.SetCursorPosition(67, 8);
            Console.Write(": 좌우이동");
            Console.SetCursorPosition(55, 10);
            Console.Write("    ▼");
            Console.SetCursorPosition(67, 10);
            Console.Write(": 하강");
            Console.SetCursorPosition(57, 12);
            Console.Write("Spacebar");
            Console.SetCursorPosition(67, 12);
            Console.Write(": 드랍");
            Console.SetCursorPosition(59, 14);
            Console.Write("Z");
            Console.SetCursorPosition(67, 14);
            Console.Write(": 홀드");
            Console.SetCursorPosition(D_X, 4);
            for (int i = 0; i < 12; i++)
            {
                Console.Write("◈");
            }
            for (int i = 0; i < 20; i++)
            {
                Console.SetCursorPosition(D_X, 5 + i);
                Console.Write("◈");
                Console.SetCursorPosition(D_X + 22, 5 + i);
                Console.Write("◈"); 
            }
            Console.SetCursorPosition(D_X, 25);
            for (int i = 0; i < 12; i++)
            {
                Console.Write("◈");
            }
            ////////////////next
            Console.SetCursorPosition(40, 4);
            Console.Write("- N E X T -");
            for(int i = 0; i < 5; i++)
            {
                Console.SetCursorPosition(38, 5 + i);
                Console.Write("◈");
                Console.SetCursorPosition(50, 5 + i);
                Console.Write("◈");
            }
            Console.SetCursorPosition(38, 10);
            for (int i = 0; i < 7; i++)
            {
                Console.Write("◈");
            }
            //////////////////// hold
            Console.SetCursorPosition(40, 13);
            Console.Write("- H O L D -");
            for (int i = 0; i < 5; i++)
            {
                Console.SetCursorPosition(38, 14 + i);
                Console.Write("◈");
                Console.SetCursorPosition(50, 14 + i);
                Console.Write("◈");
            }
            Console.SetCursorPosition(38, 19);
            for (int i = 0; i < 7; i++)
            {
                Console.Write("◈");
            }

        }
        static void Input() // 완
        {
            while(true)
            {
                input = Console.ReadKey(true);
            }
        }
        
        static void InputHandler()
            // 좌, 우, 아래 : 입력 > 간섭검사 > 간섭 아닐시 이동
            // 회전 :  입력 > 회전 > 간섭이면 닿지 않도록 조정
        {
            switch(input.Key)
            {
                case ConsoleKey.LeftArrow: // 왼쪽으로 이동
                    if(!isCollision(currentIndex, currentRot, grid, currentX - 1, currentY))
                    {
                        currentX -= 1;
                    }
                    break;
                case ConsoleKey.RightArrow: // 오른쪽으로 이동
                    if (!isCollision(currentIndex, currentRot, grid, currentX + 1, currentY))
                    {
                        currentX += 1;
                    }
                    break;
                case ConsoleKey.DownArrow: // 내려가기
                    timer = maxTime;
                    break;
                case ConsoleKey.Spacebar: // 바닥까지 하강
                    int i = 0;
                    while(true)
                    {
                        i++;
                        if(isCollision(currentIndex, currentRot, grid, currentX, currentY + i))
                        {
                            currentY = currentY + i - 1;
                            break;
                        }
                    }
                    break;
                case ConsoleKey.Z: // 홀드
                    if (holdIndex == -1)
                    {
                        holdIndex = currentIndex;
                        NewBlock();
                    }
                    else
                    {
                        if(!isCollision(holdIndex, 0, grid, currentX, currentY))
                        {
                            int temp = currentIndex;
                            currentIndex = holdIndex;
                            holdIndex = temp;
                        }
                    }
                    break;
                case ConsoleKey.UpArrow: // 회전
                    if(currentRot == 3)
                    {
                        if (!isCollision(currentIndex, 0, grid, currentX , currentY))
                        {
                            currentRot = 0;
                        }
                    }
                    else
                    {
                        if (!isCollision(currentIndex, currentRot + 1, grid, currentX, currentY))
                        {
                            currentRot += 1;
                        }
                    }
                    // 간섭이면 닿지 않도록 조정 < 이건 시간 많을때 해보자,,생각해봐!
                    break;
                case ConsoleKey.Escape:
                    GameOver();
                    break;
                default:
                    break;
            }
        }
        static int[] GenerateBag() // Knuth Shuffle,, / 한 세트에 겹치는 블록 없도록 하기 / 완
        {
            Random random = new Random();
            int n = 7;
            int[] arr = { 0, 1, 2, 3, 4, 5, 6 };
            while(n > 1)
            {
                int rand = random.Next(n--);
                int temp = arr[n];
                arr[n] = arr[rand];
                arr[rand] = temp;
            }
            return arr;

        }
        static bool isCollision(int index, int rot, int[,] grid, int x, int y)
        {
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                if (positions[index, rot, i, 1] + x > 10 || positions[index, rot, i, 1] + x < 1 || positions[index, rot, i, 0] + y > 20)
                {
                    return true;
                }
                if(grid[positions[index, rot, i, 0] + y, positions[index, rot, i, 1] + x] != 0)
                {
                    return true;
                }
            }
            return false;
        }
        static void NewBlock() // 완
        {
            if(bagIndex >= 7) // 7개 블럭 세트가 끝나면 하나 다시 만듬
            {
                bagIndex = 0;
                bag = nextBag;
                nextBag = GenerateBag();
            }
            //생성 위치 초기화
            currentY = 0;
            currentX = 5;
            currentIndex = bag[bagIndex]; // 0 ~ 6

            if(isCollision(currentIndex, currentRot, grid, currentX, currentY) && (amount > 1)) // 충돌조건,,,,
            {
                GameOver();
            }
            bagIndex++;
            amount++;
            score += 20;
        }
        static int[,] RenderGrid()
        {
            int[,] renderGrid = new int[GRID_Y, GRID_X]; // 22 12
            for (int i = 0; i < GRID_Y; i++) // 배경이랑 똑같은 출력할 배경을 만들기
            {
                for (int j = 0; j < GRID_X; j++)
                {
                    renderGrid[i, j] = grid[i, j];
                }
            }
            // 현재 블럭 추가하기
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                renderGrid[positions[currentIndex, currentRot, i, 0] + currentY, positions[currentIndex, currentRot, i, 1] + currentX] = currentIndex + 1; // 블럭을 블럭종류index로 채워넣기..!
            }
            return renderGrid;

        }
        static int[,] RenderNext()
        {
            int nextBagIndex = 0;
            int[,] nextGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextGrid[i, j] = 9;
                }
            }
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                if (bagIndex + 1 > 7)
                {
                    nextGrid[positions[nextBag[nextBagIndex], 0, i, 0], positions[nextBag[nextBagIndex], 0, i, 1]] = nextBag[nextBagIndex] + 1;
                }
                else
                {
                    nextGrid[positions[bag[bagIndex], 0, i, 0], positions[bag[bagIndex], 0, i, 1]] = bag[bagIndex] + 1;
                }
            }
            return nextGrid;
        }
        static int[,] RenderHold()
        {
            int[,] holdGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    holdGrid[i, j] = 9;
                }
            }
            if (holdIndex != -1)
            {
                for (int i = 0; i < positions.GetLength(2); i++)
                {
                    holdGrid[positions[holdIndex, 0, i, 0], positions[holdIndex, 0, i, 1]] = holdIndex + 1;
                }
            }
            return holdGrid;
        }
        static void GameOver()
        {
            Console.SetCursorPosition(8, 4);
            Console.Write("  _______      ___      .___  ___.  _______ ");
            Console.SetCursorPosition(8, 5);
            Console.Write(" /  _____|    /   '     |   '/   | |   ____|");
            Console.SetCursorPosition(8, 6);
            Console.Write("|  |  __     /  ^  '    |  '  /  | |  |__   ");
            Console.SetCursorPosition(8, 7);
            Console.Write("|  | |_ |   /  /_'  '   |  |'/|  | |   __|  ");
            Console.SetCursorPosition(8, 8);
            Console.Write("|  |__| |  /  _____  '  |  |  |  | |  |____ ");
            Console.SetCursorPosition(8, 9);
            Console.Write(" '______| /__/     '__' |__|  |__| |_______|");
            Console.SetCursorPosition(8, 10);
            Console.Write("  ______   ____    ____  _______ .______      ");
            Console.SetCursorPosition(8, 11);
            Console.Write(" /  __  '  '   '  /   / |   ____||   _  '     ");
            Console.SetCursorPosition(8, 12);
            Console.Write("|  |  |  |  '   '/   /  |  |__   |  |_)  |    ");
            Console.SetCursorPosition(8, 13);
            Console.Write("|  |  |  |   '      /   |   __|  |      /     ");
            Console.SetCursorPosition(8, 14);
            Console.Write("|  `--'  |    '    /    |  |____ |  |'  '----.");
            Console.SetCursorPosition(8, 15);
            Console.Write(" '______/      '__/     |_______|| _| `._____|");
            Console.SetCursorPosition(8, 16);
            Console.Write("                                                          \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                    \n" +
                "                                                                      ");
            Console.ReadLine();


            Environment.Exit(1); // 일단 이렇게,,
        }
        static void Print(int[,] renderGrid, int[,] holdGrid, int[,] nextGrid) // 여기서 색 바꿔주고 해야함,,
        {
            for (int i = 1; i < GRID_Y - 1; i++) // 테두리 제외했음
            {
                for (int j = 1; j < GRID_X - 1; j++)
                {
                    Console.SetCursorPosition(D_X + 2 * j, D_Y + i);
                    if (renderGrid[i, j] >= 0)
                    {
                        int index = renderGrid[i, j];
                        switch (index)
                        {
                            case 1:
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 2:
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 3:
                                Console.BackgroundColor = ConsoleColor.DarkYellow;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 4:
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 5:
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 6:
                                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            case 7:
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.Write("▣");
                                Console.ResetColor();
                                break;
                            default:
                                Console.Write("  ");
                                break;

                        }
                    }

                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Console.SetCursorPosition(42 + 2 * j, 15 + i);
                    int index = holdGrid[i, j];
                    switch (index)
                    {
                        case 1:
                            Console.BackgroundColor = ConsoleColor.Cyan;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 2:
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 3:
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 4:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 5:
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 6:
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 7:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        default:
                            Console.Write("  ");
                            break;

                    }
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Console.SetCursorPosition(42 + 2 * j, 6 + i);
                    int index = nextGrid[i, j];
                    switch (index)
                    {
                        case 1:
                            Console.BackgroundColor = ConsoleColor.Cyan;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 2:
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 3:
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 4:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 5:
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 6:
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        case 7:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.Write("▣");
                            Console.ResetColor();
                            break;
                        default:
                            Console.Write("  ");
                            break;

                    }
                }
            }
            Console.SetCursorPosition(8, 27);
        }
        // 블럭이 바닥에 닿으면,,
        // 1. 배경이랑 합치기
        // 2. 다 찬 열 있으면 지우고 내리기
        // 3. 새 블럭 만들기
        // 라인 지우는 함수
        // 라인 꽉 찼는지 아닌지 함수
        static void ClearRow(int row)
        {
            // 점수++
            for(int i = 1; i < 11; i++)
            {
                grid[row, i] = 0; // 줄 비우기
            }
            score += 100;
            
        }
        static void MoveRowDown(int row, int clearRows)
        {
            for (int i = 1; i < 11; i++)
            {
                if((row - clearRows) > 0)
                {
                    grid[row, i] = grid[row - clearRows, i];
                    grid[row - clearRows, i] = 0;
                }
                else
                {
                    grid[row, i] = 0;
                }
            }
        }
        static void ClearFullRows()
        {
            for(int i = GRID_Y - 2; i >= 0; i--)
            {
                if (isRowFull(i))
                {
                    ClearRow(i);
                    cleared++;
                    MoveRowDown(i, cleared);
                }
                else if(!isRowFull(i) && (cleared > 0) )
                {
                    MoveRowDown(i, cleared);
                }
            }
            cleared = 0;
        }
        static bool isRowFull(int row)
        {
            for (int i = 1; i < 11; i++)
            {
                if (grid[row, i] == 0)
                {
                    return false;
                }
            }
            return true;
        }
        static void DownCollision()
        {
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                grid[positions[currentIndex, currentRot, i, 0] + currentY, positions[currentIndex, currentRot, i, 1] + currentX] = currentIndex + 1;
            }
            
            NewBlock();
        }
        static void Main(string[] args)
        {
            System.Console.Title = "Tetris_KimHyunHee";
            Console.SetWindowSize(80,30);
            Console.CursorVisible = false;
            Thread inputThread = new Thread(Input);
            inputThread.Start();
            MakeGrid();
            PrintGrid(); // 그리드 출력 < 고정
            bag = GenerateBag();
            nextBag = GenerateBag();
            NewBlock();
            Console.SetCursorPosition(16, 11);
            Console.Write(" ####  ");
            Console.SetCursorPosition(16, 12);
            Console.Write("     # ");
            Console.SetCursorPosition(16, 13);
            Console.Write("  ###  ");
            Console.SetCursorPosition(16, 14);
            Console.Write("     # ");
            Console.SetCursorPosition(16, 15);
            Console.Write(" ####  ");
            Thread.Sleep(1000);
            Console.SetCursorPosition(16, 11);
            Console.Write(" ####  ");
            Console.SetCursorPosition(16, 12);
            Console.Write("     # ");
            Console.SetCursorPosition(16, 13);
            Console.Write("  ###  ");
            Console.SetCursorPosition(16, 14);
            Console.Write(" #     ");
            Console.SetCursorPosition(16, 15);
            Console.Write(" ##### ");
            Thread.Sleep(1000);
            Console.SetCursorPosition(16, 11);
            Console.Write("   #   ");
            Console.SetCursorPosition(16, 12);
            Console.Write("  ##   ");
            Console.SetCursorPosition(16, 13);
            Console.Write("   #   ");
            Console.SetCursorPosition(16, 14);
            Console.Write("   #   ");
            Console.SetCursorPosition(16, 15);
            Console.Write("   #   ");
            Thread.Sleep(1000);
            while (true)
            {
                Console.SetCursorPosition(39, 22);
                Console.Write("Score : {0}", score);
                Console.SetCursorPosition(8, 27);
                if((amount % 15) == 0)
                {
                    if(maxTime > 5)
                    {
                        maxTime -= 1;
                        amount = 2;
                    }

                }
                if (timer >= maxTime) // 중력 0.02초마다 타이머++1 / 타이머가 20이 되면 한칸 내려오기 /  이렇게 하는 이유는 키 입력을 0.02초마다 한번씩 받을 수 있기 때문 > 아래로 빨리 내려오게 할 수 있음
                {
                    //if(!충돌) 한칸내려와
                    if(!isCollision(currentIndex , currentRot, grid, currentX, currentY + 1))
                    {
                        currentY++;
                    }
                    else
                    {
                        DownCollision();
                    }
                    timer = 0;
                }
                timer++;

                InputHandler();
                input = new ConsoleKeyInfo();

                // 블럭
                int[,] randerGrid = RenderGrid();
                int[,] holdGrid = RenderHold();
                int[,] nextGrid = RenderNext();
                // 홀드
                // 다음블럭
                ClearFullRows();

                // 블럭, 홀드, 다음블럭 출력
                Print(randerGrid, holdGrid, nextGrid);
                Thread.Sleep(20); // 일정한속도,, 일단은,, 20*20=400 0.4초
            }
        }
    }
}
