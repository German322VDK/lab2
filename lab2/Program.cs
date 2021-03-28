using System;
using System.Diagnostics;
using System.IO;

namespace lab2
{
    public class SpinSystem // система спинов
    {
        private readonly SpinItem[,] fourS; // массив объектов

        public int N { get; private set; } // длина массива

        public double En { get; private set; } // энергия системы

        public double M { get; private set; } // намагниченность системы

        public string filename1 { get; private set; } // имя файла1

        public string filename2 { get; private set; } // имя файла2

        public string filename3 { get; private set; } // имя файла2

        public SpinSystem(int _n, string _filename1, string _filename2, string _filename3) // конструктор
        {
            N = _n;

            filename1 = _filename1;

            filename2 = _filename2;

            filename3 = _filename3;

            fourS = new SpinItem[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    fourS[i, j] = new SpinItem();
                }
            }
        }

        public void Creat() // создаём массив объектов, рандомим спин, считаем соседей, 
                                 //энергию спинов и системы, намагниченность
        {
            using StreamWriter file1 = new StreamWriter(filename1);
            using StreamWriter file2 = new StreamWriter(filename2);
            using StreamWriter file3 = new StreamWriter(filename3);
            var timer = Stopwatch.StartNew();
            int ir=0, jr=0;
            double E1, E2, d, rn, C, Eer = 0, Mer = 0, ES = 0, ESQ =0, MS=0;
            double[] E = new double[5];
            double[] Esq = new double[5];
            double[] Ms = new double[5];

            Random r = new Random();

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (r.Next(2) == 1) fourS[i, j].a = 1;
                    else fourS[i, j].a = -1;
                }
            }

            Console.WriteLine("Начинаем:");

            for (double t = 0.05; t < 4; t += 0.05)
            {
                t = Math.Round(t, 2);

                ES = 0;
                ESQ = 0;
                MS = 0;
                Eer = 0;
                Mer = 0;

                for (int i = 0; i < 5; i++)
                {
                    for (int mk = 0; mk < 100000; mk++)
                    {
                        CreatSpinArraySys();

                        ir = r.Next(N);
                        jr = r.Next(N);

                        E1 = CreatEnS(ir, jr);

                        fourS[ir, jr].a *= -1;

                        E2 = CreatEnS(ir, jr);

                        if (E1 > E2) fourS[ir, jr].ens = E2;

                        else
                        {
                            rn = r.NextDouble();

                            d = Math.Exp(-1 * (E2 - E1) / t);

                            if (rn > d)
                                fourS[ir, jr].a *= -1;
                        }

                    }

                    CreatEnSys();

                    E[i] = EnSys();

                    Esq[i] = E[i] * E[i];

                    Ms[i] = Magnet();
                }

                for (int i = 0; i < 5; i++)
                {
                    ES += E[i];
                    ESQ += Esq[i];
                    MS += Ms[i];
                }
               
                En = ES /= 5;
                ESQ /= 5;
                M = MS /= 5;

                C = (ESQ - ES * ES) / (t * t);

                for (int i = 0; i < 5; i++)
                {
                    Eer += (E[i] - ES) * (E[i] - ES);
                    Mer += (Ms[i] - MS) * (Ms[i] - MS);
                }

                Eer = Math.Sqrt(Eer / 4);
                Mer = Math.Sqrt(Mer / 4);

                En = Math.Round(En, 4);
                M = Math.Round(M, 4);
                C = Math.Round(C, 4);
                Eer = Math.Round(Eer, 4);
                Mer = Math.Round(Mer, 4);

                Console.WriteLine("T= " + t + " En= " + En + " M= " + M + " C= " + C + " Eer= " +
                 Eer + " Mer= " + Mer + " таймер " + timer.Elapsed.Seconds);

                file1.WriteLine(t + " " + En + " " + Eer);
                file2.WriteLine(t + " " + M + " " + Mer);
                file3.WriteLine(t + " " + C);
            }

            Console.WriteLine("Всё:)");
        }

        public void CreatSpinArray(int i, int j) // считаем соседей
        {
            int[] a = new int[4];

            //Вверх
            if (i > 0) a[0] = fourS[i - 1, j].a;
            else a[0] = fourS[N - 1, j].a;
            //Право
            if (j < N - 1) a[1] = fourS[i, j + 1].a;
            else a[1] = fourS[i, 0].a;
            //Низ
            if (i < N - 1) a[2] = fourS[i + 1, j].a;
            else a[2] = fourS[0, j].a;
            //Лево
            if (j > 0) a[3] = fourS[i, j - 1].a;
            else a[3] = fourS[i, N - 1].a;

            fourS[i, j].f = a;
        }

        public void CreatSpinArraySys()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    CreatSpinArray(i, j);
                }
            }
        }

        public double CreatEnS(int ien, int jen) // считаем энергию спина
        {
            double en = 0;

            for(int i = 0; i < 4; i++)
            {
                en += fourS[ien, jen].f[i] * fourS[ien, jen].a;
            }

            fourS[ien, jen].ens = en * (-1);

            return en * (-1);
        }

        public void CreatEnSys()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    CreatEnS(i, j);
                }
            }
        }

        public double EnSys() // Энергия системы
        {
            double en=0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    en += fourS[i, j].ens;
                }
            }

            return en / (N*N);
        }

        public double Magnet() //Намагниченность системы
        {
            double m = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    m += fourS[i, j].a;
                }
            }

           m /= (N*N);

           m = Math.Abs(m);
            return m;
        }

        public void Print() // Вывод на экран всей информации об системе
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(fourS[i, j].a + "    ");
                }
                Console.WriteLine();
            }

            //Console.WriteLine();
            //Console.WriteLine($"Намагниченность системы равна {M}; полная энергия равна {En}");
            //Console.WriteLine();

            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < N; j++)
            //    {
            //        Console.WriteLine("число [" + i + "][" + j + "] " + fourS[i, j].a + "; энергия спина " + fourS[i, j].ens);

            //        Console.WriteLine("Cпины: вверх право низ лево");

            //        for (int k = 0; k < 4; k++)
            //        {
            //            Console.Write(fourS[i, j].f[k] + " ");
            //        }
            //        Console.WriteLine();
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine();
            //}

        }
    
    }


    public class SpinItem // объект спин
    {
        public int a { get; set; } // Спин

        public int[] f { get; set; } = new int[4]; //4 соседа верх право низ лево

        public double ens { get; set; } // энергия спина
    }


    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            int n = 16;
            string fn1 = "1.txt";
            string fn2 = "2.txt";
            string fn3 = "3.txt";

            SpinSystem system = new SpinSystem(n, fn1, fn2, fn3);

            system.Creat();

            Console.ReadKey();
        }
    }
}