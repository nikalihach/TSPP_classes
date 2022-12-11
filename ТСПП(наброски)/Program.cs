using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp2
{
    abstract public class general_information
    {
        public string name;
        public string address;

        public string removing_extra_spaces(string str)
        {
            var newstr = string.Join(" ", str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()));
            return newstr;
        }
        public string fix_the_registry(string str)
        {
            string[] newstr = str.Split();
            for (int i = 0; i < newstr.Length; i++)
            {
                newstr[i] = char.ToUpper(newstr[i][0]) + newstr[i].Substring(1).ToLower();
            }
            str = string.Join(" ", newstr);
            Console.WriteLine(str);
            return str;
        }
        public string return_address()
        {
            return address;
        }
    }
    public class Provider : general_information
    {
        public Provider(String name_provider, String address_provider, uint supply, float cost) //Постачальник
        {
            name_provider = removing_extra_spaces(name_provider);
            address_provider = removing_extra_spaces(address_provider);
            name_provider = fix_the_registry(name_provider);
            address_provider = fix_the_registry(address_provider);
            this.name = name_provider;
            this.address = address_provider;
            this.supply = supply;
            this.cost = cost;
        }

        public uint supply; //Запаси
        public float cost; //Вартість товару

        ~Provider()
        {
        }
    }

    public class Client : general_information //Клієнт
    {
        public Client(String name_client, string address_client, uint order)
        {
            name_client = removing_extra_spaces(name_client);
            address_client = removing_extra_spaces(address_client);
            name_client = fix_the_registry(name_client);
            address_client = fix_the_registry(address_client);
            this.name = name_client;
            this.order = order;
            this.address = address_client;
        }

        public uint order; //потреби споживача

        ~Client()
        {
        }
    }

    public class Expenses //витрати
    {
        public float[,] fuel_spending; //витрати на топливо
        public float[] product_costs; //витрати на товар
        private float[,] general_expenses; //Загальна сума

        public float[,] Return_GeneralExpenses()
        {
            general_expenses = new float[fuel_spending.GetLength(0), fuel_spending.GetLength(1)];
            for (int i = 0; i < fuel_spending.GetLength(0); i++)
                for (int j = 0; j < general_expenses.GetLength(1); j++)
                {
                    general_expenses[i, j] = fuel_spending[i, j] + product_costs[i];
                }

            return general_expenses;
        }

        ~Expenses()
        {
        }
    }

    public class OptimalPlan //опорний план
    {
        private float[,] cost; //масив з витратами
        private float[,] plan; //опорний план
        private uint[] stock; //масив запасів
        private uint[] order; //масив попиту 

        private int non_zero; //кількість не-нульових елементів масиву plan

        private int[] U; //Масив потенціалів u
        private int[] V; //Масив потенціалів v

        int index_i_zero;
        int index_j_zero;

        public bool plan_optimality;

        public OptimalPlan(float[,] _cost, uint[] _stock, uint[] _needs)
        {
            cost = _cost;
            stock = _stock;
            order = _needs;
            Initialization();
        }
        private void Initialization()
        {
            plan = new float[cost.GetLength(0), cost.GetLength(1)];
            Array.Copy(cost, plan, cost.Length);

            U = new int[stock.Length]; //Масив потенціалів u
            V = new int[order.Length]; //Масив потенціалів v
        }

        public void Start_calculating()
        {
            Metod_MinimalElement();
            FindingPotentials();

        }

        private void Metod_MinimalElement()
        {
            uint[] stock_remnant; //залишки запасів
            stock_remnant = new uint[stock.Length];
            uint[] needs_remnant; //залишки попиту
            needs_remnant = new uint[order.Length];
            Array.Copy(stock, stock_remnant, stock.Length);
            Array.Copy(order, needs_remnant, order.Length);

            float[,] auxiliary; //допоміжний масив
            auxiliary = new float[cost.GetLength(0), cost.GetLength(1)];
            Array.Copy(cost, auxiliary, cost.Length);

            int condition; //умова невиродженності
            non_zero = 0; //кількість базисних клітинок

            float min = 9999999; //мінімальний елемент
            int index1 = 0, index2 = 0; //змінні для збереження індексу

            for (int k = 0; k < cost.Length; k++)
            {
                for (int i = 0; i < stock.Length; i++)
                {
                    for (int j = 0; j < order.Length; j++)
                    {
                        if (auxiliary[i, j] < min)
                        {
                            min = auxiliary[i, j];
                            index1 = i;
                            index2 = j;
                        }
                    }
                }

                if (stock_remnant[index1] >= needs_remnant[index2]) //якщо запасів більше ніж потрібно замовнику, то
                {
                    plan[index1, index2] = needs_remnant[index2]; //в план записується попит

                    if (plan[index1, index2] != 0)
                    {
                        non_zero++;
                    }

                    stock_remnant[index1] = stock_remnant[index1] - needs_remnant[index2]; //розрахунок залишку запасів на складі
                    needs_remnant[index2] = 0;
                }

                else if (stock_remnant[index1] <= needs_remnant[index2])
                {
                    plan[index1, index2] = stock_remnant[index1];

                    if (plan[index1, index2] != 0)
                    {
                        non_zero++;
                    }

                    needs_remnant[index2] = needs_remnant[index2] - stock_remnant[index1]; //розрахунок залишку потреб замовника
                    stock_remnant[index1] = 0;
                }

                auxiliary[index1, index2] = 88888;
                min = 9999999;
            }

            condition = stock.Length + order.Length - 1;

            if (condition == non_zero)//перевірка на невиродженість опорного плану
            {
                Console.WriteLine("GOOD");
            }

            else
            {
                float minimal = 999999;
                index_i_zero = 0;
                index_j_zero = 0;

                for (int i = 0; i < stock.Length; i++)
                {
                    for (int j = 0; j < order.Length; j++)
                    {
                        if (plan[i, j] == 0)
                        {
                            if (cost[i, j] < minimal)
                            {
                                minimal = cost[i, j];
                                index_i_zero = i;
                                index_j_zero = j;
                            }

                        }
                    }
                }

                plan[index_i_zero, index_j_zero] = -1;
                non_zero = non_zero + 1;
            }

        }

        static double[] gauss(double[,] a, double[] y, int n)
        {
            double[] x;
            double max;
            int k, index;
            const double eps = 0.00001;  // точность
            x = new double[n];
            k = 0;
            while (k < n)
            {
                // Пошук рядку з максимальним a[i][k]
                max = Math.Abs(a[k, k]);
                index = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(a[i, k]) > max)
                    {
                        max = Math.Abs(a[i, k]);
                        index = i;
                    }
                }
                // Перестановка рядків
                if (max < eps)
                {
                    double[] error = new double[1] { 0 };
                    // нет ненулевых диагональных элементов
                    return error;
                }
                double temp;
                for (int j = 0; j < n; j++)
                {
                    temp = a[k, j];
                    a[k, j] = a[index, j];
                    a[index, j] = temp;
                }
                temp = y[k];
                y[k] = y[index];
                y[index] = temp;
                // Нормализация уравнений
                for (int i = k; i < n; i++)
                {
                    temp = a[i, k];
                    if (Math.Abs(temp) < eps) continue; // для нулевого коэффициента пропустить
                    for (int j = 0; j < n; j++)
                        a[i, j] = a[i, j] / temp;
                    y[i] = y[i] / temp;
                    if (i == k) continue; // уравнение не вычитать само из себя
                    for (int j = 0; j < n; j++)
                        a[i, j] = a[i, j] - a[k, j];
                    y[i] = y[i] - y[k];
                }
                k++;
            }
            // обратная подстановка
            for (k = n - 1; k >= 0; k--)
            {
                x[k] = y[k];
                for (int i = 0; i < k; i++)
                    y[i] = y[i] - a[i, k] * x[k];
            }
            return x;
        }

        //PRIVATE
        private void FindingPotentials()
        {
            int[] u_index = new int[non_zero]; //Масив для збереження індексів на перетині не-нульового елементу масива (u - А)
            int[] v_index = new int[non_zero]; //Масив для збереження індексів на перетині не-нульового елементу масива (v - В)

            int[] v_index_correction = new int[non_zero];

        recalc:
            int k = 0;

            for (int i = 0; i < stock.Length; i++)
            {
                for (int j = 0; j < order.Length; j++)
                {
                    if (plan[i, j] != 0)
                    {
                        u_index[k] = i;
                        v_index[k] = j;
                        k++;
                    }
                }
            }

            int coincidence = 0;

            for (int p = 1; p < non_zero; p++)
            {
                if (u_index[0] == u_index[p] || v_index[0] == v_index[p])
                {
                    coincidence++;
                }
            }

            if (coincidence > 0)
            {
                int[,] index = new int[non_zero + 1, 2];
                double[,] a = new double[non_zero + 1, non_zero + 1];
                double[] y = new double[non_zero + 1];
                y[non_zero] = 0;
                int count = 0;

                for (int i = 0; i < plan.GetLength(0); i++)
                {
                    for (int j = 0; j < plan.GetLength(1); j++)
                    {
                        if (plan[i, j] != 0)
                        {
                            y[count] = cost[i, j];
                            count++;
                        }
                    }
                }

                for (int i = 0; i < non_zero; i++)
                {
                    v_index_correction[i] = v_index[i] + stock.Length;
                }

                for (int i = 0; i < non_zero; i++)
                {
                    index[i, 0] = u_index[i];
                    index[i, 1] = v_index_correction[i];
                }

                for (int i = 0; i < non_zero; i++)
                {

                    a[i, index[i, 0]] = 1;
                    a[i, index[i, 1]] = 1;
                }

                a[non_zero, 0] = 1;

                for (int i = 1; i < non_zero + 1; i++)
                {
                    a[non_zero, i] = 0;
                }

                double[] potentials = new double[non_zero + 1];

                potentials = gauss(a, y, non_zero + 1);

                for (int i = 0; i < stock.Length; i++)
                {
                    U[i] = (int)potentials[i];
                }

                count = 0;
                for (int i = stock.Length; i < potentials.Length; i++)
                {
                    V[count] = (int)potentials[i];
                    count++;
                }

            }

            else
            {
                plan[index_i_zero, index_j_zero] = 0; //прибираємо псевдоноль 

                float minimal = 999999;

                for (int j = 0; j < order.Length; j++)
                {
                    if (plan[0, j] == 0)
                    {
                        if (cost[0, j] < minimal)
                        {
                            minimal = cost[0, j];
                            index_i_zero = 0;
                            index_j_zero = j;
                        }
                    }
                }

                plan[index_i_zero, index_j_zero] = -1; //розміщаємо "нульовy" підставку
                goto recalc;
            }


            for (int i = 0; i < V.Length; i++)
            {
                Console.WriteLine(" V[" + i + "]=" + V[i]);
            }

            for (int i = 0; i < U.Length; i++)
            {
                Console.WriteLine(" U[" + i + "]=" + U[i]);
            }


            CalculateGrades();
        }

        private void CalculateGrades()
        {
            int[] grades;


            int stop = 0;
            int index_i = 0, index_j = 0;
            int count = 0;
            int null_cells = 0;

            for (int i = 0; i < stock.Length; i++)
            {
                for (int j = 0; j < order.Length; j++)
                {
                    if (plan[i, j] == 0)
                    {
                        null_cells++;
                    }
                }
            }

            grades = new int[null_cells];

            for (int i = 0; i < stock.Length; i++)
            {
                for (int j = 0; j < order.Length; j++)
                {
                    if (plan[i, j] == 0)
                    {
                        grades[count] = (int)cost[i, j] - (U[i] + V[j]);
                        Console.WriteLine((int)cost[i, j] + "- (" + U[i] + "+" + V[j] + ") = " + grades[count]);


                        if (grades[count] < 0)
                        {
                            stop = 1;
                            index_i = i;
                            index_j = j;
                        }
                        count++;
                    }
                }
            }

            if (stop != 1)
            {
                Console.WriteLine("The plan is optimal");
                plan_optimality = true;
            }
            else
            {
                Console.WriteLine("The plan is not optimal");
                plan_optimality = false;
                Recalculation(index_i, index_j);
            }
        }

        private void Recalculation(int i, int j)
        {
            float[,] recalculation = new float[cost.GetLength(0), cost.GetLength(1)];
           
            int counter = 1;
            int[] current_index = new int[2];
            int[] old_index = new int[2];
            int[] deadlock = new int[2] { 666, 666 };
            recalculation[i, j] = 1;
            current_index[0] = i;
            current_index[1] = j;
            int stop_minus;
            int stop_plus;
            int finish = 0;

            while (true)
            {
                if (counter % 2 != 0)
                {
                    stop_minus = 0;

                    for (int horizon = 0; horizon < order.Length; horizon++)
                    {
                        if (horizon != current_index[1] && plan[current_index[0], horizon] != 0 && plan[current_index[0], horizon] != -1 && horizon != deadlock[1])
                        {
                            recalculation[current_index[0], horizon] = -1;
                            old_index[1] = current_index[1];
                            current_index[1] = horizon;
                            break;
                        }

                        else
                        {
                            stop_minus++;

                            if (stop_minus > order.Length - 1)
                            {
                                recalculation[current_index[0], current_index[1]] = 0;
                                deadlock[0] = current_index[0];
                                current_index[0] = old_index[0];
                                stop_minus = 0;
                                break;
                            }
                        }
                    }
                }


                else if (counter % 2 == 0)
                {
                    stop_plus = 0;
                    for (int vertical = 0; vertical < stock.Length; vertical++)
                    {
                        if (current_index[1] == j && vertical == i && counter > 1)
                        {
                            finish = 1;
                            break;
                        }


                        if (vertical != current_index[0] && plan[vertical, current_index[1]] != 0 && plan[vertical, current_index[1]] != -1 && vertical != deadlock[0])
                        {
                            recalculation[vertical, current_index[1]] = 1;
                            old_index[0] = current_index[0];
                            current_index[0] = vertical;
                            break;
                        }

                        else
                        {
                            stop_plus++;

                            if (stop_plus > stock.Length - 1)
                            {
                                recalculation[current_index[0], current_index[1]] = 0;
                                deadlock[1] = current_index[1];
                                current_index[1] = old_index[1];
                                stop_plus = 0;
                                break;
                            }
                        }
                    }
                }

                counter++;

                if (finish == 1) break;
            }

            float min = 99999999;
            for (int k = 0; k < stock.Length; k++)
            {
                for (int p = 0; p < order.Length; p++)
                {
                    if (recalculation[k, p] == -1 && recalculation[k, p] < min)
                    {
                        min = plan[k, p];
                    }
                }
            }

            for (int k = 0; k < plan.GetLength(0); k++)
            {
                for (int p = 0; p < plan.GetLength(1); p++)
                {

                    if (recalculation[k, p] == -1)
                    {
                        plan[k, p] = plan[k, p] - min;
                    }

                    if (recalculation[k, p] == 1)
                    {
                       plan[k, p] = plan[k, p] + min;
                    }
                }
            }

            FindingPotentials();
        }


    }


    class Report
    {
        String report_text;
        String FileName;
        private float[,] optimal_plan;

        string[] provider_addresses;
        string[] clien_addresses;

        public void get_data_for_report(float[,] plan, string[] provider_addresses_, string[] clien_addresses_)
        {
            optimal_plan = new float[plan.GetLength(0), plan.GetLength(1)];
            provider_addresses = new string[provider_addresses_.Length];
            clien_addresses = new string[clien_addresses_.Length];

            Array.Copy(plan, optimal_plan, plan.Length);
            Array.Copy(provider_addresses_, provider_addresses, provider_addresses.Length);
            Array.Copy(clien_addresses_, clien_addresses, clien_addresses.Length);
            Fill_out_a_report();
        }

        private void Fill_out_a_report()
        {
            for (int i = 0; i < optimal_plan.GetLength(0); i++)
            {
                for (int j = 0; j < optimal_plan.GetLength(1); j++)
                {
                    if (optimal_plan[i, j] != 0)
                    {
                        report_text = report_text + "З адреси " + provider_addresses[i] + " треба направити " + optimal_plan[i, j] + " одиниць вантажу до споживача за адресою " + clien_addresses[j] + ". \n";
                    }
                }
            }

            Console.WriteLine(report_text);
        }

        private void Filename_confirmation(String name)
        {
            FileName = name + ".txt";
        }

        public bool Save_report(string name)
        {
            int error = 0;
            Filename_confirmation(name);
            StreamWriter SW = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write));
            try
            {
                SW.Write(report_text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                error = 1;
            }
            finally
            {
                SW.Close();
            }

            if (error == 1)
            {
                return false;
            }

            else
            {
                return true;
            }

        }

    }



    class Program
    {
        static void Main(string[] args)
        {
            /*
            var A1 = new Provider("A1", "h1", 20, 1);
            var A2 = new Provider("A2", "h2", 35, 4);
            var A3 = new Provider("A3", "h3", 30, 2);
            
            var B1 = new Client("B1", "g1", 10);
            var B2 = new Client("B2", "g2", 15);
            var B3 = new Client("B3", "g3", 25);
            var B4 = new Client("B4", "g4", 30);

            var E = new Expenses();
            E.fuel_spending = new float[3, 4] { {10,3,2,0 }, { 2,4,5,3},{ 2,6,2,0 } };
            E.product_costs = new float[3] { A1.cost, A2.cost, A3.cost};

            uint[] stock = { A1.supply, A2.supply, A3.supply };
            uint[] needs = { B1.order, B2.order, B3.order, B4.order };

            //var sw = new Stopwatch();
           // sw.Start();
            var OP = new OptimalPlan(E.Return_GeneralExpenses(), stock, needs);
            OP.Start_calculating();

            float[,] plan = new float[3, 4];
            plan = OP.Metod_MinimalElement();

            for( int i =0; i< plan.GetLength(0); i++ )
                for( int j =0; j< plan.GetLength(1); j++ )
            Console.WriteLine(plan[i,j]);


            // sw.Stop();
            Console.WriteLine();
            //Console.WriteLine(sw.Elapsed);
            var report = new Report();
            //  report.Save("REPORT");
            
            Console.ReadLine();
            */

            uint[] stock = { 20, 35, 30 };
            uint[] needs = { 10, 15, 25, 30 };
            float[,] general_expenses = new float[3, 4] { { 11, 4, 3, 1 }, { 6, 8, 9, 7 }, { 4, 8, 4, 2 } };
            var OP = new OptimalPlan(general_expenses, stock, needs);
            OP.Start_calculating();


            Console.ReadLine();
        }
    }
}