using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MySql.Data.MySqlClient;

namespace SoundSystemSolutionTest;

public partial class LoginWindow : Window
{
    private CancellationTokenSource? _awsAnimationCts;
    private static string _dbpassword = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
    private static readonly MySqlConnectionStringBuilder Connection = new()
    {
        Server = "soundsystemdb-govsystem01.j.aivencloud.com",
        Port = 16924,
        UserID = "avnadmin",
        Password = _dbpassword,
        Database = "logincredentials"
    };
    
    public LoginWindow()
    {
        InitializeComponent();
    }

    
    private async void LoginButton_OnClick(object? sender, RoutedEventArgs? e)
    //login button behaviour
    {
        try
        {
            string username = null!;
            string password = null!;
            
            //check if the textboxes are not empty
            if (!string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                username = UsernameTextBox.Text;
            }

            if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                password = PasswordTextBox.Text;
            }

            //checks if username and password matches in the database
            bool isValid = await VerifyCredentialsAsync(username, password);
        
            if (isValid)
            {
                //login successful, proceed with application logic
                //navigate to main application window?????
                StatusLabel.Text = "Login successful";
            }
            else
            {
                //login failed, show error message
                StatusLabel.Text = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            //show error to user
            Console.WriteLine($"Login error: {ex.Message}");
            
        }
    }

    
    private void PasswordTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    //behavior for when user presses enter while in password textbox
    {
        if (e.Key == Key.Enter)
        {
            LoginButton_OnClick(LoginButton, null);
            e.Handled = true;
        }
    }
    
    
    private async Task<bool> VerifyCredentialsAsync(string username, string password)
    //takes username and password as parameters and checks if the
    //credentials match a username and password in the database
    {
        try
        {
            await using var conn = new MySqlConnection(Connection.ConnectionString);
            await conn.OpenAsync();
            
            //COUNT(*) returns the number of rows that match the query
            string query = "SELECT COUNT(*) FROM credentials WHERE username = @username AND password = @password";
            await using var cmd = new MySqlCommand(query, conn);
            
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            
            //checks if the query returns a value greater than 0
            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            
            //if the count is greater than 0, the credentials match a username and password in the database
            Console.WriteLine(count);
            return count > 0;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Database query failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during credential verification: {ex.Message}");
            return false;
        }
    }


    private async void CheckAwsButton_OnClick(object? sender, RoutedEventArgs e)
    //check aws button behavior
    //checks if the AWS database connection is working
    //features an ellipsis animation
    {
        if (_awsAnimationCts != null)
        {
            await _awsAnimationCts.CancelAsync();   //stop any previous animation
        }
        _awsAnimationCts = new CancellationTokenSource();
        var token = _awsAnimationCts.Token;

        _ = AnimateEllipsisAsync(token);

        bool success = await TestAwsDatabaseAsync();

        await _awsAnimationCts.CancelAsync();       //stop animation
        AwsLabel.Text = success ? "✔" : "✘";
        AwsLabel.Foreground = success ? Brushes.Green : Brushes.Red;
    }

    
    private async Task AnimateEllipsisAsync(CancellationToken token)
    //animates the AWS label to show an ellipsis
    {
        var states = new[] { ".", "..", "..." };
        int index = 0;
        while (!token.IsCancellationRequested)
        {
            AwsLabel.Text = states[index];
            index = (index + 1) % states.Length;
            try { await Task.Delay(400, token); } catch { break; }
        }
    }
    
    
    private async Task<bool> TestAwsDatabaseAsync()
    //tests the AWS database connection
    {
        try
        {
            Connection.ConnectionTimeout = 5;       //seconds
            var conn = new MySqlConnection(Connection.ConnectionString);
        
            await using (conn)
            {
                await conn.OpenAsync();
                return true;
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Connection attempt timed out");
            return false;
        }
    }
}