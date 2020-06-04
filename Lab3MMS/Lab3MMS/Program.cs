using System;

namespace Lab3MMS
{
    class Program
    {
        class Params
        {
            private const int CNT = 3;
            private const double EPS = 1e-6;

            public int N { get; set; }
            public double[,] P { get; set; }
            public double[] M { get; set; }
            public int[] R { get; set; }

            private long Factorial(int k)
            {
                var ans = 1;
                for (int i = 1; i <= k; i++)
                {
                    ans *= k;
                }
                return ans;
            }

            public double[,] GetPParams()
            {
                var p = new double[CNT, N + 1];
                var e = GetEParams();
                for (int i = 0; i < CNT; i++)
                {
                    for (int k = 0; k < N + 1; k++)
                    {
                        p[i, k] = Math.Pow((e[i] / M[i]), k);
                        if (k <= R[i])
                        {
                            p[i, k] *= (1 / Factorial(k));
                        }
                        else
                        {
                            p[i, k] *= (1 / (Factorial(R[i]) * Math.Pow(R[i], (k - R[i]))));
                        }
                    }
                }
                return p;
            }

            public double[] GetEParams()
            {
                var e = new double[CNT];
                e[0] = 1;
                //e[1] = P[0, 1];
                //e[2] = P[0, 2];
                e[2] = 5.0 / 8.0;
                e[1] = ((1 - P[0, 0]) * e[0] + P[0, 3] * e[2]) / (1 - P[0, 1]);

                //e[0] = 10.0;
                //e[1] = 7.0;
                //e[2] = 1.0;
                return e;
            }

            public double GetNormalMultyplier()
            {
                var p = GetPParams();
                var temp = 0.0;
                for (int i = 0; i <= N; i++)
                {
                    for (int j = 0; j <= N - i; j++)
                    {
                        temp += p[0, i] * p[1, j] * p[2, N - i - j];
                    }
                }
                return Math.Pow(temp, -1);
            }

            public double[,] GetPCMOParams()
            {
                var ans = new double[CNT, N + 1];
                var c = GetNormalMultyplier();
                var p = GetPParams();
                for (int i = 0; i < N + 1; i++)
                {
                    for (int j = 0; j < N + 1 - i; j++)
                    {
                        ans[0, i] += c * p[0, i] * p[1, j] * p[2, N - i - j];
                        ans[1, i] += c * p[1, i] * p[0, j] * p[2, N - i - j];
                        ans[2, i] += c * p[2, i] * p[0, j] * p[1, N - i - j];
                    }

                }
                return ans;
            }

            public bool CheckNormal()
            {
                var p = GetPCMOParams();
                for (int i = 0; i < CNT; i++)
                {
                    var tempAns = 0.0;
                    for (int j = 0; j < N + 1; j++)
                    {
                        tempAns += p[i, j];
                    }
                    if (Math.Abs(tempAns - 1) > EPS)
                        return false;
                }
                return true;
            }

            public double[] GetAvgRequest()
            {
                var ans = new double[CNT];
                var p = GetPCMOParams();
                for (int i = 0; i < CNT; i++)
                {
                    for (int j = R[i] + 1; j < N + 1; j++)
                    {
                        ans[i] += (j - R[i]) * p[i, j];
                    }
                }
                return ans;
            }

            public double[] GetAvgNotFreeDevice()
            {
                var ans = new double[CNT];
                var p = GetPCMOParams();
                for (int i = 0; i < CNT; i++)
                {
                    for (int j = 0; j < R[i] - 1; j++)
                    {
                        ans[i] += (R[i] - j) * p[i, j];
                    }
                    ans[i] *= -1;
                    ans[i] += R[i];
                }
                return ans;
            }

            public double[] GetAvgRequestsInDevice()
            {
                var ans = new double[CNT];
                var l = GetAvgRequest();
                var r = GetAvgNotFreeDevice();
                for (int i = 0; i < CNT; i++)
                {
                    ans[i] = l[i] + r[i];
                }
                return ans;
            }

            public double[] GetIntensivityOutside()
            {
                var ans = new double[CNT];
                var r = GetAvgNotFreeDevice();
                for (int i = 0; i < CNT; i++)
                {
                    ans[i] = r[i] * M[i];
                }
                return ans;
            }

            public double[] GetAvgTimeInCMO()
            {
                var ans = new double[CNT];
                var m = GetAvgRequestsInDevice();
                var l = GetIntensivityOutside();
                for (int i = 0; i < CNT; i++)
                {
                    ans[i] = m[i] / l[i];
                }
                return ans;
            }

            public double[] GetAvgTimeInQueue()
            {
                var ans = new double[CNT];
                var m = GetAvgRequest();
                var l = GetIntensivityOutside();
                for (int i = 0; i < CNT; i++)
                {
                    ans[i] = m[i] / l[i];
                }
                return ans;
            }
        }

        static void Main(string[] args)
        {
            var mo = new Params
            {
                N = 20,
                P = new double[1, 4] { { 0.5, 0.4, 0.6, 0.2 } },
                M = new double[3] { 0.6, 0.9, 0.4 },
                R = new int[3] { 2, 3, 1 }
            };
            Console.WriteLine("Коефiцiєнти передачi:");
            foreach (var x in mo.GetEParams())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine($"Нормуючий множник: {mo.GetNormalMultyplier()}, його перевiрка дала {mo.CheckNormal()}");
            Console.WriteLine("Допомiжнi функцiї p(i, k):");
            var p = mo.GetPParams();
            var width = p.GetUpperBound(1);
            for (int i = 0; i < p.Length / width; i++)
            {
                Console.Write($"{i + 1}: ");
                for (int k = 0; k < width; k++)
                {
                    Console.Write($"{p[i, k]} ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Ймовiрнiсть k вимого в СМО i:");
            p = mo.GetPCMOParams();
            width = p.GetUpperBound(1);
            for (int i = 0; i < p.Length / width; i++)
            {
                Console.Write($"{i + 1}: ");
                for (int k = 0; k < width; k++)
                {
                    Console.Write($"{p[i, k]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Показники ефективностi:");
            Console.WriteLine("Середня кiлькiсть вимог у черзi СМОi:");
            foreach (var x in mo.GetAvgRequest())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine("Середня кiлькiсть зайнятих пристроїв у СМОi:");
            foreach (var x in mo.GetAvgNotFreeDevice())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine("Середня кiлькiсть вимог у СМОi:");
            foreach (var x in mo.GetAvgRequestsInDevice())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine("Iнтенсивнiсть вихiдного потоку вимог у СМОi:");
            foreach (var x in mo.GetIntensivityOutside())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine("Середнiй час перебування вимоги в СМОi:");
            foreach (var x in mo.GetAvgTimeInCMO())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();
            Console.WriteLine("Середнiй час очiкування в черзi СМОi:");
            foreach (var x in mo.GetAvgTimeInQueue())
            {
                Console.Write($"{x} ");
            }
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}
