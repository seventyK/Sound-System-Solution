using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;

namespace SoundSystemSolutionTest
{
    public partial class Dash : Window
    {
        private Panel? _tab1Content;
        private Panel? _tab2Content;
        private Panel? _tab3Content;
        private Panel? _tab4Content;
        private ContentControl? _contentArea;
        private readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        private DataGrid? _transactionDataGrid;
        private ObservableCollection<TransactionItem> _transactionData = new ObservableCollection<TransactionItem>();
        
        
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            _contentArea = this.FindControl<ContentControl>("ContentArea");
            
            //////
            ////// STYLES ////////////////////
            //////
            
            //data grid theme
            var dataGridTheme = new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            };

            //buttons
            var buttonTheme = new Style(x => x.OfType<Button>().Class("ContentButton"))
            {
                Setters =
                {
                    new Setter(ForegroundProperty, new SolidColorBrush(Colors.DarkBlue)),
                    new Setter(BackgroundProperty, new SolidColorBrush(Color.Parse("#E0E0E0"))),
                    new Setter(BorderBrushProperty, new SolidColorBrush(Color.Parse("#E0E0E0"))),
                    new Setter(BorderThicknessProperty, new Avalonia.Thickness(1)),
                    new Setter(HeightProperty, 30.0),
                    new Setter(MinWidthProperty, 150.0),
                    new Setter(CornerRadiusProperty, new Avalonia.CornerRadius(4)),
                    new Setter(HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center)
                }
            };
            
            //data grid styles
            var dataGridStyle = new Style(x => x.OfType<DataGrid>())
            {
                Setters =
                {
                    new Setter(BorderBrushProperty, new SolidColorBrush(Colors.DarkBlue)),
                    new Setter(DataGrid.GridLinesVisibilityProperty, DataGridGridLinesVisibility.All),
                    new Setter(DataGrid.HeadersVisibilityProperty, DataGridHeadersVisibility.All)
                }
            };

            //data grid header
            var dataGridHeaderStyle = new Style(x => x.OfType<DataGridColumnHeader>())
            {
                Setters =
                {
                    new Setter(BorderBrushProperty, new SolidColorBrush(Colors.DarkBlue)),
                    new Setter(BorderThicknessProperty, new Avalonia.Thickness(0.5)),
                    new Setter(ForegroundProperty, new SolidColorBrush(Colors.Black)),
                    new Setter(BackgroundProperty, new SolidColorBrush(Colors.Azure)),
                    new Setter(PaddingProperty, new Avalonia.Thickness(8, 4)),
                    new Setter(FontWeightProperty, FontWeight.SemiBold),
                    new Setter(FontSizeProperty, 15.0)
                }
            };

            //data grid cell styles
            var dataGridCellStyle = new Style(x => x.OfType<DataGridCell>())
            {
                Setters =
                {
                    new Setter(BorderBrushProperty, new SolidColorBrush(Colors.DarkBlue)),
                    new Setter(BorderThicknessProperty, new Avalonia.Thickness(0.5)),
                    new Setter(ForegroundProperty, new SolidColorBrush(Colors.Black)),
                    new Setter(BackgroundProperty, new SolidColorBrush(Colors.Azure)),
                    new Setter(FontSizeProperty, 14.0)
                }
            };

            // Add the styles
            Styles.Add(dataGridStyle);
            Styles.Add(dataGridHeaderStyle);
            Styles.Add(dataGridCellStyle);

            
            Styles.Add(dataGridTheme);
            Styles.Add(buttonTheme);
            
            //////
            ////// STYLES  END ///////////////
            //////
            
            //////
            ////// WELCOME TAB CONTENT ///////
            //////
            
            _tab1Content = new Grid();
            
            //container for the welcome message
            var welcomePanel = new StackPanel
            {
                Spacing = 20,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            welcomePanel.Children.Add(new TextBlock
            {
                Text = "Welcome to Bicol Sound",
                FontSize = 28,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.SkyBlue),
                BorderThickness = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 10, 0, 10),
                Background = new SolidColorBrush(
                    Color.FromArgb(20, 135, 206, 250))
            };
            
            var messagePanel = new StackPanel { Spacing = 15 };
            messagePanel.Children.Add(new TextBlock
            {
                Text = "Navigate through the tabs to explore different features.",
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
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
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
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
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            
            var transactionBorder = new Border
            {
                CornerRadius = new Avalonia.CornerRadius(8),
                BoxShadow = BoxShadows.Parse("0 4 8 0 #20000000"),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Avalonia.Thickness(1),
                Margin = new Avalonia.Thickness(15),
                Padding = new Avalonia.Thickness(15),
                ClipToBounds = true,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch 
            };
            
            transactionBorder.Child = transactionGrid;
            
            var transactionTitle = new TextBlock
            {
                Text = "Transaction History",
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#330d69")),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            transactionTitle.SetValue(Grid.RowProperty, 0);
            transactionTitle.SetValue(Grid.ColumnProperty, 0);
            transactionGrid.Children.Add(transactionTitle);
            
            
            //data grid to display transactions
            _transactionDataGrid = new DataGrid
            {
                IsReadOnly = true,
                Margin = new Avalonia.Thickness(0, 10, 0, 0), 
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                HeadersVisibility = DataGridHeadersVisibility.All,
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
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
                Header = "Rent Duration",
                Binding = new Avalonia.Data.Binding("RentDuration"),
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
                Header = "Total Amount to Pay",
                Binding = new Avalonia.Data.Binding("Amount"),
                Width = DataGridLength.Auto, 
                MinWidth = 100 
            });
            
            //refresh button
            var transactionRefreshButton = new Button
            {
                Content = "Refresh",
                FontSize = 15,
                Margin = new Avalonia.Thickness(0, 0, 10, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
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
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            transactionButtonNew.Classes.Add("ContentButton");
            transactionButtonNew.SetValue(Grid.RowProperty, 0);
            transactionButtonNew.SetValue(Grid.ColumnProperty, 2);
            transactionGrid.Children.Add(transactionButtonNew);
            
            _tab2Content.Children.Add(transactionBorder);
            
            //////
            ////// TRANSACTION TAB END ///////
            //////
            
            
            _tab3Content = new Panel
            {
                Children = { new TextBlock { Text = "Content 3", FontSize = 20, 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, 
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center } }
            };
            
            //applies button names from .axaml counterpart
            var tab1Button = this.FindControl<Button>("Tab1Button");
            var tab2Button = this.FindControl<Button>("Tab2Button");
            var tab3Button = this.FindControl<Button>("Tab3Button");
            
            if (tab1Button != null) _tabButtons["Tab1Button"] = tab1Button;
            if (tab2Button != null) _tabButtons["Tab2Button"] = tab2Button;
            if (tab3Button != null) _tabButtons["Tab3Button"] = tab3Button;
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
        {
            //properties of the transaction item
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
        {
            await LoadTransactionsAsync();
        }

        private async Task LoadTransactionsAsync()
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
                                ORDER BY TransactionID DESC";
                
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var transaction = new TransactionItem
                    {
                        TransactionId = reader["TransactionID"].ToString() ?? string.Empty,
                        Method = GetMethodText(reader["Method"]),
                        Amount = FormatCurrency(reader["Amount"]),
                        Deposit = reader["Deposit"].ToString() == "1" ? "Yes" : "No",
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
                        _contentArea.Content = _tab2Content;
                        // Load transactions when the tab is first opened
                        await LoadTransactionsAsync();
                    }
                    else if (clickedButton.Name == "Tab3Button")
                        _contentArea.Content = _tab3Content;
                }
            }
        }
    }
}