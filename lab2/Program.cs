using System;
using System.IO;

namespace lab2
{
    class Program
    {
        static int[,] a; // массив спинов
        static int N = 16; // количество спиннов NxN

        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            double En, M; // Энергия системы и намагниченность
            string fn1 = "1.txt", fn2 = "2.txt", fn3 = "3.txt"; // имена файлов
            StreamWriter[] sw = new StreamWriter[50]; // массив потоков файлов
            a = new int[N, N]; // создание массива спинов
            int ir, jr; // индексы рандомного спина
            double E1, E2, d, rn, C, rd, spold, Eer = 0, Mer = 0, ES = 0, ESQ = 0, MS = 0; // переменные для подсчёта энергии намагниченности теплоёмкости и тд
            double[] E = new double[5]; // массив энергий системы
            double[] Esq = new double[5]; // массив квадратов энергий системы
            double[] Ms = new double[5]; // массив намагниченностей системы
            int ch = 0; // номер текущего файла
            Random r = new Random(); // объект рандома
            string[] sv = new string[80]; // массив 2ых названий 

            double[] EnAr = new double[80];
            double[] MAr = new double[80];
            double[] CAr = new double[80];
            double[] EerAr = new double[80];
            double[] MerAr = new double[80];
            for (int i = 0; i < N; i++) // рандомим систему спинов
            {
                for (int j = 0; j < N; j++)
                {
                    if (r.Next(2) == 0) a[i, j] = 1;
                    else a[i, j] = -1;
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
            
            for (double t = 0.1; t <= 5; t += 0.1) // цикл по температуре
            {
                t = Math.Round(t, 2); // округляем (ps иногда бывало 0.1000000000000003)
                // обнуляем
                ES = 0;
                ESQ = 0;
                MS = 0;
                Eer = 0;
                Mer = 0;
                using (sw[ch] = new StreamWriter(t + ".txt")) // безопасно открываем поток записываем в файл и его закрываем
                {
                    for (int i = 0; i < 5; i++) // 5 раз вызываем монтекарло и считаем среднее
                    {
                        for (int mk = 0; mk < 500000; mk++) // монтекарло
                        {
                            ir = r.Next(N);
                            jr = r.Next(N);
                            rd = NextDouble(-1 * Math.PI, Math.PI);

                            spold = a[ir, jr];
                            E1 = CreatEnS(ir, jr);
                            a[ir, jr] *= -1;
                            E2 = CreatEnS(ir, jr);

                            if (E1 <= E2)
                            {
                                rn = r.NextDouble();

                                d = Math.Exp(-1 * (E2 - E1) / t);

                                if (rn > d) a[ir, jr] *= -1;
                            }

                            if(mk % 500 == 0)
                            {
                                for (int l = 0; l < N; l++)
                                {
                                    for (int j = 0; j < N; j++)
                                    {
                                        sw[ch].Write(a[l, j] + " ");
                                    }
                                }
                                sw[ch].Write("\n");
                            }
                        }

                        E[i] = EnSys();

                        Esq[i] = E[i] * E[i];

                        Ms[i] = Magnet();

                        
                    }
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

                for (int i = 0; i < 5; i++) // считаем ошибки
                {
                    Eer += (E[i] - ES) * (E[i] - ES);
                    Mer += (Ms[i] - MS) * (Ms[i] - MS);
                }

                Eer = Math.Sqrt(Eer / 4);
                Mer = Math.Sqrt(Mer / 4);

                EnAr[ch] = En;
                MAr[ch] = M;
                EerAr[ch] = Eer;
                MerAr[ch] = Mer;

                Console.WriteLine("T= " + t + " En= " + En + " M= " + M + " Eer= " + Eer + " Mer= " + Mer);

                if (ch == 0)
                {
                    using (StreamWriter file1 = new StreamWriter("output.txt", false))
                    {
                        
                        file1.WriteLine(t + "\t" + En + "\t" + Eer + "\t" + M + "\t" + Mer);
                        
                    }
                }
                else
                {
                    using (StreamWriter file1 = new StreamWriter("output.txt", true))
                    {
                        file1.WriteLine(t + "\t" + En + "\t" + Eer + "\t" + M + "\t" + Mer);
                    }
                }

                ch++;
            }

            double T; // записываем в 3 файла для гнуплот (вынес из цикла Т ради скорости)
            
            

            Console.WriteLine("Всё:)");

            Console.ReadKey();
        }

        public static int[] CreatSpinArray(int i, int j) // считаем соседей
        {
            int[] s = new int[4];

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
            int[] s = CreatSpinArray(ien, jen);
            for (int i = 0; i < 4; i++)
            {
                en += s[i] * a[ien, jen];
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
            double m = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    m += a[i, j];
                }
            }

            m /= (N * N);

            m = Math.Abs(m);
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