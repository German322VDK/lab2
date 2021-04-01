using System;
using System.IO;

namespace lab2
{
    class Program
    {
        static double[,] a; // массив спинов
        static int N = 64; // количество спиннов NxN

        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            double En, M; // Энергия системы и намагниченность
            string fn1 = "1.txt", fn2 = "2.txt", fn3 = "3.txt"; // имена файлов
            using StreamWriter file1 = new StreamWriter(fn1); // поток файла 1.txt
            using StreamWriter file2 = new StreamWriter(fn2); // поток файла 2.txt
            using StreamWriter file3 = new StreamWriter(fn3); // поток файла 3.txt
            StreamWriter[] sw = new StreamWriter[80]; // массив потоков файлов
            a = new double[N, N]; // создание массива спинов
            int ir, jr; // индексы рандомного спина
            double E1, E2, d, rn, C, rd, spold, Eer = 0, Mer = 0, ES = 0, ESQ = 0, MS = 0; // переменные для подсчёта энергии намагниченности теплоёмкости и тд
            double[] E = new double[5]; // массив энергий системы
            double[] Esq = new double[5]; // массив квадратов энергий системы
            double[] Ms = new double[5]; // массив намагниченностей системы
            int ch = 0; // номер текущего файла
            Random r = new Random(); // объект рандома
            string[] sv = new string[80]; // массив 2ых названий 

            for (int i = 0; i < N; i++) // рандомим систему спинов
            {
                for (int j = 0; j < N; j++)
                {
                    a[i, j] = NextDouble(-1 * Math.PI, Math.PI);
                }
            }

            for (int k = 0; k < 80; k++) // считем названия
            {
                sv[k] = Convert.ToString(k, 2);
                int l = sv[k].Length;
                while (l < 7)
                {
                    sv[k] = "0" + sv[k];
                    l++;
                }
            }

            Console.WriteLine("Начинаем:");

            for (double t = 0.05; t <= 4; t += 0.05) // цикл по температуре
            {
                t = Math.Round(t, 2); // округляем (ps иногда бывало 0.1000000000000003)
                // обнуляем
                ES = 0;
                ESQ = 0;
                MS = 0;
                Eer = 0;
                Mer = 0;

                for (int i = 0; i < 5; i++) // 5 раз вызываем монтекарло и считаем среднее
                {
                    for (int mk = 0; mk < 1000000; mk++) // монтекарло
                    {
                        ir = r.Next(N);
                        jr = r.Next(N);
                        rd = NextDouble(-1 * Math.PI, Math.PI);

                        spold = a[ir, jr];
                        E1 = CreatEnS(ir, jr);
                        a[ir, jr] = rd;
                        E2 = CreatEnS(ir, jr);

                        if (E1 <= E2) 
                        {
                            rn = r.NextDouble();

                            d = Math.Exp(-1 * (E2 - E1) / t);

                            if (rn > d) a[ir, jr] = spold;
                        }
                    }

                    E[i] = EnSys();

                    Esq[i] = E[i] * E[i];

                    Ms[i] = Magnet();
                }

                for (int i = 0; i < 5; i++) // считаем среднее
                {
                    ES += E[i];
                    ESQ += Esq[i];
                    MS += Ms[i];
                }


                En = ES /= 5;
                ESQ /= 5;
                M = MS /= 5;

                C = (ESQ - ES * ES) / (t * t); // теплоёмкость

                for (int i = 0; i < 5; i++) // считаем ошибки
                {
                    Eer += (E[i] - ES) * (E[i] - ES);
                    Mer += (Ms[i] - MS) * (Ms[i] - MS);
                }

                Eer = Math.Sqrt(Eer / 4);
                Mer = Math.Sqrt(Mer / 4);

                Console.WriteLine("T= " + t + " En= " + En + " M= " + M + " C= " + C + " Eer= " + Eer + " Mer= " + Mer);

                file1.WriteLine(t + " " + En + " " + Eer);
                file2.WriteLine(t + " " + M + " " + Mer);
                file3.WriteLine(t + " " + C);

                using (sw[ch] = new StreamWriter("s-" + sv[ch] + ".txt")) // безопасно открываем поток записываем в файл и его закрываем
                {
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            sw[ch].WriteLine(i + "\t" + j + "\t" + 0 + "\t" + Math.Cos(a[i, j])
                                + "\t" + Math.Sin(a[i, j]) + "\t" + 0);
                        }
                    }
                }
                ch++;
            }

            Console.WriteLine("Всё:)");

            Console.ReadKey();
        }

        public static double[] CreatSpinArray(int i, int j) // считаем соседей
        {
            double[] s = new double[4];

            //Вверх
            if (i > 0) s[0] = a[i - 1, j];
            else s[0] = a[N - 1, j];
            //Право
            if (j < N - 1) s[1] = a[i, j + 1];
            else s[1] = a[i, 0];
            //Низ
            if (i < N - 1) s[2] = a[i + 1, j];
            else s[2] = a[0, j];
            //Лево
            if (j > 0) s[3] = a[i, j - 1];
            else s[3] = a[i, N - 1];

            return s;
        }

        public static double CreatEnS(int ien, int jen) // считаем энергию спина
        {
            double en = 0;
            double[] s = CreatSpinArray(ien, jen);
            for (int i = 0; i < 4; i++)
            {
                en += Math.Cos(a[ien, jen] - s[i]);
            }

            return en * (-1);
        }

        public static double EnSys() // Энергия системы
        {
            double en = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    en += CreatEnS(i, j);
                }
            }

            return en / (N * N);
        }

        public static double Magnet() //Намагниченность системы
        {
            double m, ms = 0, mc = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    ms += Math.Sin(a[i, j]);
                    mc += Math.Cos(a[i, j]);
                }
            }

            m = ms * ms + mc * mc;

            m /= (N * N * N * N);

            return m;
        }

        public static double NextDouble(double a, double b) // считаем случайное дробное число в промежутке от до
        {
            Random random = new Random();

            double x = random.NextDouble();

            return x * a + (1 - x) * b;
        } 
    }
}