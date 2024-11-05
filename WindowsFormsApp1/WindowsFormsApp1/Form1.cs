using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=LIZAVETAS-LAPTO;Initial Catalog=Repair;Integrated Security=True;TrustServerCertificate=True";
        private string captcha;
        private int attempts = 3;
        private string entered;
        private int lockoutCounter = 0;
        private DateTime lockoutEndTime;

        public Form1()
        {
            InitializeComponent();
            textBoxCaptcha.Visible = false;
            pictureBoxRefresh.Visible = false;
            pictureBoxCaptcha.Visible = false;
            button2.Visible = true;
            timer1.Tick += Timer_Tick;
            textBox2.PasswordChar = '*';
        }

        private void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        private void AddUserLogin(string login, string password, string entered)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO UserLogin (Date, Login, Password, Entered) VALUES (GETDATE(), @login, @password, @entered)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@entered", entered);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Ошибка: {ex.Message}", "Ошибка", MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void StartLockout(int minutes)
        {
            lockoutEndTime = DateTime.Now.AddMinutes(minutes);
            timer1.Start();
            button1.Enabled = false;
            ShowMessage($"Вход заблокирован на {minutes} минут.", "Блокировка", MessageBoxIcon.Warning);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= lockoutEndTime)
            {
                button1.Enabled = true;
                timer1.Stop();
                ShowMessage("Вы можете попробовать войти.", "Разблокировано", MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!button1.Enabled)
            {
                ShowMessage("Вход временно заблокирован. Подождите.", "Блокировка", MessageBoxIcon.Warning);
                return;
            }

            string login = textBox1.Text;
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Пожалуйста, заполните все поля.", "Предупреждение", MessageBoxIcon.Warning);
                return;
            }

            if (textBoxCaptcha.Visible && textBoxCaptcha.Text != captcha)
            {
                ShowMessage("Неверная CAPTCHA. Попробуйте снова.", "Ошибка CAPTCHA", MessageBoxIcon.Error);
                GenerateCaptcha();
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT FullName, RoleType FROM Users WHERE Login = @login AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = reader["FullName"].ToString();
                                int userType = (int)reader["RoleType"];

                                switch (userType)
                                {
                                    case 4:
                                        Form2 form2 = new Form2(fullName);
                                        form2.labelName.Text = fullName;
                                        form2.Show();
                                        this.Hide();
                                        break;
                                    case 2:
                                        Form4 form4 = new Form4(fullName);
                                        form4.labelName.Text = fullName;
                                        form4.Show();
                                        this.Hide();
                                        break;
                                    case 3:
                                        Form3 form3 = new Form3(fullName);
                                        form3.labelName.Text = fullName;
                                        form3.Show();
                                        this.Hide();
                                        break;
                                    case 1:
                                        Form5 form5 = new Form5(fullName);
                                        form5.labelName.Text = fullName;
                                        form5.Show();
                                        this.Hide();
                                        break;
                                    default:
                                        ShowMessage("Неизвестный тип пользователя.", "Ошибка", MessageBoxIcon.Error);
                                        break;
                                }

                                entered = "Успешно";
                                attempts = 3;
                                lockoutCounter = 0;
                            }
                            else
                            {
                                ShowMessage("Неверный логин или пароль.", "Ошибка входа", MessageBoxIcon.Error);
                                attempts--;
                                entered = "Ошибочно";

                                if (attempts == 2)
                                {
                                    textBoxCaptcha.Visible = true;
                                    pictureBoxRefresh.Visible = true;
                                    pictureBoxCaptcha.Visible = true;
                                    GenerateCaptcha();
                                }
                                else if (attempts <= 0)
                                {
                                    lockoutCounter++;
                                    if (lockoutCounter == 1)
                                    {
                                        StartLockout(3);
                                        attempts = 3;
                                    }
                                    else
                                    {
                                        ShowMessage("Вход заблокирован. Перезапустите приложение для новой попытки.", "Блокировка", MessageBoxIcon.Warning);
                                        button1.Enabled = false;
                                    }
                                }
                            }

                            AddUserLogin(login, password, entered);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Не удалось подключиться к базе данных. " + ex.Message, "Ошибка базы данных", MessageBoxIcon.Error);
            }
        }

        private void pictureBoxRefresh_Click(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            captcha = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            Bitmap bmp = new Bitmap(pictureBoxCaptcha.Width, pictureBoxCaptcha.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                for (int i = 0; i < 50; i++)
                {
                    int x = random.Next(bmp.Width);
                    int y = random.Next(bmp.Height);
                    bmp.SetPixel(x, y, Color.Gray);
                }

                using (Font font = new Font("Arial", 24, FontStyle.Bold))
                {
                    for (int i = 0; i < captcha.Length; i++)
                    {
                        float x = i * 20 + random.Next(-5, 5);
                        float y = random.Next(5, bmp.Height - 30);
                        g.DrawString(captcha[i].ToString(), font, Brushes.Black, x, y);
                    }
                }
            }
            pictureBoxCaptcha.Image = bmp;
        }

        private void buttonShowPassword_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
            {
                textBox2.PasswordChar = '\0';
                button2.Text = "Скрыть";
            }
            else
            {
                textBox2.PasswordChar = '*';
                button2.Text = "Показать";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
