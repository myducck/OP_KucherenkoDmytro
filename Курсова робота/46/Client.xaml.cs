using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Windows.Input;
using System.Data.SqlClient;

namespace _46
{
    public partial class Client : Window
    {
        string connectionStr = null;
        SqlConnection connection = null;
        SqlCommand command = null;
        SqlDataAdapter adapter = null;
        static public DataTable dt, temp;
        DataTable ft;
        static public int index = 0;
        public Client()
        {
            InitializeComponent();
            connectionStr = "Data Source = DMYTRO; Initial Catalog = Cursova; Integrated Security = True";
            Serial.Content = $"{Temporary.serial} - ваш серійний номер";
            CarsCombo.SelectedIndex = 0;
            updateFeedback();
            updateCars();
            check();
        }
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            Hide();
            mw.Show();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        public void updateFeedback()
        {
            Feedback.Columns.Clear();
            connection = new SqlConnection(connectionStr);
            connection.Open();

            string strQ = "SELECT dbo.Contract.Feedback as Відгук, dbo.Contract.Date as Дата, (dbo.Car.Price + dbo.Car.Com) as Ціна, dbo.Car.Model as Модель FROM dbo.Contract INNER JOIN dbo.Car ON dbo.Contract.IDCar = dbo.Car.IDCar WHERE dbo.Contract.Feedback != '';";
            adapter = new SqlDataAdapter(strQ, connection);
            ft = new DataTable("Відгуки");
            adapter.Fill(ft);
            Feedback.AutoGeneratedColumns += (s, e) =>
            {
                Feedback.Columns[0].Width = 600;
                (Feedback.Columns[1] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            };
            Feedback.ItemsSource = ft.DefaultView;

            connection.Close();
            index = 0;
            check();
        }
        public void updateCars()
        {
            Cars.Columns.Clear();
            connection = new SqlConnection(connectionStr);
            connection.Open();

            string strQ = "SELECT dbo.Car.IDCar as Номер, dbo.Car.Photo as Фото, dbo.Car.Model as Модель, dbo.Car.Run as Пробіг, (dbo.Car.Price + dbo.Car.Com) as Ціна, dbo.Car.Date as [Дата випуску], dbo.Dealer.Surname as [Прізвище дилера], dbo.Dealer.Name as [Ім'я дилера], dbo.Dealer.Phone as [Номер дилера] FROM dbo.Car INNER JOIN dbo.Dealer ON dbo.Car.IDDealer = dbo.Dealer.IDDealer WHERE dbo.Car.Status = 1;";
            adapter = new SqlDataAdapter(strQ, connection);
            dt = new DataTable("Наявні автомобілі");
            adapter.Fill(dt);
            Cars.AutoGeneratedColumns += (s, e) =>
            {
                Cars.Columns[1] = GetImageColumn("Фото");
                (Cars.Columns[2] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            };
            Cars.ItemsSource = dt.DefaultView;

            strQ = "SELECT dbo.Dealer.Photo, dbo.Dealer.Adress FROM dbo.Car INNER JOIN dbo.Dealer ON dbo.Car.IDDealer = dbo.Dealer.IDDealer WHERE dbo.Car.Status = 1;";
            adapter = new SqlDataAdapter(strQ, connection);
            temp = new DataTable("Додатково");
            adapter.Fill(temp);

            connection.Close();
        }
        public DataGridTemplateColumn GetImageColumn(string columnName)
        {
            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            DataTemplate cellTemplate = new DataTemplate();
            cellTemplate.VisualTree = image;
            Binding binding = new Binding(columnName) { Converter = new Dealer.ImageConvertor() };
            image.SetValue(Image.SourceProperty, binding);
            DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
            templateColumn.Header = columnName;
            templateColumn.CellTemplate = cellTemplate;
            templateColumn.Width = 150;

            return templateColumn;
        }
        private void CarsFilter()
        {
            string symbol, selPrice = "", selFind = "";
            if (CarsCombo.SelectedIndex == 0)
                symbol = ">";
            else
                symbol = "<";
            if (CarsPrice.Text != "")
                selPrice = $"AND (dbo.Car.Price + dbo.Car.Com) {symbol} {CarsPrice.Text}";
            if (Find.Text != "")
                selFind = $"AND dbo.Car.Model LIKE '{Find.Text}%'";

            Cars.Columns.Clear();
            connection = new SqlConnection(connectionStr);
            connection.Open();

            string strQ = $"SELECT dbo.Car.IDCar as Номер, dbo.Car.Photo as Фото, dbo.Car.Model as Модель, dbo.Car.Run as Пробіг, (dbo.Car.Price + dbo.Car.Com) as Ціна, dbo.Car.Date as [Дата випуску], dbo.Dealer.Surname as [Прізвище дилера], dbo.Dealer.Name as [Ім'я дилера], dbo.Dealer.Phone as [Номер дилера] FROM dbo.Car INNER JOIN dbo.Dealer ON dbo.Car.IDDealer = dbo.Dealer.IDDealer WHERE dbo.Car.Status = 1 {selFind} {selPrice};";
            adapter = new SqlDataAdapter(strQ, connection);
            dt = new DataTable("Відгуки");
            adapter.Fill(dt);
            Cars.AutoGeneratedColumns += (s, d) =>
            {
                Cars.Columns[1] = GetImageColumn("Фото");
                (Cars.Columns[5] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            };
            Cars.ItemsSource = dt.DefaultView;

            connection.Close();
            index = 0;
            check();
        }
        private void CarsClick(object sender, KeyEventArgs e)
        {
            CarsPrice.Text = Dealer.NumberCorrection(CarsPrice.Text);
            CarsFilter();
        }
        private void CarsClick(object sender, SelectionChangedEventArgs e)
        {
            CarsFilter();
        }
        string selTime = "";
        private void FeedbackFilter()
        {
            string[] prev = DatePrev.ToString().Split('/');
            string[] next = DateNext.ToString().Split('/');
            string selFind = "";

            if (FeedbackFind.Text != "") 
                selFind = $"AND dbo.Car.Model LIKE '{FeedbackFind.Text}%'";

            if (prev.Length == 3 && next.Length == 3)
            {
                prev[2] = prev[2].Substring(0, 4);
                next[2] = next[2].Substring(0, 4);
                if (int.Parse(prev[2]) > int.Parse(next[2]))
                    (DatePrev, DateNext) = (DateNext, DatePrev);
                else if(int.Parse(prev[2]) == int.Parse(next[2]))
                {
                    if (int.Parse(prev[1]) > int.Parse(next[1]))
                        (DatePrev, DateNext) = (DateNext, DatePrev);
                    else if (int.Parse(prev[1]) == int.Parse(next[1]))
                    {
                        DatePrev = DateNext = null;
                        MessageBox.Show("Оберіть різні межі");
                    }
                }
                selTime = $"AND DATEDIFF(month, '{DatePrev}', dbo.Contract.Date) > 0 AND DATEDIFF(month, dbo.Contract.Date, '{DateNext}')> 0";
            }
            else if (prev.Length == 3)
            {
                selTime = $"AND DATEDIFF(month, '{DatePrev}', dbo.Contract.Date) > 0";
            }
            else if (next.Length == 3)
            {
                selTime = $"AND DATEDIFF(month, dbo.Contract.Date, '{DateNext}') > 0";
            }

            Feedback.Columns.Clear();
            connection = new SqlConnection(connectionStr);
            connection.Open();

            string strQ = $"SELECT dbo.Contract.Feedback as Відгук, dbo.Contract.Date as Дата, (dbo.Car.Price + dbo.Car.Com) as Ціна, dbo.Car.Model as Модель FROM dbo.Contract INNER JOIN dbo.Car ON dbo.Contract.IDCar = dbo.Car.IDCar WHERE dbo.Contract.Feedback != '' {selFind} {selTime};";
            adapter = new SqlDataAdapter(strQ, connection);
            dt = new DataTable("Відгуки");
            adapter.Fill(dt);
            Feedback.AutoGeneratedColumns += (s, e) =>
            {
                (Feedback.Columns[1] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            };
            Feedback.ItemsSource = dt.DefaultView;

            connection.Close();
        }
        private void FeedbackClick_Click(object sender, RoutedEventArgs e)
        {
            FeedbackFilter();
        }
        private void FeedbackClick(object sender, KeyEventArgs e)
        {
            string selFind = "";

            if (FeedbackFind.Text != "")
                selFind = $"AND dbo.Car.Model LIKE '{FeedbackFind.Text}%'";

            Feedback.Columns.Clear();
            connection = new SqlConnection(connectionStr);
            connection.Open();

            string strQ = $"SELECT dbo.Contract.Feedback as Відгук, dbo.Contract.Date as Дата, (dbo.Car.Price + dbo.Car.Com) as Ціна, dbo.Car.Model as Модель FROM dbo.Contract INNER JOIN dbo.Car ON dbo.Contract.IDCar = dbo.Car.IDCar WHERE dbo.Contract.Feedback != '' {selFind} {selTime};";
            adapter = new SqlDataAdapter(strQ, connection);
            dt = new DataTable("Відгуки");
            adapter.Fill(dt);
            Feedback.AutoGeneratedColumns += (s, m) =>
            {
                (Feedback.Columns[1] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            };
            Feedback.ItemsSource = dt.DefaultView;

            connection.Close();
        }
        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (index > 0)
            {
                index--;
                check();
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (index < dt.Rows.Count - 1)
            {
                index++;
                check();
            }
        }
        private void check()
        {
            if (dt.Rows.Count > 0)
            {
                Buy.IsEnabled = true;
                Number.Content = "Номер " + dt.Rows[index][0].ToString();
                Model.Content = dt.Rows[index][2].ToString();
                Run.Content = dt.Rows[index][3].ToString();
                Price.Content = dt.Rows[index][4].ToString();
                if (dt.Rows[index][5].ToString().Contains(" "))
                    Date.Content = dt.Rows[index][5].ToString().Substring(0, 9);
                else
                    Date.Content = dt.Rows[index][5].ToString().Substring(0, 10);
                ImageLoad.Source = Temporary.GetImageFromByteArray((byte[])dt.Rows[index][1]);
            }
            else
                Buy.IsEnabled = false;
        }
        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            FeedbackW fw = new FeedbackW();
            fw.ShowDialog();

            if (FeedbackW.f)
            {
                connection = new SqlConnection(connectionStr);
                connection.Open();

                string strQ = $"SELECT IDDealer FROM dbo.Car WHERE IDCar = {dt.Rows[index][0].ToString()};";
                int dealer = (int)new SqlCommand(strQ, connection).ExecuteScalar();

                strQ = $"INSERT INTO Contract values({Temporary.serial}, {dealer}, {dt.Rows[index][0].ToString()}, GETDATE(), N'{FeedbackW.fd}');";
                command = new SqlCommand(strQ, connection);
                command.ExecuteNonQuery();

                strQ = $"UPDATE dbo.Car SET dbo.Car.Status = 0 WHERE dbo.Car.IDCar = {dt.Rows[index][0].ToString()};";
                command = new SqlCommand(strQ, connection);
                command.ExecuteNonQuery();

                updateFeedback();
                updateCars();
                index = 0;
                check();

                MessageBox.Show($"Ви купили автомобіль");
                connection.Close();
            }
        }
        static public string FeedbackCorrection(string fd)
        {
            string res = "";
            string[] temp = new string[fd.Length / 70 + 1];
            for (int i = 0; i < fd.Length; i += 70)
            {
                if (i == (fd.Length / 70) * 70)
                {
                    temp[i / 70] = fd.Substring(i, (int)((double)fd.Length % 70));
                    res += temp[i / 70];
                    break;
                }
                temp[i / 70] = fd.Substring(i, 70);
                res += temp[i / 70] + "\n";
            }
            return res;
        }
        private void Serial_Click(object sender, RoutedEventArgs e)
        {
            ClientData cd = new ClientData();
            cd.ShowDialog();
        }
    }
}