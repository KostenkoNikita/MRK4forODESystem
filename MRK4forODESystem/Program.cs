﻿#pragma warning disable

using System;
//Пространство имён для коллекций
using System.Collections.Generic;
//Пространство имён для ввода-вывода
using System.IO;
//Пространство имён для работы с текстом
using System.Text;
//Пространство имён для выполнения запросов LINQ (Select, Zip)
using System.Linq;

using System.Windows.Forms;

namespace MRK4forODESystem
{
    class Program
    {
        const double g = 9.809;

        const double m = 5;

        const double a1 = 1;

        const double H0 = 3.0 / (2 * Math.PI);

        const double lambda = 2;

        const double k = 2 * Math.PI / lambda;

        static double kappa
        {
            get
            {
                return -1.0 / m;
            }
        }

        /// <summary>
        /// Список значений времени
        /// </summary>
        static List<double> t;

        /// <summary>
        /// Список значений x1
        /// </summary>
        static List<double> x1;

        /// <summary>
        /// Список значений x2
        /// </summary>
        static List<double> x2;

        /// <summary>
        /// Список значений x3
        /// </summary>
        static List<double> x3;

        /// <summary>
        /// Список значений x4
        /// </summary>
        static List<double> x4;

        /// <summary>
        /// Список значений x5
        /// </summary>
        static List<double> x5;

        /// <summary>
        /// Список значений beta (коэффициент разделения лучей)
        /// </summary>
        static List<double> beta;

        /// <summary>
        /// Список значений Kp (коэффициент рефракции)
        /// </summary>
        static List<double> Kp;

        /// <summary>
        /// Список значений Kт (коэффициент трансформации)
        /// </summary>
        static List<double> Kt;

        /// <summary>
        /// Список значений H (глубина)
        /// </summary>
        static List<double> HList;

        /// <summary>
        /// Список произведений коэффициентов трансормации на коэффициенты рефракции
        /// </summary>
        static List<double> KtKp;

        /// <summary>
        /// Массив (должен содержать пять элементов) коэффициентов МРК4
        /// </summary>
        static double[] k1;

        /// <summary>
        /// Массив (должен содержать пять элементов) коэффициентов МРК4
        /// </summary>
        static double[] k2;

        /// <summary>
        /// Массив (должен содержать пять элементов) коэффициентов МРК4
        /// </summary>
        static double[] k3;

        /// <summary>
        /// Массив (должен содержать пять элементов) коэффициентов МРК4
        /// </summary>
        static double[] k4;

        /// <summary>
        /// Номер шага в методе Рунге-Кутты (считается по количеству элементов в 
        /// списке для времени t)
        /// </summary>
        static int i
        {
            get
            {
                return t == null ? 0 : t.Count - 1;
            }
        }

        /// <summary>
        /// H на бесконечности (формула 2.10)
        /// </summary>
        static double Hinf
        {
            get
            {
                try
                {
                    return H(x1.ElementAt(0));
                }
                catch (ArgumentNullException)
                {
                    MessageBox.Show("Error: x1 list has not initialized. Program will be terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return 0;
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Error: x1 list doesn't contains the first element. Program will be terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return 0;
                }
                catch
                {
                    MessageBox.Show("An error occured. Program will be terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return 0;
                }
            }
        }

        /// <summary>
        /// Знаменатель под корнем в формуле 2.10
        /// </summary>
        static double KtNumenator
        {
            get
            {
                try
                {
                    return Math.Tanh(k * Hinf) * (1 + 2 * k * Hinf / (Math.Sinh(2 * k * Hinf)));
                }
                catch
                {
                    MessageBox.Show("Inner exception in Hinf calculating. Program will be terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return 0;
                }
            }
        }

        /// <summary>
        /// Шаг по времени в методе Рунге-Кутты
        /// </summary>
        static double h;

        /// <summary>
        /// Точка входа в приложение
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        static void Main(string[] args)
        {
            //Создание объекта для вычисление времени работы программы
            System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Starting chronometer");
            chrono.Start();
            //Шаг по времени
            h = 0.1;
            //Максимальное время
            double tMax = 5;

            //Инициализация списков для результатов
            //По сути это тот же массив, который при добавлении 
            //Элементов сам расширяется
            t = new List<double>();
            x1 = new List<double>();
            x2 = new List<double>();
            x3 = new List<double>();
            x4 = new List<double>();
            x5 = new List<double>();

            //Начальные условия
            t.Add(0);
            x1.Add(0);
            x2.Add(0);
            x3.Add(0.5 * Math.PI / a1);
            x4.Add(1);
            x5.Add(0);

            //Создание массивов для коэффициентов в МРК4
            //Их количество равно количеству уравнений (первого порядка)
            //в системе
            k1 = new double[5];
            k2 = new double[5];
            k3 = new double[5];
            k4 = new double[5];

            Console.WriteLine("Starting Runge–Kutta method for ODE's system");
            do
            {
                //Вычисление коэффициентов
                IntermediateCalculations();
                //Внесение их в списки
                x1.Add(x1[i] + (1.0 / 6.0) * h * (k1[0] + 2 * k2[0] + 2 * k3[0] + k4[0]));
                x2.Add(x2[i] + (1.0 / 6.0) * h * (k1[1] + 2 * k2[1] + 2 * k3[1] + k4[1]));
                x3.Add(x3[i] + (1.0 / 6.0) * h * (k1[2] + 2 * k2[2] + 2 * k3[2] + k4[2]));
                x4.Add(x4[i] + (1.0 / 6.0) * h * (k1[3] + 2 * k2[3] + 2 * k3[3] + k4[3]));
                x5.Add(x5[i] + (1.0 / 6.0) * h * (k1[4] + 2 * k2[4] + 2 * k3[4] + k4[4]));
                //К времени просто добавляется величина шага по времени
                t.Add(t[i] + h);
            } while (t[i] <= tMax-h);
            //Заполнение списка для beta
            beta = x4.Select(d => d).ToList();
            //Заполнение списка для Кр
            Kp = beta.Select(b => 1.0 / Math.Sqrt(b)).ToList();
            //Заполнение списка для глубины
            HList = x1.Select(x => kappa * x + H0).ToList();
            //Заполнение списка для Кт
            Kt = HList.Select(h => Math.Sqrt(KtNumenator/(Math.Tanh(k * h) * (1 + 2 * k * h / (Math.Sinh(2 * k * h)))))).ToList();
            //Заполнение списков произведний Кт*Кр
            KtKp = Kt.Zip(Kp, (kt, kr) => { return kt * kr; }).ToList();

            Console.WriteLine("Runge–Kutta method for ODE's system has been completed");
            Console.WriteLine("Writing to file result.txt...");
            //Объявление объекта, выполняющего запись в текстовый файл
            StreamWriter sw = null;
            try
            {
                //и его инициализация. Файл result.txt будет лежать в той же папке, что и приложение
                //второй параметр означает, что файл будет перезаписан или создан при его отсутствии
                //третий параметр указывает кодировку (если использовать только английскую раскладку клавиатуры,
                //то ASCII хватит)
                sw = new StreamWriter("result.txt", false, Encoding.ASCII);
                WriteToFile(sw, t);
                WriteToFile(sw, x1);
                WriteToFile(sw, x2);
                WriteToFile(sw, x3);
                WriteToFile(sw, x4);
                WriteToFile(sw, x5);
                WriteToFile(sw, beta);
                WriteToFile(sw, HList);
                WriteToFile(sw, Kp);
                WriteToFile(sw, Kt);
                WriteToFile(sw, KtKp);
                //После записи открываем файл result.txt
                System.Diagnostics.Process.Start("result.txt");
            }
            //При возникновении ошибки уведомляем об этом
            catch (Exception e)
            {
                MessageBox.Show("An error occured. Program will be terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            //В любом случае закрываем поток для записи в файл
            //и останавливаем хронометр
            finally
            {
                chrono.Stop();
                sw.Dispose();
                Console.WriteLine("Writing to file has been completed");
                Console.WriteLine("Elapsed seconds: {0:G} s", chrono.Elapsed.TotalSeconds);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Метод для записи в файл значений из коллекции
        /// </summary>
        /// <param name="sw">Ссылка на объект для записи</param>
        /// <param name="res">Ссылка на записываемую в файл коллекцию</param>
        static void WriteToFile(StreamWriter sw, List<double> res)
        {
            //Создание объекта для построения строки
            StringBuilder sb = new StringBuilder();
            string resultName=string.Empty;
            if (res.Equals(t))
            {
                resultName = "t";
            }
            else if (res.Equals(x1))
            {
                resultName = "x1";
            }
            else if (res.Equals(x2))
            {
                resultName = "x2";
            }
            else if (res.Equals(x3))
            {
                resultName = "x3";
            }
            else if (res.Equals(x4))
            {
                resultName = "x4";
            }
            else if (res.Equals(x5))
            {
                resultName = "x5";
            }
            else if (res.Equals(beta))
            {
                resultName = "beta";
            }
            else if (res.Equals(Kp))
            {
                resultName = "Kp";
            }
            else if (res.Equals(Kt))
            {
                resultName = "Kt";
            }
            else if (res.Equals(KtKp))
            {
                resultName = "Kt*Kp";
            }
            else if (res.Equals(HList))
            {
                resultName = "H";
            }
            else
            {
                //В случае передечи какого-то иного списка -- ошибка
                throw new ArgumentException();
            }
            sb.Append(resultName + " = { ");
            //Для каждого элемента переданого списка
            foreach (double d in res)
            {
                sb.Append(d.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + ", ");
            }
            sb.Append(" };");
            string sbToStr = sb.ToString();
            //Запись в файл с удалением последней запятой
            sw.WriteLine(sbToStr.Remove(sbToStr.LastIndexOf(','))+"};");
            sw.WriteLine(string.Empty);
            Console.WriteLine("Writing " + resultName + " values to result.txt has been completed");
        }

        /// <summary>
        /// Промежуточные вычисления коэффициентов в методе Рунге-Кутты
        /// </summary>
        static void IntermediateCalculations()
        {
            k1[0] = f1(t[i], x1[i], x2[i], x3[i], x4[i], x5[i]);
            k1[1] = f2(t[i], x1[i], x2[i], x3[i], x4[i], x5[i]);
            k1[2] = f3(t[i], x1[i], x2[i], x3[i], x4[i], x5[i]);
            k1[3] = f4(t[i], x1[i], x2[i], x3[i], x4[i], x5[i]);
            k1[4] = f5(t[i], x1[i], x2[i], x3[i], x4[i], x5[i]);

            k2[0] = f1(t[i] + 0.5 * h, x1[i] + 0.5 * h * k1[0], x2[i] + 0.5 * h * k1[1], x3[i] + 0.5 * h * k1[2], x4[i] + 0.5 * h * k1[3], x5[i] + 0.5 * h * k1[4]);
            k2[1] = f2(t[i] + 0.5 * h, x1[i] + 0.5 * h * k1[0], x2[i] + 0.5 * h * k1[1], x3[i] + 0.5 * h * k1[2], x4[i] + 0.5 * h * k1[3], x5[i] + 0.5 * h * k1[4]);
            k2[2] = f3(t[i] + 0.5 * h, x1[i] + 0.5 * h * k1[0], x2[i] + 0.5 * h * k1[1], x3[i] + 0.5 * h * k1[2], x4[i] + 0.5 * h * k1[3], x5[i] + 0.5 * h * k1[4]);
            k2[3] = f4(t[i] + 0.5 * h, x1[i] + 0.5 * h * k1[0], x2[i] + 0.5 * h * k1[1], x3[i] + 0.5 * h * k1[2], x4[i] + 0.5 * h * k1[3], x5[i] + 0.5 * h * k1[4]);
            k2[4] = f5(t[i] + 0.5 * h, x1[i] + 0.5 * h * k1[0], x2[i] + 0.5 * h * k1[1], x3[i] + 0.5 * h * k1[2], x4[i] + 0.5 * h * k1[3], x5[i] + 0.5 * h * k1[4]);

            k3[0] = f1(t[i] + 0.5 * h, x1[i] + 0.5 * h * k2[0], x2[i] + 0.5 * h * k2[1], x3[i] + 0.5 * h * k2[2], x4[i] + 0.5 * h * k2[3], x5[i] + 0.5 * h * k2[4]);
            k3[1] = f2(t[i] + 0.5 * h, x1[i] + 0.5 * h * k2[0], x2[i] + 0.5 * h * k2[1], x3[i] + 0.5 * h * k2[2], x4[i] + 0.5 * h * k2[3], x5[i] + 0.5 * h * k2[4]);
            k3[2] = f3(t[i] + 0.5 * h, x1[i] + 0.5 * h * k2[0], x2[i] + 0.5 * h * k2[1], x3[i] + 0.5 * h * k2[2], x4[i] + 0.5 * h * k2[3], x5[i] + 0.5 * h * k2[4]);
            k3[3] = f4(t[i] + 0.5 * h, x1[i] + 0.5 * h * k2[0], x2[i] + 0.5 * h * k2[1], x3[i] + 0.5 * h * k2[2], x4[i] + 0.5 * h * k2[3], x5[i] + 0.5 * h * k2[4]);
            k3[4] = f5(t[i] + 0.5 * h, x1[i] + 0.5 * h * k2[0], x2[i] + 0.5 * h * k2[1], x3[i] + 0.5 * h * k2[2], x4[i] + 0.5 * h * k2[3], x5[i] + 0.5 * h * k2[4]);

            k4[0] = f1(t[i] + h, x1[i] + h * k3[0], x2[i] + h * k3[1], x3[i] + h * k3[2], x4[i] + h * k3[3], x5[i] + h * k3[4]);
            k4[1] = f2(t[i] + h, x1[i] + h * k3[0], x2[i] + h * k3[1], x3[i] + h * k3[2], x4[i] + h * k3[3], x5[i] + h * k3[4]);
            k4[2] = f3(t[i] + h, x1[i] + h * k3[0], x2[i] + h * k3[1], x3[i] + h * k3[2], x4[i] + h * k3[3], x5[i] + h * k3[4]);
            k4[3] = f4(t[i] + h, x1[i] + h * k3[0], x2[i] + h * k3[1], x3[i] + h * k3[2], x4[i] + h * k3[3], x5[i] + h * k3[4]);
            k4[4] = f5(t[i] + h, x1[i] + h * k3[0], x2[i] + h * k3[1], x3[i] + h * k3[2], x4[i] + h * k3[3], x5[i] + h * k3[4]);
        }

        static double H(double x1)
        {
            return kappa * x1 + H0;
        }

        static double R(double x1)
        {
            return 2*k*H(x1)/(Math.Sinh(2*k*H(x1)));
        }

        static double c(double x1)
        {
            return Math.Sqrt(g * H(x1));
        }

        static double K1(double x1)
        {
            return (c(x1) / H(x1)) * (R(x1) / (1 + R(x1))) * kappa;
        }

        static double K4(double x1)
        {
            return -c(x1) / Math.Pow(H(x1), 2);
        }

        /// <summary>
        /// Функция Р
        /// </summary>
        static double P(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return -2 * K1(x1) * x3;
        }

        /// <summary>
        /// Функция Q
        /// </summary>
        static double Q(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return c(x1) * K4(x1) * Math.Pow(Math.Sin(x3), 2);
        }

        /// <summary>
        /// Правая часть первого уравнения системы диф.уравнений
        /// </summary>
        static double f1(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return c(x1) * Math.Cos(x3);
        }

        /// <summary>
        /// Правая часть второго уравнения системы диф.уравнений
        /// </summary>
        static double f2(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return c(x1) * Math.Sin(x3);
        }

        /// <summary>
        /// Правая часть третьего уравнения системы диф.уравнений
        /// </summary>
        static double f3(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return K1(x1) * Math.Sin(x3);
        }

        /// <summary>
        /// Правая часть четвертого уравнения системы диф.уравнений
        /// </summary>
        static double f4(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return x5;
        }

        /// <summary>
        /// Правая часть пятого уравнения системы диф.уравнений
        /// </summary>
        static double f5(double t, double x1, double x2, double x3, double x4, double x5)
        {
            return -P(t,x1,x2,x3,x4,x5) * x5 - Q(t, x1, x2, x3, x4, x5) * x4;
        }
    }
}
