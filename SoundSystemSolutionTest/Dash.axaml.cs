using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MySql.Data.MySqlClient;

namespace SoundSystemSolutionTest
{
    public partial class Dash : Window
    {
        private Panel? _tab1Content; //welcome tab content
        private Panel? _tab2Content; //transaction tab content
        private Panel? _tab3Content; //rentals tab content
        private Panel? _tab4Content; //partial deposits tab content
        private Panel? _tab5Content; //inventory tab content
        
        //part of transaction tab content
        private ContentControl? _contentArea;
        private readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        private DataGrid? _transactionDataGrid;
        private readonly ObservableCollection<TransactionItem> _transactionData = new ObservableCollection<TransactionItem>();
        
        //part of rentals tab content
        private StackPanel? _activeRentalsPanel;
        private readonly ObservableCollection<TransactionItem> _activeRentalsData = new ObservableCollection<TransactionItem>();
        
        //part of partial deposits tab content
        private StackPanel? _partialDepositsPanel;
        private readonly ObservableCollection<TransactionItem> _partialDepositsData = new ObservableCollection<TransactionItem>();
        
        private static string _dbpassword = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
        private static readonly MySqlConnectionStringBuilder Connection = new()
        {
            Server = "soundsystemdb-govsystem01.j.aivencloud.com",
            Port = 16924,
            UserID = "avnadmin",
            Password = _dbpassword,
            Database = "soundsystemdata" 
        };

        public Dash()
        {
            InitializeComponent();
            InitializeTabContent();
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        //prevents the window from being resized when the window is maximized (getting shaved down on the left side)
        {
            if (e.Property == WindowStateProperty)
            {
                UpdateWindowMargins();
            }
        }

        private void UpdateWindowMargins()
        //updates the window margins when the window is resized
        {
            if (WindowState == WindowState.Maximized)
            {
                Padding = new Thickness(8, 0, 8, 8);
            }
            else
            {
                Padding = new Thickness(0);
            }
        }
        
        private void InitializeComponent()
        //initializes the tabs and their content
        {
            AvaloniaXamlLoader.Load(this);
            
            PropertyChanged += OnWindowPropertyChanged;
            _contentArea = this.FindControl<ContentControl>("ContentArea");
            
            //////
            ////// WELCOME TAB CONTENT ///////
            //////
            
            _tab1Content = new Grid();
            
            //container for the welcome message
            var welcomePanel = new StackPanel
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            welcomePanel.Children.Add(new TextBlock
            {
                Text = "Welcome to Bicol Sound",
                FontSize = 28,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.SkyBlue),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 10, 0, 10),
                Background = new SolidColorBrush(Color.FromArgb(20, 135, 206, 250)) //for transparency
            };
            
            var messagePanel = new StackPanel { Spacing = 15 };
            messagePanel.Children.Add(new TextBlock
            {
                Text = "Navigate through the tabs to explore different features.",
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DarkSlateGray)
            });
            
            border.Child = messagePanel;
            welcomePanel.Children.Add(border);
            
            _tab1Content.Children.Add(welcomePanel);
            
            //////
            ////// WELCOME TAB CONTENT END //////
            //////
            
            
            //////
            ////// TRANSACTION TAB CONTENT ///////
            //////
            
            _tab2Content = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            //container for the transaction history
            var transactionGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star) 
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            var transactionBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BoxShadow = BoxShadows.Parse("0 4 8 0 #20000000"),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(15),
                Padding = new Thickness(15),
                ClipToBounds = true,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch 
            };
            
            transactionBorder.Child = transactionGrid;
            
            var transactionTitle = new TextBlock
            {
                Text = "Transaction History",
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69")),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            transactionTitle.SetValue(Grid.RowProperty, 0);
            transactionTitle.SetValue(Grid.ColumnProperty, 0);
            transactionGrid.Children.Add(transactionTitle);
            
            
            //data grid to display transactions
            _transactionDataGrid = new DataGrid
            {
                IsReadOnly = true,
                Margin = new Thickness(0, 10, 0, 0), 
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                HeadersVisibility = DataGridHeadersVisibility.All,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = _transactionData,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                ClipToBounds = true
            };
            _transactionDataGrid.SetValue(Grid.RowProperty, 1);
            _transactionDataGrid.SetValue(Grid.ColumnSpanProperty, 4);
            transactionGrid.Children.Add(_transactionDataGrid);

            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Transaction ID",
                Binding = new Avalonia.Data.Binding("TransactionId"),
                Width = DataGridLength.Auto
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Customer ID",
                Binding = new Avalonia.Data.Binding("CustomerId"),
                Width = DataGridLength.Auto
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Bundle ID",
                Binding = new Avalonia.Data.Binding("BundleId"),
                Width = DataGridLength.Auto
            });

            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Method",
                Binding = new Avalonia.Data.Binding("Method"),
                Width = DataGridLength.Auto
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Rent Date",
                Binding = new Avalonia.Data.Binding("RentDate"),
                Width = DataGridLength.SizeToHeader
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Rent End Date",
                Binding = new Avalonia.Data.Binding("RentEndDate"),
                Width = DataGridLength.SizeToHeader
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Rent Duration",
                Binding = new Avalonia.Data.Binding("RentDuration"),
                Width = DataGridLength.SizeToHeader
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Rental Status",
                Binding = new Avalonia.Data.Binding("RentalStatus"),
                Width = DataGridLength.Auto
            });

            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Deposit (80%)",
                Binding = new Avalonia.Data.Binding("Deposit"),
                Width = DataGridLength.SizeToHeader
            });

            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Upfront Payment",
                Binding = new Avalonia.Data.Binding("RentalFee"),
                Width = DataGridLength.Auto
            });
            
            _transactionDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Total Amount Paid/to Pay",
                Binding = new Avalonia.Data.Binding("Amount"),
                Width = DataGridLength.Auto, 
                MinWidth = 100 
            });
            
            //refresh button
            var transactionRefreshButton = new Button
            {
                Content = "Refresh",
                FontSize = 15,
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            transactionRefreshButton.Classes.Add("ContentButton");
            transactionRefreshButton.Click += RefreshTransactions_Click;
            transactionRefreshButton.SetValue(Grid.RowProperty, 0);
            transactionRefreshButton.SetValue(Grid.ColumnProperty, 1);
            transactionGrid.Children.Add(transactionRefreshButton);

            //new transaction button
            var transactionButtonNew = new Button
            {
                Content = "New Transaction",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            transactionButtonNew.Classes.Add("ContentButton");
            transactionButtonNew.SetValue(Grid.RowProperty, 0);
            transactionButtonNew.SetValue(Grid.ColumnProperty, 2);
            transactionGrid.Children.Add(transactionButtonNew);
            
            _tab2Content.Children.Add(transactionBorder);
            
            //////
            ////// TRANSACTION TAB END ///////
            //////
            
            
            //////
            ////// ACTIVE RENTALS TAB CONTENT ///////
            //////
            
            _tab3Content = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            //container for the active rentals
            var activeRentalsGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star) 
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            var activeRentalsBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BoxShadow = BoxShadows.Parse("0 4 8 0 #20000000"),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(15),
                Padding = new Thickness(15),
                ClipToBounds = true,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch 
            };
            
            activeRentalsBorder.Child = activeRentalsGrid;
            
            //header panel for the active rentals
            var headerPanel = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            
            var activeRentalsTitle = new TextBlock
            {
                Text = "Active Rentals",
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69")),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(activeRentalsTitle);
            
            var activeRentalsRefreshButton = new Button
            {
                Content = "Refresh",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            activeRentalsRefreshButton.Classes.Add("ContentButton");
            activeRentalsRefreshButton.Click += RefreshActiveRentals_Click;
            activeRentalsRefreshButton.SetValue(Grid.ColumnProperty, 1);
            headerPanel.Children.Add(activeRentalsRefreshButton);
            
            headerPanel.SetValue(Grid.RowProperty, 0);
            activeRentalsGrid.Children.Add(headerPanel);
            
            //scrolling panel for the active rentals
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 10, 0, 0)
            };
            scrollViewer.SetValue(Grid.RowProperty, 1);
            
            _activeRentalsPanel = new StackPanel
            {
                Spacing = 10,
                Orientation = Orientation.Vertical
            };
            
            scrollViewer.Content = _activeRentalsPanel;
            activeRentalsGrid.Children.Add(scrollViewer);
            
            _tab3Content.Children.Add(activeRentalsBorder);
            
            //////
            ////// ACTIVE RENTALS TAB END ///////
            //////
            
            //////
            ////// PARTIAL DEPOSITS TAB CONTENT ///////
            //////
            
            _tab4Content = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            //container for the partial deposits
            var partialDepositsGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star) 
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            var partialDepositsBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BoxShadow = BoxShadows.Parse("0 4 8 0 #20000000"),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(15),
                Padding = new Thickness(15),
                ClipToBounds = true,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch 
            };
            
            partialDepositsBorder.Child = partialDepositsGrid;
            
            //header panel for the partial deposits
            var partialDepositsHeaderPanel = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            
            var partialDepositsTitle = new TextBlock
            {
                Text = "Partial Deposits",
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69")),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            partialDepositsHeaderPanel.Children.Add(partialDepositsTitle);
            
            var partialDepositsRefreshButton = new Button
            {
                Content = "Refresh",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            partialDepositsRefreshButton.Classes.Add("ContentButton");
            partialDepositsRefreshButton.Click += RefreshPartialDeposits_Click;
            partialDepositsRefreshButton.SetValue(Grid.ColumnProperty, 1);
            partialDepositsHeaderPanel.Children.Add(partialDepositsRefreshButton);
            
            partialDepositsHeaderPanel.SetValue(Grid.RowProperty, 0);
            partialDepositsGrid.Children.Add(partialDepositsHeaderPanel);
            
            //scrolling panel for the partial deposits
            var partialDepositsScrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 10, 0, 0)
            };
            partialDepositsScrollViewer.SetValue(Grid.RowProperty, 1);
            
            _partialDepositsPanel = new StackPanel
            {
                Spacing = 10,
                Orientation = Orientation.Vertical
            };
            
            partialDepositsScrollViewer.Content = _partialDepositsPanel;
            partialDepositsGrid.Children.Add(partialDepositsScrollViewer);
            
            _tab4Content.Children.Add(partialDepositsBorder);
            
            //////
            ////// PARTIAL DEPOSITS TAB END ///////
            //////

            var tab5Content = new TextBlock
            {
                Text = "Tab 5 Content",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };
            
            _tab5Content?.Children.Add(tab5Content);

            
            //applies button names from .axaml counterpart
            var tab1Button = this.FindControl<Button>("Tab1Button");
            var tab2Button = this.FindControl<Button>("Tab2Button");
            var tab3Button = this.FindControl<Button>("Tab3Button");
            var tab4Button = this.FindControl<Button>("Tab4Button");
            var tab5Button = this.FindControl<Button>("Tab5Button");
            
            if (tab1Button != null) _tabButtons["Tab1Button"] = tab1Button;
            if (tab2Button != null) _tabButtons["Tab2Button"] = tab2Button;
            if (tab3Button != null) _tabButtons["Tab3Button"] = tab3Button;
            if (tab4Button != null) _tabButtons["Tab4Button"] = tab4Button;
            if (tab5Button != null) _tabButtons["Tab5Button"] = tab5Button;
        }

        private void InitializeTabContent()
        //initializes the content of the tabs based on the content area
        {
            if (_contentArea != null)
            {
                _contentArea.Content = _tab1Content;
            }
        }
        
        public class TransactionItem
        //data model for the transaction item
        {
            public string TransactionId { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string Amount { get; set; } = string.Empty;
            public string Deposit { get; set; } = string.Empty;
            public string RentalFee { get; set; } = string.Empty;
            public string RentDate { get; set; } = string.Empty;
            public string RentDuration { get; set; } = string.Empty;
            public string RentEndDate { get; set; } = string.Empty;
            public string BundleId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            
            //rental status is determined by the expiration date of the rental
            public bool IsRentalExpired { get; set; }
            public string RentalStatus => IsRentalExpired ? "Expired" : "Active";
        }

        private async void RefreshTransactions_Click(object? sender, RoutedEventArgs e)
        //handles the click event of the refresh button
        {
            await LoadTransactionsAsync();
        }

        private async Task LoadTransactionsAsync()
        //loads the transactions from the database and populates the data grid
        {
            try
            {
                _transactionData.Clear();
                
                await using var conn = new MySqlConnection(Connection.ConnectionString);
                await conn.OpenAsync();
                
                string query = @"SELECT TransactionID, Method, Amount, Deposit, RentalFee, 
                                RentDate, RentDuration, RentEndDate, 
                                (CURDATE() > RentEndDate) AS IsRentalExpired, -- calculates if the rental is expired
                                BundleID, CustomerID 
                                FROM transactions 
                                ORDER BY TransactionID";
                
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var transaction = new TransactionItem
                    {
                        TransactionId = reader["TransactionID"].ToString() ?? string.Empty,
                        Method = GetMethodText(reader["Method"]),
                        Amount = FormatCurrency(reader["Amount"]),
                        Deposit = GetDepositText(reader["Deposit"]),
                        RentalFee = FormatCurrency(reader["RentalFee"]),
                        RentDate = reader["RentDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentDate"]).ToString("yyyy-MM-dd") : "N/A",
                        RentDuration = reader["RentDuration"] != DBNull.Value ? 
                            reader["RentDuration"] + " days" : "N/A",
                        RentEndDate = reader["RentEndDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentEndDate"]).ToString("yyyy-MM-dd") : "N/A",
                        IsRentalExpired = reader["IsRentalExpired"] != DBNull.Value && 
                                         Convert.ToBoolean(reader["IsRentalExpired"]),
                        BundleId = reader["BundleID"].ToString() ?? string.Empty,
                        CustomerId = reader["CustomerID"].ToString() ?? string.Empty
                    };
                    
                    _transactionData.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
        }

        private string GetDepositText(object? depositValue)
        //formats the deposit value to a human-readable format
        {
            if (depositValue == null || depositValue == DBNull.Value)
                return "Unknown";
        
            return depositValue.ToString() switch
            {
                "0" => "Partial",
                "1" => "Full", 
                "2" => "Complete",
                _ => "Unknown"
            };
        }

        private string GetMethodText(object? methodValue)
        //formats the method value to a human-readable format
        {
            if (methodValue == null || methodValue == DBNull.Value)
                return "Unknown";
                
            return methodValue.ToString() switch
            {
                "0" => "Cash",
                "1" => "Card",
                "2" => "Bank Transfer",
                "3" => "Check",
                _ => "Unknown"
            };
        }

        private string FormatCurrency(object? amount)
        //formats the amount to a currency format
        {
            if (amount == null || amount == DBNull.Value)
                return "₱0.00";
                
            if (decimal.TryParse(amount.ToString(), out decimal value))
                return $"₱{value:F2}";
                
            return "₱0.00";
        }
        
        private async void RefreshActiveRentals_Click(object? sender, RoutedEventArgs e)
        //refreshes the active rentals
        {
            await LoadActiveRentalsAsync();
        }

        private async Task LoadActiveRentalsAsync()
        //loads the active rentals from the database
        {
            try
            {
                _activeRentalsData.Clear();
                _activeRentalsPanel?.Children.Clear();
                
                await using var conn = new MySqlConnection(Connection.ConnectionString);
                await conn.OpenAsync();
                
                string query = @"SELECT TransactionID, Method, Amount, Deposit, RentalFee, 
                                RentDate, RentDuration, RentEndDate, 
                                (CURDATE() > RentEndDate) AS IsRentalExpired,
                                BundleID, CustomerID 
                                FROM transactions 
                                WHERE (CURDATE() <= RentEndDate) OR RentEndDate IS NULL
                                ORDER BY RentDate";
                
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var transaction = new TransactionItem
                    {
                        TransactionId = reader["TransactionID"].ToString() ?? string.Empty,
                        Method = GetMethodText(reader["Method"]),
                        Amount = FormatCurrency(reader["Amount"]),
                        Deposit = GetDepositText(reader["Deposit"]),
                        RentalFee = FormatCurrency(reader["RentalFee"]),
                        RentDate = reader["RentDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentDate"]).ToString("yyyy-MM-dd") : "N/A",
                        RentDuration = reader["RentDuration"] != DBNull.Value ? 
                            reader["RentDuration"] + " days" : "N/A",
                        RentEndDate = reader["RentEndDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentEndDate"]).ToString("yyyy-MM-dd") : "N/A",
                        IsRentalExpired = reader["IsRentalExpired"] != DBNull.Value && 
                                         Convert.ToBoolean(reader["IsRentalExpired"]),
                        BundleId = reader["BundleID"].ToString() ?? string.Empty,
                        CustomerId = reader["CustomerID"].ToString() ?? string.Empty
                    };
                    
                    _activeRentalsData.Add(transaction);
                    CreateRentalCard(transaction);
                }
                
                if (_activeRentalsData.Count == 0)
                {
                    var noDataCard = new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#F5F5F5")),
                        BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(20),
                        Margin = new Thickness(5),
                        Child = new TextBlock
                        {
                            Text = "No active rentals found",
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Colors.Gray)
                        }
                    };
                    _activeRentalsPanel?.Children.Add(noDataCard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading active rentals: {ex.Message}");
            }
        }

        private void CreateRentalCard(TransactionItem transaction)
        //renders a card for an appropriate rental 
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(5),
                BoxShadow = BoxShadows.Parse("0 2 4 0 #10000000")
            };
            
            var cardContent = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            
            //header panel
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };
            
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"Transaction #{transaction.TransactionId}",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69"))
            });
            
            var statusBadge = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#4CAF50")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 4),
                Child = new TextBlock
                {
                    Text = "ACTIVE",
                    FontSize = 10,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Colors.White)
                }
            };
            
            headerPanel.Children.Add(statusBadge);
            headerPanel.SetValue(Grid.RowProperty, 0);
            headerPanel.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(headerPanel);
            
            //details grid
            var detailsGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            //customer and bundle details
            var customerInfo = CreateInfoBlock("Customer ID:", transaction.CustomerId);
            customerInfo.SetValue(Grid.RowProperty, 0);
            customerInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(customerInfo);
            
            var bundleInfo = CreateInfoBlock("Bundle ID:", transaction.BundleId);
            bundleInfo.SetValue(Grid.RowProperty, 0);
            bundleInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(bundleInfo);
            
            //rental info
            var startDateInfo = CreateInfoBlock("Start Date:", transaction.RentDate);
            startDateInfo.SetValue(Grid.RowProperty, 1);
            startDateInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(startDateInfo);
            
            var endDateInfo = CreateInfoBlock("End Date:", transaction.RentEndDate);
            endDateInfo.SetValue(Grid.RowProperty, 1);
            endDateInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(endDateInfo);
            
            //payment info
            var methodInfo = CreateInfoBlock("Payment Method:", transaction.Method);
            methodInfo.SetValue(Grid.RowProperty, 2);
            methodInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(methodInfo);
            
            var amountInfo = CreateInfoBlock("Total Amount:", transaction.Amount);
            amountInfo.SetValue(Grid.RowProperty, 2);
            amountInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(amountInfo);
            
            detailsGrid.SetValue(Grid.RowProperty, 1);
            detailsGrid.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(detailsGrid);
            
            //duration info
            var durationPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            durationPanel.Children.Add(new TextBlock
            {
                Text = "Duration:",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Colors.Gray)
            });
            
            durationPanel.Children.Add(new TextBlock
            {
                Text = transaction.RentDuration,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            });
            
            durationPanel.SetValue(Grid.RowProperty, 2);
            durationPanel.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(durationPanel);
            
            card.Child = cardContent;
            _activeRentalsPanel?.Children.Add(card);
        }
        
        private StackPanel CreateInfoBlock(string label, string value)
        //creates a panel for a label and value
        {
            var panel = new StackPanel
            {
                Spacing = 2,
                Margin = new Thickness(0, 0, 10, 5)
            };
            
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Colors.Gray)
            });
            
            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap
            });
            
            return panel;
        }
        
        private async void RefreshPartialDeposits_Click(object? sender, RoutedEventArgs e)
        //refreshes the partial deposits
        {
            await LoadPartialDepositsAsync();
        }

        private async Task LoadPartialDepositsAsync()
        //loads the partial deposits from the database in ascending order
        {
            try
            {
                _partialDepositsData.Clear();
                _partialDepositsPanel?.Children.Clear();
                
                await using var conn = new MySqlConnection(Connection.ConnectionString);
                await conn.OpenAsync();
                
                string query = @"SELECT TransactionID, Method, Amount, Deposit, RentalFee, 
                                RentDate, RentDuration, RentEndDate, 
                                (CURDATE() > RentEndDate) AS IsRentalExpired,
                                BundleID, CustomerID 
                                FROM transactions 
                                WHERE Deposit = 0
                                ORDER BY RentDate";
                
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var transaction = new TransactionItem
                    {
                        TransactionId = reader["TransactionID"].ToString() ?? string.Empty,
                        Method = GetMethodText(reader["Method"]),
                        Amount = FormatCurrency(reader["Amount"]),
                        Deposit = GetDepositText(reader["Deposit"]),
                        RentalFee = FormatCurrency(reader["RentalFee"]),
                        RentDate = reader["RentDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentDate"]).ToString("yyyy-MM-dd") : "N/A",
                        RentDuration = reader["RentDuration"] != DBNull.Value ? 
                            reader["RentDuration"] + " days" : "N/A",
                        RentEndDate = reader["RentEndDate"] != DBNull.Value ? 
                            Convert.ToDateTime(reader["RentEndDate"]).ToString("yyyy-MM-dd") : "N/A",
                        IsRentalExpired = reader["IsRentalExpired"] != DBNull.Value && 
                                         Convert.ToBoolean(reader["IsRentalExpired"]),
                        BundleId = reader["BundleID"].ToString() ?? string.Empty,
                        CustomerId = reader["CustomerID"].ToString() ?? string.Empty
                    };
                    
                    _partialDepositsData.Add(transaction);
                    CreatePartialDepositCard(transaction);
                }
                
                if (_partialDepositsData.Count == 0)
                {
                    var noDataCard = new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#F5F5F5")),
                        BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(20),
                        Margin = new Thickness(5),
                        Child = new TextBlock
                        {
                            Text = "No partial deposits found",
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Colors.Gray)
                        }
                    };
                    _partialDepositsPanel?.Children.Add(noDataCard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading partial deposits: {ex.Message}");
            }
        }

        private void CreatePartialDepositCard(TransactionItem transaction)
        //renders a card for a partial deposit
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#FFA500")), // Orange border for partial deposits
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(5),
                BoxShadow = BoxShadows.Parse("0 2 4 0 #10000000")
            };
            
            var cardContent = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            
            //header row - transaction ID and status
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };
            
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"Transaction #{transaction.TransactionId}",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69"))
            });
            
            var depositBadge = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FF9800")), // Orange background
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 4),
                Child = new TextBlock
                {
                    Text = "PARTIAL DEPOSIT",
                    FontSize = 10,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Colors.White)
                }
            };
            
            headerPanel.Children.Add(depositBadge);
            
            //add the deposit status to the header panel
            if (transaction.IsRentalExpired)
            {
                var expiredBadge = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#F44336")), // Red background
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(8, 4),
                    Child = new TextBlock
                    {
                        Text = "EXPIRED",
                        FontSize = 10,
                        FontWeight = FontWeight.Bold,
                        Foreground = new SolidColorBrush(Colors.White)
                    }
                };
                headerPanel.Children.Add(expiredBadge);
            }
            
            headerPanel.SetValue(Grid.RowProperty, 0);
            headerPanel.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(headerPanel);
            
            //details grid
            var detailsGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            //customer and bundle info
            var customerInfo = CreateInfoBlock("Customer ID:", transaction.CustomerId);
            customerInfo.SetValue(Grid.RowProperty, 0);
            customerInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(customerInfo);
            
            var bundleInfo = CreateInfoBlock("Bundle ID:", transaction.BundleId);
            bundleInfo.SetValue(Grid.RowProperty, 0);
            bundleInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(bundleInfo);
            
            //renter info
            var startDateInfo = CreateInfoBlock("Start Date:", transaction.RentDate);
            startDateInfo.SetValue(Grid.RowProperty, 1);
            startDateInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(startDateInfo);
            
            var endDateInfo = CreateInfoBlock("End Date:", transaction.RentEndDate);
            endDateInfo.SetValue(Grid.RowProperty, 1);
            endDateInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(endDateInfo);
            
            //payment info
            var methodInfo = CreateInfoBlock("Payment Method:", transaction.Method);
            methodInfo.SetValue(Grid.RowProperty, 2);
            methodInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(methodInfo);
            
            var amountInfo = CreateInfoBlock("Total Amount:", transaction.Amount);
            amountInfo.SetValue(Grid.RowProperty, 2);
            amountInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(amountInfo);
            
            //deposit info
            var depositInfo = CreateInfoBlock("Deposit Status:", transaction.Deposit);
            depositInfo.SetValue(Grid.RowProperty, 3);
            depositInfo.SetValue(Grid.ColumnProperty, 0);
            detailsGrid.Children.Add(depositInfo);
            
            var rentalFeeInfo = CreateInfoBlock("Upfront Payment:", transaction.RentalFee);
            rentalFeeInfo.SetValue(Grid.RowProperty, 3);
            rentalFeeInfo.SetValue(Grid.ColumnProperty, 1);
            detailsGrid.Children.Add(rentalFeeInfo);
            
            detailsGrid.SetValue(Grid.RowProperty, 1);
            detailsGrid.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(detailsGrid);
            
            //duration info
            var durationPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            durationPanel.Children.Add(new TextBlock
            {
                Text = "Duration:",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Colors.Gray)
            });
            
            durationPanel.Children.Add(new TextBlock
            {
                Text = transaction.RentDuration,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            });
            
            durationPanel.SetValue(Grid.RowProperty, 2);
            durationPanel.SetValue(Grid.ColumnSpanProperty, 2);
            cardContent.Children.Add(durationPanel);
            
            card.Child = cardContent;
            _partialDepositsPanel?.Children.Add(card);
        }
        
        public async void OnTabButtonClick(object sender, RoutedEventArgs args)
        //handles the click event of the tab buttons
        {
            if (sender is Button clickedButton)
            {
                //updates the appearance of the clicked tab button
                foreach (var button in _tabButtons.Values)
                {
                    button.Classes.Remove("Selected");
                }
                
                clickedButton.Classes.Add("Selected");
                
                
                //changes the content of the content area based on which tab is clicked
                if (_contentArea != null)
                {
                    if (clickedButton.Name == "Tab1Button")
                        _contentArea.Content = _tab1Content;
                    else if (clickedButton.Name == "Tab2Button")
                    {
                        //loads the transactions when the tab is clicked for the first time
                        _contentArea.Content = _tab2Content;
                        await LoadTransactionsAsync();
                    }
                    else if (clickedButton.Name == "Tab3Button")
                    {
                        _contentArea.Content = _tab3Content;
                        await LoadActiveRentalsAsync();
                    }
                    else if (clickedButton.Name == "Tab4Button")
                    {
                        _contentArea.Content = _tab4Content;
                        await LoadPartialDepositsAsync();
                    }
                    else if (clickedButton.Name == "Tab5Button")
                        _contentArea.Content = _tab5Content;
                }
            }
        }
    }
}