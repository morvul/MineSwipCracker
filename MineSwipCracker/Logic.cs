using System;
using System.Collections.Generic;
using System.Drawing;
using MineSwipCracker.Enums;

namespace MineSwipCracker
{
    public static class Logic
    {
        public static Dictionary<Mouse.Buttons, List<Point>> GetSteps(CellType[,] board)
        {
            var rects = new List<int[]>();
            var state = new List<short[,]>[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    state[i, j] = new List<short[,]> {new short[3, 3]};
                }
            var needscan = new bool[board.GetLength(0), board.GetLength(1)];
            var verrs = new double[board.GetLength(0), board.GetLength(1)];
            double minver = double.MaxValue;
            List<Point> rclicks = new List<Point>();
            List<Point> lclicks = new List<Point>();
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    if (board[i, j] >= CellType.Cell1 && board[i, j] <= CellType.Cell8)       // если это числовая клетка
                    {
                        rects.Clear();
                        var flags = 0;
                        double closs = 0;                                   // число закрытых клеток
                        for (int i0 = i - 1; i0 < i + 2; i0++)              // определение окрестности числовой клетки
                            for (int j0 = j - 1; j0 < j + 2; j0++)
                                if (i0 > -1 && j0 > -1 && i0 < board.GetLength(0) && j0 < board.GetLength(1)) // исключение выхода за пределы поля
                                    if (board[i0, j0] == CellType.Free)               // если это закрытая клетка
                                    {
                                        rects.Add(new [] { i0 - i + 1, j0 - j + 1 });   // добавляем ее в список закрытых клеток
                                        state[i0, j0][0][1 - i0 + i, 1 - j0 + j] = 1;       // указываем закрытой клетке, что она соседствует с числовой 
                                        state[i, j][0][i0 - i + 1, j0 - j + 1] = 1;         // указываем числовой клетке...
                                        needscan[i0, j0] = true;                            // пометка границы фронта   
                                        closs++;                                            // учет закрытых клеток (пригодится для 3-его этапа)
                                    }
                                    else if (board[i0, j0] == CellType.Flag)                           // если это флажек, ведем учет их числа
                                        flags++;
                        var bombsCount = GetBombsCount(board[i, j]);
                        var ver = (bombsCount - flags) / closs;             // вероятность попадания по бомбе
                        if (minver > ver)
                            minver = ver;
                        for (int i0 = i - 1; i0 < i + 2; i0++)              // определение окрестности числовой клетки
                            for (int j0 = j - 1; j0 < j + 2; j0++)
                                if (i0 > -1 && j0 > -1 && i0 < board.GetLength(0) && j0 < board.GetLength(1)) // исключение выхода за пределы поля
                                    if (verrs[i0, j0] > ver)                // если пересекается с большей вероятностью
                                        verrs[i0, j0] = ver;
                        Getarr(ref state[i, j], rects, new short[3, 3], bombsCount - flags, 0);   // поиск всех возможных расстановок для клетки
                    }

            // 1-ый этап: исключение несочетаемых расстановок
            bool next;
            for (int i = 0; i < board.GetLength(0); i++)                                 // прогон всех клеток поля
                for (int j = 0; j < board.GetLength(1); j++)
                    if (needscan[i, j])                                 // если клетка на фроне (закрытая)
                        for (int i0 = 0; i0 < 3; i0++)                  // исследуем ее окрестности
                            for (int j0 = 0; j0 < 3; j0++)
                                if (state[i, j][0][i0, j0] == 1)        // нашлась цифровая клетка 
                                {
                                    next = false;
                                    for (int i1 = i0; i1 < 3; i1++)     // ищем еще одну
                                        for (int j1 = 0; j1 < 3; j1++)
                                            if (next && state[i, j][0][i1, j1] == 1)  // коль нашли
                                                Arrsval(state, board, i0, j0, i1, j1, i, j);        // удаляем несовместимые перестановки этих двух цифровых клеток
                                            else
                                              if (i0 == i1 && j0 == j1)
                                                next = true;
                                }
            for (int i = 0; i < board.GetLength(0); i++)          // прокликивание клеток
                for (int j = 0; j < board.GetLength(1); j++)
                    if (state[i, j].Count == 2)  // если для клетки есть только одна перестановка
                        Gooou(state, board, i, j, ref lclicks, ref rclicks);  // исследовать ее окрестность
            // 2-ой этап: выборка клеток, со значением, одинаковым для всех расстановок
            int buf;
            bool ya;
            for (int i = 0; i < board.GetLength(0); i++)          // начинаем просмотр всех клеток
                for (int j = 0; j < board.GetLength(1); j++)
                    if (board[i, j] >= CellType.Cell1 && board[i, j] <= CellType.Cell8 && state[i, j].Count > 2)       // если клетка числовая и для нее есть куча перестановок
                        for (int i1 = 0; i1 < 3; i1++)                  // берем каждую закрытую клетку из окрестности
                            for (int j1 = 0; j1 < 3; j1++)
                                if (state[i, j][0][i1, j1] == 1)
                                {
                                    buf = state[i, j][1][i1, j1];      // бомба/небомба для первой перестановки
                                    ya = true;
                                    for (int k = 2; k < state[i, j].Count; k++)            // ищем клетки, одинаковые для всех перестановок (для всех - бомбы / не бомбы)
                                        if (state[i, j][k][i1, j1] != buf) ya = false;
                                    if (ya && (board[i + i1 - 1, j + j1 - 1] == CellType.Free))      // если есть такая клетка 
                                    {
                                        if (buf == 0)                          // если она не бомба  - кликаем левой кнопкой
                                        {
                                            lclicks.Add(new Point(j + j1 - 1, i + i1 - 1));
                                        }
                                        if (buf == 1)                                      // если она бомба - кликаем правой кнопкой
                                        {
                                            board[i + i1 - 1, j + j1 - 1] = CellType.Flag;
                                            rclicks.Add(new Point(j + j1 - 1, i + i1 - 1));
                                        }
                                    }
                                }
            // 3-ий этап
            if (rclicks.Count == 0 && lclicks.Count == 0)      // если за предыдущие этапы ничего не кликнулось
            {
                List<Point> points = new List<Point>();        // список кандидатов на клик
                for (int i = 0; i < board.GetLength(0); i++)
                    for (int j = 0; j < board.GetLength(1); j++)
                        if (board[i, j] == CellType.Bonus ||
                            (board[i, j] == CellType.Free && (verrs[i, j] == minver || verrs[i, j] == 1)))   // если клетка закрытая и с минимальной вероятностью, либо граничит с числовыми клетками
                            points.Add(new Point(j, i));
                if (points.Count > 0)
                {
                    buf = (new Random()).Next(0, points.Count - 1);                              // выбор случайного индекса
                    lclicks.Add(points[buf]);
                }
            }

            var result = new Dictionary<Mouse.Buttons, List<Point>>
            {
                {Mouse.Buttons.Left, lclicks},
                {Mouse.Buttons.Right, rclicks}
            };
            return result;
        }

        private static int GetBombsCount(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Cell1:
                    return 1;
                case CellType.Cell2:
                    return 2;
                case CellType.Cell3:
                    return 3;
                case CellType.Cell4:
                    return 4;
                case CellType.Cell5:
                    return 5;
                case CellType.Cell6:
                    return 6;
                case CellType.Cell7:
                    return 7;
                case CellType.Cell8:
                    return 8;
                default:
                    return 0;
            }
        }

        static void Getarr(ref List<short[,]> state, List<int[]> rects, short[,] arr, int mines, int val) // рекурсивный метод нахождения всех перестановок
        {
            if (mines < 1)            // если все бомбы расставлены
            {
                state.Add(arr);       // сохранить расстановку и прекратить погружение
                return;
            }
            for (int i = val; i < rects.Count - mines + 1; i++)            // перебор расстановок val-ой бобмы
            {
                arr[rects[i][0], rects[i][1]] = 1;
                Getarr(ref state, rects, (short[,])arr.Clone(), mines - 1, i + 1); // перебор расстановок оставшихся бомб
                arr[rects[i][0], rects[i][1]] = 0;
            }
        }

        static void Arrsval(List<short[,]>[,] state, CellType[,] board, int i1, int j1, int i2, int j2, int y, int x)    // метод удаления несовместимых расстановок
        {
            bool ya, ya2;
            List<int[]> steps = new List<int[]>();                    // список закрытых клеток, общих для двух числовых
            for (int i = 0; i < 3; i++)                               // нахождение этих клеток
                for (int j = 0; j < 3; j++)
                    if ((Math.Abs(i - i1) + Math.Abs(j - j1) < 3) && (Math.Abs(i - i1) == 1 || Math.Abs(j - j1) == 1) &&
                        (Math.Abs(i - i2) + Math.Abs(j - j2) < 3) && (Math.Abs(i - i2) == 1 || Math.Abs(j - j2) == 1) &&
                        (y + i - 1 > -1 && x + j - 1 > -1 && y + i - 1 < board.GetLength(0) && x + j - 1 < board.GetLength(1)) && (board[y + i - 1, x + j - 1] == CellType.Free))
                        steps.Add(new [] { y + i - 1, x + j - 1 });
            int x1 = x + j1 - 1,
                y1 = y + i1 - 1,
                x2 = x + j2 - 1,
                y2 = y + i2 - 1;
            if (state[y1, x1].Count > state[y2, x2].Count)
            {
                x2 = x + j1 - 1;
                y2 = y + i1 - 1;
                x1 = x + j2 - 1;
                y1 = y + i2 - 1;
            }
            List<short[,]> a = state[y1, x1],
                           b = state[y2, x2];
            int ii = 1;
            while (ii < b.Count)                            // перебор расстановок для первой клетки
            {
                ya2 = false;
                for (int j = 1; j < a.Count && !ya2; j++)   // сравнение с расстановками для второй клетки
                {
                    ya = true;
                    for (int k = 0; k < steps.Count; k++)   // сравнение двух расстановок
                        ya &= (b[ii][steps[k][0] - y2 + 1, steps[k][1] - x2 + 1] == a[j][steps[k][0] - y1 + 1, steps[k][1] - x1 + 1]);
                    ya2 |= ya;
                }
                if (!ya2)                                  // если ни одна расстановка для второй клетки не совпадает с расстановкой для первой, то эта расстановка ошибочна
                    b.RemoveAt(ii);
                else
                    ii++;
            }
        }

        static void Gooou(List<short[,]>[,] state, CellType[,] board, int i, int j, ref List<Point> lclicks, ref List<Point> rclicks)      // прокликивание окрестности клетки
        {
            for (int i0 = i - 1; i0 < i + 2; i0++)
                for (int j0 = j - 1; j0 < j + 2; j0++)
                    if (i0 > -1 && j0 > -1 && i0 < board.GetLength(0) && j0 < board.GetLength(1) && board[i0, j0] == CellType.Free)  // если клетка закрытая 
                    {
                        if (state[i, j][1][i0 - i + 1, j0 - j + 1] == 0)                 // пустая
                        {
                            board[i0, j0] = CellType.Empty;
                            lclicks.Add(new Point(j0, i0));
                            // Mouse.clickIt(xs + 16 * j0 - 8, ys + 16 * i0 - 8, speed, Mouse.Buttons.LEFT);
                        }
                        if (state[i, j][1][i0 - i + 1, j0 - j + 1] == 1)                 // с бомбой
                        {
                            board[i0, j0] = CellType.Bomb;
                            rclicks.Add(new Point(j0, i0));
                            // Mouse.clickIt(xs + 16 * j0 - 8, ys + 16 * i0 - 8, speed, Mouse.Buttons.RIGHT);
                        }
                    }
            state[i, j].RemoveAt(1);
        }
    }
}
