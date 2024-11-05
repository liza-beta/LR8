using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.VisualStyles;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        private string connectionString = "Data Source=LIZAVETAS-LAPTO;Initial Catalog=Repair;Integrated Security=True;TrustServerCertificate=True";

        public Form3(string name)
        {
            InitializeComponent();
            labelName.Text = name;
            LoadRepairRequests();
            displayLines();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                LoadRepairRequests();
                labelLines.Visible = true;
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                LoadDeviceTypes();
                LoadMaster();
                LoadStatus();
                labelLines.Visible = false;
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                LoadLogin();
                labelLines.Visible = false;
            }
        }

        private void LoadLogin()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                    SELECT *
                    FROM UserLogin";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            dataGridView2.DataSource = dataTable;

                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                            dataGridView2.RowHeadersVisible = false;
                            dataGridView2.AllowUserToAddRows = false;
                            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                            dataGridView2.ScrollBars = ScrollBars.Both;

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

        private void LoadRepairRequests()
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
                        Status s ON r.Status = s.StatusID;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
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

                    string query = @"
                    SELECT COUNT(*)
                    FROM 
                        RepairRequest r";

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

        private void LoadDeviceTypes()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT TypeName FROM DeviceTypes", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox4.Items.Clear();
                            while (reader.Read())
                            {
                                comboBox4.Items.Add(reader["TypeName"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке типов оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadMaster()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT FullName FROM Users WHERE RoleType = 2", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox1.Items.Clear();
                            while (reader.Read())
                            {
                                comboBox1.Items.Add(reader["FullName"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке мастеров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadStatus()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT StatusName FROM Status", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox2.Items.Clear();
                            while (reader.Read())
                            {
                                comboBox2.Items.Add(reader["StatusName"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке мастеров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox12.Text))
            {
                MessageBox.Show("Введите ID заявки для поиска.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox12.Enabled = true;
                return;
            }

            FindRequestById(textBox12.Text);
            textBox12.Enabled = false;
        }

        private void FindRequestById(string requestId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                    SELECT 
                        rr.RequestDate,
                        dt.TypeName AS DeviceType,
                        d.DeviceModel,
                        rr.ProblemDescription,
                        s.StatusName,
                        client.FullName AS ClientName,
                        master.FullName AS MasterName
                    FROM 
                        RepairRequest rr
                    INNER JOIN Devices d ON rr.DeviceID = d.DeviceID
                    INNER JOIN DeviceTypes dt ON d.TypeID = dt.TypeID
                    INNER JOIN Status s ON rr.Status = s.StatusID
                    INNER JOIN Users client ON rr.ClientID = client.UserID
                    LEFT JOIN Users master ON rr.MasterID = master.UserID
                    WHERE 
                        rr.RepairRequestID = @requestId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox11.Text = Convert.ToDateTime(reader["RequestDate"]).ToString("yyyy-MM-dd");
                                comboBox4.Text = reader["DeviceType"].ToString();
                                textBox4.Text = reader["DeviceModel"].ToString();
                                richTextBox2.Text = reader["ProblemDescription"].ToString();
                                comboBox2.Text = reader["StatusName"].ToString();
                                textBox8.Text = reader["ClientName"].ToString();
                                comboBox1.Text = reader["MasterName"]?.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Заявка с указанным ID не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox12.Enabled = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при поиске заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox12.Enabled = true;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox12.Text))
            {
                MessageBox.Show("Введите ID заявки для сохранения изменений.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveRequestChanges(textBox12.Text);
        }

        private void SaveRequestChanges(string requestId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                    UPDATE RepairRequest
                    SET 
                        RequestDate = @RequestDate,
                        DeviceID = (SELECT TOP 1 DeviceID FROM Devices WHERE DeviceModel = @DeviceModel),
                        ProblemDescription = @ProblemDescription,
                        Status = (SELECT StatusID FROM Status WHERE StatusName = @StatusName),
                        ClientID = (SELECT UserID FROM Users WHERE FullName = @ClientName),
                        MasterID = (SELECT UserID FROM Users WHERE FullName = @MasterName)
                    WHERE 
                        RepairRequestID = @requestId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RequestDate", DateTime.Parse(textBox11.Text));
                        command.Parameters.AddWithValue("@DeviceModel", textBox4.Text);
                        command.Parameters.AddWithValue("@ProblemDescription", richTextBox2.Text);
                        command.Parameters.AddWithValue("@StatusName", comboBox2.Text);
                        command.Parameters.AddWithValue("@ClientName", textBox8.Text);
                        command.Parameters.AddWithValue("@MasterName", string.IsNullOrEmpty(comboBox1.Text) ? (object)DBNull.Value : comboBox1.Text);
                        command.Parameters.AddWithValue("@requestId", requestId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            textBox11.Clear();
                            textBox4.Clear();
                            richTextBox2.Clear();
                            textBox8.Clear();
                            comboBox1.SelectedIndex = -1;
                            comboBox2.SelectedIndex = -1;
                            comboBox4.SelectedIndex = -1;
                            textBox12.Clear();
                            textBox12.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось сохранить изменения. Проверьте корректность данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
