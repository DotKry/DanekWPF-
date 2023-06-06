using System;
using System.Globalization;
using Z.Expressions;

namespace DanekWPF
{ 
    public class Solver
    {
        public static double[,] ParseMatrix(string input)
        {
            string[] rows = input.Split(";", StringSplitOptions.RemoveEmptyEntries);

            int rowCount = rows.Length;
            int columnCount = rows[0].Split(",", StringSplitOptions.RemoveEmptyEntries).Length;

            double[,] matrix = new double[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                string[] elements = rows[i].Split(",", StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < columnCount; j++)
                {
                    double value = double.Parse(elements[j], System.Globalization.NumberStyles.Number);
                    matrix[i, j] = value;
                }
            }

            return matrix;
        }

        public static double[] ParseVector(string input)
        {
            string[] elements = input.Split(",", StringSplitOptions.RemoveEmptyEntries);

            // Создание одномерного массива
            double[] vector = new double[elements.Length];

            // Заполнение массива значениями
            for (int i = 0; i < elements.Length; i++)
            {
                double value = double.Parse(elements[i], System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture);
                vector[i] = value;
            }

            return vector;
        }


        public static void ConvertToDiagonalDominance(ref double[,] A, ref double[] b)
        {
            int n = A.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                int maxIndex = i;
                double maxValue = Math.Abs(A[i, i]);

                for (int j = i + 1; j < n; j++)
                    if (Math.Abs(A[j, i]) > maxValue)
                    {
                        maxIndex = j;
                        maxValue = Math.Abs(A[j, i]);
                    }

                if (maxIndex != i)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double temp = A[i, k];
                        A[i, k] = A[maxIndex, k];
                        A[maxIndex, k] = temp;
                    }

                    double tempB = b[i];
                    b[i] = b[maxIndex];
                    b[maxIndex] = tempB;
                }
            }
        }


        public class JacobiResult
        {
            public int Iter { get; set; }
            public double[] X { get; set; }
            public double[,] A { get; set; }
            public double[] b { get; set; }
            public JacobiResult(int iter, double[] x)
            {
                Iter = iter;
                X = x;
            }
            public override string ToString()
            {
                return $"X = {string.Join (" | ", X)} на итерации {Iter}";
            }
        }
        private static JacobiResult JacobiSolver(double[,] A, double[] b, double[] x, double eps, int n, int max_iter)
        { 
            double[] x_new = new double[n];
            double[] D_inv = new double[n];
            double[,] L_U = new double[n, n];
            for (int i = 0; i < n; i++)
            { // Инициализируем начальное приближение x
                x_new[i] = x[i];
            }
            int iter = 0;
            double error = eps + 1.0;
            while (error > eps && iter < max_iter)
            {
                iter++;
                for (int i = 0; i < n; i++)
                { // Вычисляем обратную диагональную матрицу D_inv
                    D_inv[i] = 1 / A[i, i];
                }
                for (int i = 0; i < n; i++)
                { // Разделяем матрицу A на D и L+U
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j)
                        {
                            L_U[i, j] = 0;
                        }
                        else
                        {
                            L_U[i, j] = A[i, j];
                        }
                    }
                }
                for (int i = 0; i < n; i++)
                { // Используем сжимающее отображение для получения нового значения x
                    double diff = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        diff += L_U[i, j] * x[j];
                    }
                    x_new[i] = D_inv[i] * (b[i] - diff);
                }
                error = 0.0;
                for (int i = 0; i < n; i++)
                { // Вычисляем норму разности между x и x_new
                    error += Math.Abs(x_new[i] - x[i]);
                }
                for (int i = 0; i < n; i++)
                { // Присваиваем x_new как начальное приближение для следующей итерации
                    x[i] = x_new[i];
                }
            }
            for (int i = 0; i < n; i++)
            { 
                x[i] = Math.Round(x[i], 2, MidpointRounding.AwayFromZero);
            }
            return new JacobiResult(iter, x);
        }

        public static JacobiResult JacobiInitialization(double[,] A, double[] b, string eps, string x_0, string max_iter)
        {
            int n = b.Length;
            double[] x = ParseVector (x_0); // Создаем начальное приближение x
            var epsel = Eval.Execute<double>(eps);
            var max_it = Eval.Execute<int>(max_iter);
            ConvertToDiagonalDominance(ref A, ref b);
            

            var result = JacobiSolver(A, b, x, epsel, n, max_it); // Решаем систему методом Якоби
            result.A = A;
            result.b = b;
            return result;

        }


        private static double Compressive(double x, string f, string f_prime)
        { // Функция g(x), которая является сжимающим отображением 
            var f_result = Eval.Execute<double>(f.ToLower(), new { x });
            var f_prime_result = Eval.Execute<double>(f_prime.ToLower(), new { x });
            return x - f_result / f_prime_result;
        }

        public class NewtonResult { 
            public int Iter { get; set; }
            public double X { get; set; }

            public NewtonResult(int iter, double x)
            {
                Iter = iter;
                X = x;
            }
            public override string ToString()
            {
                return $"X = {Math.Round(X, 2, MidpointRounding.AwayFromZero)} на итерации {Iter}";
            }
        }

        private static NewtonResult NewtonInitialization(string x0, string eps, string max_iter, string f, string f_prime)
        {
            var x = Eval.Execute<double>(x0);
            var epsel = Eval.Execute<double>(eps);
            var max_it = Eval.Execute<int>(max_iter);// Максимальное количество итераций 
            double x_prev;
            int iter = 0;
            do
            {
                x_prev = x;
                x = Compressive(x, f, f_prime);
                iter++;
            } while (Math.Abs(x - x_prev) > epsel && iter < max_it);
            x = Math.Round(x, 2, MidpointRounding.AwayFromZero);
            return new NewtonResult(iter-1, x);
        }


        public static NewtonResult SolveNewton(string f, string f_prime, string x0, string eps, string max_iter)
        {
            return NewtonInitialization(x0, eps, max_iter, f, f_prime);
        }

        public class GaussSeideResult
        {
            public int Iter { get; set; }
            public double[] X { get; set; }
            public double[,] A { get; set; }
            public double[] b { get; set; }
            public GaussSeideResult(int iter, double[] x)
            {
                Iter = iter;
                X = x;
            }
            public override string ToString()
            {
                return $"X = {string.Join(" | ", X)} на итерации {Iter}";
            }
        }
        public static GaussSeideResult GaussSeideSolver(double[,] A, double[] b, double eps, double [] x, int max_iter)
        {
            int n = b.Length;


            double[] x_new = (double[])x.Clone();
            double error = eps + 1.0;
            int iter = 0;

            while (error > eps && iter < max_iter)
            {
                iter++;
                error = 0.0;

                for (int i = 0; i < n; i++)
                {
                    double sum1 = 0.0;
                    double sum2 = 0.0;

                    for (int j = 0; j < i; j++)
                    {
                        sum1 += A[i, j] * x_new[j];
                    }
                    for (int j = i + 1; j < n; j++)
                    {
                        sum2 += A[i, j] * x[j];
                    }

                    x_new[i] = (b[i] - sum1 - sum2) / A[i, i];

                    error += Math.Abs(x_new[i] - x[i]);
                }

                for (int i = 0; i < n; i++)
                {
                    x[i] = x_new[i];
                }
            }
            for (int i = 0; i < n; i++)
            {
                x[i] = Math.Round(x[i], 2, MidpointRounding.AwayFromZero);
            }
            return new GaussSeideResult(iter, x);
        }
        public static GaussSeideResult GaussSeideInitialization(double[,] A, double[] b, string eps, string x_0, string max_iter)
        {
            int n = b.Length;
            double[] x = ParseVector(x_0); // Создаем начальное приближение x
            var epsel = Eval.Execute<double>(eps);
            var max_it = Eval.Execute<int>(max_iter);
            ConvertToDiagonalDominance(ref A, ref b);

            var result = GaussSeideSolver(A, b, epsel, x, max_it) ;
            result.A = A;
            result.b = b;
            return result;

        }

        public class RelaxationResult
        {
            public int Iter { get; set; }
            public double[] X { get; set; }
            public double[,] A { get; set; }
            public double[] b { get; set; }
            public RelaxationResult(int iter, double[] x)
            {
                Iter = iter;
                X = x;
            }
            public override string ToString()
            {
                return $"X = {string.Join(" | ", X)} на итерации {Iter}";
            }
        }

        public static RelaxationResult RelaxationSolver(double[,] A, double[] b, double eps, double[] x, int max_iter, double omega)
        {
            int n = b.Length;

            double[] x_new = (double[])x.Clone();
            double error = eps + 1.0;
            int iter = 0;

            while (error > eps && iter < max_iter)
            {
                iter++;
                error = 0.0;

                for (int i = 0; i < n; i++)
                {
                    double sum1 = 0.0;
                    double sum2 = 0.0;

                    for (int j = 0; j < i; j++)
                    {
                        sum1 += A[i, j] * x_new[j];
                    }

                    for (int j = i + 1; j < n; j++)
                    {
                        sum2 += A[i, j] * x[j];
                    }

                    x_new[i] = (1 - omega) * x[i] + (omega / A[i, i]) * (b[i] - sum1 - sum2);

                    error += Math.Abs(x_new[i] - x[i]);
                }

                for (int i = 0; i < n; i++)
                {
                    x[i] = x_new[i];
                }
            }
            for (int i = 0; i < n; i++)
            {
                x[i] = Math.Round(x[i], 2, MidpointRounding.AwayFromZero);
            }
            return new RelaxationResult(iter, x);
        }
        public static RelaxationResult RelaxationInitialization(double[,] A, double[] b, string eps, string x_0, string max_iter, string omega)
        {
            double[] x = ParseVector(x_0); // Создаем начальное приближение x
            var epsel = Eval.Execute<double>(eps);
            var max_it = Eval.Execute<int>(max_iter);
            var omega_val = Eval.Execute<double>(omega);
            ConvertToDiagonalDominance(ref A, ref b);

            var result = RelaxationSolver(A, b, epsel, x, max_it, omega_val);
            result.A = A;
            result.b = b;
            return result;

        }

        public class SimpleIterationResult
        {
            public int Iter { get; set; }
            public double X { get; set; }

            public SimpleIterationResult(int iter, double x)
            {
                Iter = iter;
                X = x;
            }
            public override string ToString()
            {
                return $"X = {Math.Round(X, 2, MidpointRounding.AwayFromZero)} на итерации {Iter}";
            }
        }
        public static SimpleIterationResult SimpleIterationInitialization(string g, string x_0, string eps, string max_iter)
        {
            double x = Eval.Execute<double>(x_0);
            var epsel = Eval.Execute<double>(eps);
            var max_it = Eval.Execute<int>(max_iter);

            double x_new = 0.0;
            double error = epsel + 1.0;
            int iter = 0;

            while (error > epsel && iter < max_it)
            {
                x_new = Eval.Execute<double>(g.ToLower(), new { x });

                error = Math.Abs(x_new - x);
                x = x_new;

                iter++;
            }
            x_new = Math.Round(x_new, 2, MidpointRounding.AwayFromZero);
            return new SimpleIterationResult(iter, x_new);
        }

        public static SimpleIterationResult SolveSimpleIteration(string g, string x_0, string eps, string max_iter)
        {
            return SimpleIterationInitialization(g, x_0, eps, max_iter);
        }
    }
}
