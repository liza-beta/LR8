using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form5 : Form
    {
        private string connectionString = "Data Source=LIZAVETAS-LAPTO;Initial Catalog=Repair;Integrated Security=True;TrustServerCertificate=True";

        public Form5(string name)
        {
            InitializeComponent();
            labelName.Text = name;
            LoadRepairRequests();
            LoadStatuses();
            displayLines();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                LoadRepairRequests();
                LoadStatuses();
                labelLines.Visible = true;
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                LoadChartData();
                labelLines.Visible = false;
            }
        }

        private void LoadStatuses()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT StatusID, StatusName FROM Status";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            DataRow row = dataTable.NewRow();
                            row["StatusID"] = 0;
                            row["StatusName"] = "Показать все";
                            dataTable.Rows.InsertAt(row, 0);

                            comboBox2.DisplayMember = "StatusName";
                            comboBox2.ValueMember = "StatusID";
                            comboBox2.DataSource = dataTable;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadRepairRequests(int statusID = 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            r.RepairRequestID AS ID,
                            r.RequestDate AS [Дата создания],
                            u.FullName AS Заказчик,
                            dt.TypeName AS [Тип оборудования],
                            d.DeviceModel AS [Модель оборудования],
                            s.StatusName AS Статус,
                            r.ProblemDescription AS Описание
                        FROM 
                            RepairRequest r
                        JOIN 
                            Users u ON r.ClientID = u.UserID
                        JOIN 
                            Devices d ON r.DeviceID = d.DeviceID
                        JOIN 
                            DeviceTypes dt ON d.TypeID = dt.TypeID
                        JOIN 
                            Status s ON r.Status = s.StatusID";

                    if (statusID > 0)
                    {
                        query += " WHERE r.Status = @StatusID";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (statusID > 0)
                        {
                            command.Parameters.AddWithValue("@StatusID", statusID);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            dataGridView1.DataSource = dataTable;

                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                            dataGridView1.RowHeadersVisible = false;
                            dataGridView1.AllowUserToAddRows = false;
                            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                            dataGridView1.ScrollBars = ScrollBars.Both;

                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ComboBoxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedStatusID = (int)comboBox2.SelectedValue;
            LoadRepairRequests(selectedStatusID);
            displayLines();
        }

        private void displayLines()
        {
            labelLines.Text = "" + countCurrentLines() + " из " + countTotalLines() + " заявок";
        }

        private int countTotalLines()
        {
            int totalLines = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM RepairRequest";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        totalLines = (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return totalLines;
        }

        private int countCurrentLines()
        {
            int currentLines = dataGridView1.Rows.Count;
            return currentLines;
        }

        private void LoadChartData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                    SELECT 
                        s.StatusName AS Статус,
                        COUNT(*) AS Количество
                    FROM 
                        RepairRequest r
                    JOIN 
                        Status s ON r.Status = s.StatusID
                    GROUP BY 
                        s.StatusName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            chart1.Series.Clear();

                            Series series = new Series
                            {
                                Name = "Заявки по статусам",
                                IsValueShownAsLabel = true,
                                ChartType = SeriesChartType.Pie
                            };
                            chart1.Series.Add(series);

                            while (reader.Read())
                            {
                                string statusName = reader["Статус"].ToString();
                                int count = Convert.ToInt32(reader["Количество"]);

                                series.Points.AddXY(statusName, count);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных для диаграммы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Form5_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
