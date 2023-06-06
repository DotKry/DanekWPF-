using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DanekWPF
{
    public partial class MainWindow : Window
    {
        private List<Task> Tasks = new List<Task>();
        private Task CurrentTask = null;

        private Method CurrentMethod = Method.Jacobi;

        public MainWindow()
        {
            InitializeComponent();
            MethodCombo.SelectedIndex = (int)CurrentMethod;
            LoadTasks();
        }

        private void LoadTasks()
        {
            using (StreamReader r = new StreamReader("tasks.json"))
            {
                string json = r.ReadToEnd();
                Tasks = JsonConvert.DeserializeObject<List<Task>>(json);
            }
            foreach (var task in Tasks)
            {
                var taskLabel = new Label();
                taskLabel.Content = task.Name;
                taskLabel.MouseUp += TasksList_TaskClicked;
                TasksList.Items.Add(taskLabel);
            }
        }


        private void DanekButton_Click(object sender, RoutedEventArgs e)
        {
            double[,] matrixA = { };
            double[] vectorB = { };
            if (CurrentMethod != Method.Newton && CurrentMethod != Method.SimpleIteration)
            {
                matrixA = Solver.ParseMatrix(MatrixAInput.Text);
                vectorB = Solver.ParseVector(VectorBInput.Text);
            }
            var eps = EpsInput.Text;
            var x_0 = X_0Input.Text;
            var maxIt = MaxIterInput.Text;
            switch (CurrentMethod)
            {
                case Method.Jacobi:    
                    var result = Solver.JacobiInitialization(matrixA, vectorB, eps, x_0, maxIt);
                    Result.Content = string.Join("; ", result);
                    var matrixAstr = new List<string> ();
                    for (int i=0;i<result.A.GetLength(0); i++) {
                        var row = new double[result.A.GetLength(1)];
                        for (int j = 0; j < result.A.GetLength(1); j++)
                        {
                            
                            row [j] = result.A[i,j];
                        }
                        matrixAstr.Add(string.Join(", ", row));
                    }
                    MatrixAInput.Text = string.Join("; ", matrixAstr);
                    VectorBInput.Text = string.Join(", ", result.b);
                    break;
                case Method.GaussSeide:
                    var resultG = Solver.GaussSeideInitialization(matrixA, vectorB, eps, x_0, maxIt);
                    Result.Content = string.Join("; ", resultG);
                    var matrixAstrG = new List<string>();
                    for (int i = 0; i < resultG.A.GetLength(0); i++)
                    {
                        var row = new double[resultG.A.GetLength(1)];
                        for (int j = 0; j < resultG.A.GetLength(1); j++)
                        {

                            row[j] = resultG.A[i, j];
                        }
                        matrixAstrG.Add(string.Join(", ", row));
                    }
                    MatrixAInput.Text = string.Join("; ", matrixAstrG);
                    VectorBInput.Text = string.Join(", ", resultG.b);
                    break;
                case Method.Relaxation:
                    var omega = OmegaInput.Text;
                    var resultR = Solver.RelaxationInitialization(matrixA, vectorB, eps, x_0, maxIt, omega);
                    Result.Content = string.Join("; ", resultR);
                    var matrixAstrR = new List<string>();
                    for (int i = 0; i < resultR.A.GetLength(0); i++)
                    {
                        var row = new double[resultR.A.GetLength(1)];
                        for (int j = 0; j < resultR.A.GetLength(1); j++)
                        {

                            row[j] = resultR.A[i, j];
                        }
                        matrixAstrR.Add(string.Join(", ", row));
                    }
                    MatrixAInput.Text = string.Join("; ", matrixAstrR);
                    VectorBInput.Text = string.Join(", ", resultR.b);
                    break;
                case Method.Newton:
                    var f = MatrixAInput.Text;
                    var fPrime = VectorBInput.Text;               
                    Result.Content = Solver.SolveNewton(f, fPrime, x_0, eps, maxIt);
                    break;
                case Method.SimpleIteration:
                    var g = MatrixAInput.Text;
                    Result.Content = Solver.SolveSimpleIteration(g, x_0, eps, maxIt);
                    break;
            }
            if (CurrentTask != null && CurrentTask.Result == Result.Content.ToString())
            {
                MessageBox.Show(
                    $"Вы выполнили задание \"{CurrentTask.Name}\"",
                    "Поздравляем!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);

                TasksList.UnselectAll();
                TasksList.SelectedItem = null;
                CurrentTask = null;
            }
            else if (CurrentTask != null)
            {
                MessageBox.Show(
                    "Попробуйте ещё раз!",
                    "Неправильно!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MethodChoose.ToolTip = "Выберите один из 5-ти методов. 2 из которых предназачены для решения нелинейных уравнений и остальные для решения систем линейнх алгебраических уравнений. ";
            QuestChoose.ToolTip = "Выберите одно из представленных ниже заданий. В каждом здании используется разный метод. ";
            LableResult.ToolTip = "После нажатия кнопки 'Решить' ниже будет выведен результат расчёта по выбранному методу и начальным данным. В самом результате будет корень уравнения 'X' и число последнего итерационного шага.  ";
            Input3.ToolTip = "Введите желаемую точность до которой будет сходится метод. Можно записать ввиде: 1e-6. ";
            Input4.ToolTip = "Введите начальное приближение (для НУ) или вектор начального приближения (для СЛАУ). Вектор записывается разделяя переменные символом ','. ";
            Input5.ToolTip = "Введите желаемое максимальное количество итераций. Чем число меньше - тем менее точнно сойдётся метод, но система не будет сильно нагружаться. И наоборот. ";
            Input6.Visibility = Visibility.Hidden;
            OmegaInput.Visibility = Visibility.Hidden;
            Input2.Visibility = Visibility.Visible;
            VectorBInput.Visibility = Visibility.Visible;
            switch (MethodCombo.SelectedIndex)
            {
                case 0:
                    CurrentMethod = Method.Jacobi;
                    Input1.Content = "Введите матрицу A";
                    Input2.Content = "Введите вектор B";
                    MatrixAInput.Text = "1, 3, 4; 2, 7, -1; -1, 5, 0";
                    VectorBInput.Text = "14, 5, 1";
                    X_0Input.Text = "1, 2, 3";
                    Input1.ToolTip = "Введите значения  матрицы А, разделяя переменные в строках символом ',', а сами строки символом ';'. ";
                    Input2.ToolTip = "Введите значения вектора B, разделяя переменные символом ','. ";
                    

                    break;
                case 1:
                    CurrentMethod = Method.Newton;
                    Input1.Content = "Введите выражение";
                    Input2.Content = "Введите производную";
                    MatrixAInput.Text = "x*x-1";
                    VectorBInput.Text = "2*x";
                    X_0Input.Text = "1";
                    Input1.ToolTip = "Введите выражение F(x). Применимы формы записи из класса Math. ";
                    Input2.ToolTip = "Введите производную F'(x). Применимы формы записи из класса Math. ";
                    break;
                case 2:
                    CurrentMethod = Method.GaussSeide;
                    Input1.Content = "Введите матрицу A";
                    Input2.Content = "Введите вектор B";
                    MatrixAInput.Text = "1, 3, 4; 2, 7, -1; -1, 5, 0";
                    VectorBInput.Text = "14, 5, 1";
                    X_0Input.Text = "1, 2, 3";
                    Input1.ToolTip = "Введите значения  матрицы А, разделяя переменные в строках символом ',', а сами строки символом ';'. ";
                    Input2.ToolTip = "Введите значения вектора B, разделяя переменные символом ','. ";
                    break;
                case 3:
                    CurrentMethod = Method.Relaxation;
                    Input1.Content = "Введите матрицу A";
                    Input2.Content = "Введите вектор B";
                    MatrixAInput.Text = "1, 3, 4; 2, 7, -1; -1, 5, 0";
                    VectorBInput.Text = "14, 5, 1";
                    X_0Input.Text = "1, 2, 3";
                    Input1.ToolTip = "Введите значения  матрицы А, разделяя переменные в строках символом ',', а сами строки символом ';'. ";
                    Input2.ToolTip = "Введите значения вектора B, разделяя переменные символом ','. ";
                    Input6.ToolTip = "Введите параметр omega в доступном диапазоне от 0 до 2. (0 < omega < 2). ";
                    OmegaInput.Text = "0.7";
                    Input6.Visibility = Visibility.Visible;
                    OmegaInput.Visibility = Visibility.Visible;
                    break;
                case 4:
                    CurrentMethod = Method.SimpleIteration;
                    Input1.Content = "Введите выражение";
                    Input2.Visibility = Visibility.Hidden;
                    VectorBInput.Visibility = Visibility.Hidden;
                    MatrixAInput.Text = "Sqrt(-2*x+4)";
                    X_0Input.Text = "1";
                    Input1.Content = "Ввердите преобразование g(x)";
                    Input1.ToolTip = "Ввердите преобразование g(x) от выражения F(x). Применимы формы записи из класса Math. ";
                    break;
                default:
                    CurrentMethod = Method.Jacobi;
                    Input1.Content = "Введите матрицу A";
                    Input2.Content = "Введите вектор B";
                    MatrixAInput.Text = "1, 3, 4; 2, 7, -1; -1, 5, 0";
                    VectorBInput.Text = "14, 5, 1";
                    X_0Input.Text = "1, 2, 3";
                    Input1.ToolTip = "Введите значения  матрицы А, разделяя переменные в строках символом ',', а сами строки символом ';'. ";
                    Input2.ToolTip = "Введите значения вектора B, разделяя переменные символом ','. ";
                    break;
            }
        }

        private void TasksList_TaskClicked(object sender, MouseButtonEventArgs e)
        {
            var selectedTask = Tasks[TasksList.SelectedIndex];
            if (CurrentTask != selectedTask)
            {
                CurrentTask = selectedTask;
                CurrentMethod = CurrentTask.Method;
                MethodCombo.SelectedIndex = (int)CurrentMethod;
                MatrixAInput.Text = CurrentTask.Input1;
                VectorBInput.Text = CurrentTask.Input2;
                EpsInput.Text = CurrentTask.Eps;
                X_0Input.Text = CurrentTask.X0;
                MaxIterInput.Text = CurrentTask.MaxIt;

                MessageBox.Show(
                    $"Описание: {CurrentTask.Description}\n" +
                    $"Ожидаемый результат: {CurrentTask.Result}.\n" +
                    $"Для отмены выполнения задания нажмите на него ещё раз.",
                    CurrentTask.Name,
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);

                if (CurrentTask.Omega != null)
                    OmegaInput.Text = CurrentTask.Omega;
            }
            else
            {
                TasksList.UnselectAll();
                TasksList.SelectedItem = null;
                CurrentTask = null;
            }
        }
    }
}
