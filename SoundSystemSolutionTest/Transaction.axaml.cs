using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySql.Data.MySqlClient;

namespace SoundSystemSolutionTest
{
    public partial class PurchaseWizard : Window
    {
        private static string _dbpassword = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
        private static readonly MySqlConnectionStringBuilder Connection = new()
        {
            Server = "soundsystemdb-govsystem01.j.aivencloud.com",
            Port = 16924,
            UserID = "avnadmin",
            Password = _dbpassword,
            Database = "soundsystemdata"
        };

        // Add Philippine culture for currency formatting
        private static readonly CultureInfo PhilippineCulture = new CultureInfo("en-PH");
        
        private int _currentPage = 1;
        private const int TotalPages = 4;

        // UI Elements
        private TextBlock? _headerText;
        private TextBlock? _stepIndicator;
        private StackPanel?[] _pages;
        private Button? _backButton;
        private Button? _nextButton;
        private Button? _finishButton;
        
        // Page 1 Controls
        private TextBox? _customerIdTextBox;
        private TextBox? _fullNameTextBox;
        private TextBox? _emailTextBox;
        private TextBox? _phoneTextBox;
        
        // Page 2 Controls
        private RadioButton? _cashRadio;
        private RadioButton? _cardRadio;
        private RadioButton? _bankTransferRadio;
        private RadioButton? _digitalWalletRadio;
        private StackPanel? _cardDetailsPanel;
        private TextBox? _cardNumberTextBox;
        private TextBox? _expiryTextBox;
        private TextBox? _cvvTextBox;
        private TextBox? _cardHolderTextBox;
        
        // Payment Amount Options
        private RadioButton? _payDepositRadio;
        private RadioButton? _payFullRadio;
        
        // Page 3 Controls
        private ComboBox? _bundleComboBox;
        private NumericUpDown? _rentalDurationUpDown;
        private CalendarDatePicker? _rentDatePicker;
        private TextBox? _venueAddressTextBox; // Add this line
        private TextBox? _notesTextBox; // Add this line
        private TextBlock? _rentalFeeLabel;
        private TextBlock? _depositLabel;
        private TextBlock? _totalAmountLabel;
        
        // Page 4 Controls
        private StackPanel? _reviewPanel;
        private CheckBox? _termsCheckBox;

        // Store bundle information
        private double _bundleDailyRate = 0;
        private string _bundleName = "";

        // Bundle daily rates according to DDL
        private readonly Dictionary<string, double> BundleDailyRates = new()
        {
            { "B001", 5000 },   // Basic Package: ₱5,000/day
            { "B002", 10000 },  // Standard Package: ₱10,000/day  
            { "B003", 20000 },  // Premium Package: ₱20,000/day
            { "B004", 35000 }   // Pro Package: ₱35,000/day
        };

        public PurchaseWizard()
        {
            InitializeComponent();
            InitializeControls();
            UpdatePageVisibility();
            UpdateNavigationButtons();
            SetupEventHandlers();
            
            UpdateNextButtonState();

        }
        
        private string GetSelectedPaymentAmount()
        {
            if (_payDepositRadio?.IsChecked == true) return "Deposit Only";
            if (_payFullRadio?.IsChecked == true) return "Full Amount";
            return "Not Selected";
        }

        private bool IsPayingFullAmount()
        {
            return _payFullRadio?.IsChecked == true;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeControls()
        {
            // Header elements
            _headerText = this.FindControl<TextBlock>("HeaderText");
            _stepIndicator = this.FindControl<TextBlock>("StepIndicator");
            
            // Pages
            _pages = new StackPanel?[4];
            _pages[0] = this.FindControl<StackPanel>("Page1");
            _pages[1] = this.FindControl<StackPanel>("Page2");
            _pages[2] = this.FindControl<StackPanel>("Page3");
            _pages[3] = this.FindControl<StackPanel>("Page4");
            
            // Navigation buttons
            _backButton = this.FindControl<Button>("BackButton");
            _nextButton = this.FindControl<Button>("NextButton");
            _finishButton = this.FindControl<Button>("FinishButton");
            
            // Page 1 controls
            _customerIdTextBox = this.FindControl<TextBox>("CustomerIdTextBox");
            _fullNameTextBox = this.FindControl<TextBox>("FullNameTextBox");
            _emailTextBox = this.FindControl<TextBox>("EmailTextBox");
            _phoneTextBox = this.FindControl<TextBox>("PhoneTextBox");
            
            // Page 2 controls
            _cashRadio = this.FindControl<RadioButton>("CashRadio");
            _cardRadio = this.FindControl<RadioButton>("CardRadio");
            _bankTransferRadio = this.FindControl<RadioButton>("BankTransferRadio");
            _digitalWalletRadio = this.FindControl<RadioButton>("DigitalWalletRadio");
            _cardDetailsPanel = this.FindControl<StackPanel>("CardDetailsPanel");
            _cardNumberTextBox = this.FindControl<TextBox>("CardNumberTextBox");
            _expiryTextBox = this.FindControl<TextBox>("ExpiryTextBox");
            _cvvTextBox = this.FindControl<TextBox>("CvvTextBox");
            _cardHolderTextBox = this.FindControl<TextBox>("CardHolderTextBox");
            
            // Payment amount options
            _payDepositRadio = this.FindControl<RadioButton>("PayDepositRadio");
            _payFullRadio = this.FindControl<RadioButton>("PayFullRadio");
            
            // Page 3 controls
            _bundleComboBox = this.FindControl<ComboBox>("BundleComboBox");
            _rentalDurationUpDown = this.FindControl<NumericUpDown>("RentalDurationUpDown");
            _rentDatePicker = this.FindControl<CalendarDatePicker>("RentDatePicker");
            _venueAddressTextBox = this.FindControl<TextBox>("VenueAddress"); 
            _rentalFeeLabel = this.FindControl<TextBlock>("RentalFeeLabel");
            _depositLabel = this.FindControl<TextBlock>("DepositLabel");
            _totalAmountLabel = this.FindControl<TextBlock>("TotalAmountLabel");
            _notesTextBox = this.FindControl<TextBox>("NotesTextBox"); 
            if (_rentalDurationUpDown != null) 
            {
                _rentalDurationUpDown.Value = 3; // Minimum 3 days
                _rentalDurationUpDown.Minimum = 3; // Enforce 3-day minimum
                _rentalDurationUpDown.Increment = 1; // Only allow integer increments
                _rentalDurationUpDown.FormatString = "0"; // Display as whole numbers only
                
            }


            
            // Page 4 controls
            _reviewPanel = this.FindControl<StackPanel>("ReviewPanel");
            _termsCheckBox = this.FindControl<CheckBox>("TermsCheckBox");
            
            // Set default values and configure CalendarDatePicker
            if (_rentDatePicker != null) 
            {
                _rentDatePicker.SelectedDate = DateTime.Today;
                _rentDatePicker.DisplayDateStart = DateTime.Today; // Prevent selecting dates before today
                _rentDatePicker.DisplayDate = DateTime.Today; // Set the calendar display to today
            }
            
            if (_rentalDurationUpDown != null) 
            {
                _rentalDurationUpDown.Value = 3; // Minimum 3 days
                _rentalDurationUpDown.Minimum = 3; // Enforce 3-day minimum
            }

            // Generate next customer ID
            _ = GenerateNextCustomerIdAsync();
        }




        private void SetupEventHandlers()
        {
            if (_cardRadio != null) 
                _cardRadio.IsCheckedChanged += (s, e) => UpdateCardDetailsVisibility();
            if (_cashRadio != null) 
                _cashRadio.IsCheckedChanged += (s, e) => UpdateCardDetailsVisibility();
            if (_bankTransferRadio != null)
                _bankTransferRadio.IsCheckedChanged += (s, e) => UpdateCardDetailsVisibility();
            if (_digitalWalletRadio != null)
                _digitalWalletRadio.IsCheckedChanged += (s, e) => UpdateCardDetailsVisibility();

            if (_bundleComboBox != null)
                _bundleComboBox.SelectionChanged += async (s, e) => await OnBundleSelectionChanged();
            if (_rentalDurationUpDown != null)
                _rentalDurationUpDown.ValueChanged += async (s, e) => await OnDurationChanged();

            // Add real-time validation for Page 1 fields
            if (_customerIdTextBox != null)
                _customerIdTextBox.TextChanged += (s, e) => UpdateNextButtonState();
            if (_fullNameTextBox != null)
                _fullNameTextBox.TextChanged += (s, e) => UpdateNextButtonState();
            if (_emailTextBox != null)
                _emailTextBox.TextChanged += (s, e) => UpdateNextButtonState();
            if (_phoneTextBox != null)
                _phoneTextBox.TextChanged += (s, e) => UpdateNextButtonState();

            // Add real-time validation for Page 2 payment methods
            if (_cashRadio != null)
                _cashRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();
            if (_cardRadio != null)
                _cardRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();
            if (_bankTransferRadio != null)
                _bankTransferRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();
            if (_digitalWalletRadio != null)
                _digitalWalletRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();

            // Payment amount option handlers
            if (_payDepositRadio != null)
                _payDepositRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();
            if (_payFullRadio != null)
                _payFullRadio.IsCheckedChanged += (s, e) => UpdateNextButtonState();


            // Add real-time validation for card details
            if (_cardNumberTextBox != null)
                _cardNumberTextBox.TextChanged += (s, e) => UpdateNextButtonState();
            if (_expiryTextBox != null)
                _expiryTextBox.TextChanged += (s, e) => UpdateNextButtonState();
            if (_cvvTextBox != null)
                _cvvTextBox.TextChanged += (s, e) => UpdateNextButtonState();

            // Add Page 3 event handlers for real-time validation
            if (_bundleComboBox != null)
                _bundleComboBox.SelectionChanged += (s, e) => UpdateNextButtonState();
            if (_rentalDurationUpDown != null)
            {
                _rentalDurationUpDown.ValueChanged += async (s, e) => await OnDurationChanged();
                _rentalDurationUpDown.ValueChanged += (s, e) => UpdateNextButtonState();
    
                // Round any decimal input to nearest integer
                _rentalDurationUpDown.ValueChanged += (s, e) =>
                {
                    if (_rentalDurationUpDown.Value.HasValue)
                    {
                        var roundedValue = Math.Round(_rentalDurationUpDown.Value.Value);
                        if (Math.Abs(_rentalDurationUpDown.Value.Value - roundedValue) > (decimal)0.001)
                        {
                            _rentalDurationUpDown.Value = roundedValue;
                        }
                    }
                };
            }
            if (_rentDatePicker != null)
                _rentDatePicker.SelectedDateChanged += (s, e) => UpdateNextButtonState();
        }


        private void UpdateNextButtonState()
        {
            if (_nextButton == null) return;

            bool isValid = false;

            switch (_currentPage)
            {
                case 1:
                    var customerId = _customerIdTextBox?.Text;
                    var fullName = _fullNameTextBox?.Text;
                    var email = _emailTextBox?.Text;
                    var phone = _phoneTextBox?.Text; // Add phone validation

                    // Debug output - remove this later
                    System.Diagnostics.Debug.WriteLine($"Validation - CustomerID: '{customerId}', FullName: '{fullName}', Email: '{email}', Phone: '{phone}'");

                    // Updated validation to include phone number
                    isValid = !string.IsNullOrWhiteSpace(customerId) &&
                              !string.IsNullOrWhiteSpace(fullName) &&
                              !string.IsNullOrWhiteSpace(email) &&
                              !string.IsNullOrWhiteSpace(phone); // Add phone validation here
                    break;
                
                case 2:
                    bool paymentSelected = _cashRadio?.IsChecked == true || 
                                           _cardRadio?.IsChecked == true || 
                                           _bankTransferRadio?.IsChecked == true || 
                                           _digitalWalletRadio?.IsChecked == true;

                    bool paymentAmountSelected = _payDepositRadio?.IsChecked == true ||
                                                 _payFullRadio?.IsChecked == true;

                    if (_cardRadio?.IsChecked == true)
                    {
                        isValid = paymentSelected &&
                                  paymentAmountSelected &&
                                  !string.IsNullOrWhiteSpace(_cardNumberTextBox?.Text) &&
                                  !string.IsNullOrWhiteSpace(_expiryTextBox?.Text) &&
                                  !string.IsNullOrWhiteSpace(_cvvTextBox?.Text);
                    }
                    else
                    {
                        isValid = paymentSelected && paymentAmountSelected;
                    }
                    break;

                case 3:
                    isValid = _bundleComboBox?.SelectedItem != null &&
                              _rentDatePicker?.SelectedDate != null &&
                              (_rentalDurationUpDown?.Value ?? 0) >= 3;
                    break;

                default:
                    isValid = true;
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Next button enabled: {isValid}");
            _nextButton.IsEnabled = isValid;
        }

        private async Task GenerateNextCustomerIdAsync()
        {
            try
            {
                using var connection = new MySqlConnection(Connection.ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT CustomerID FROM customers ORDER BY CustomerID DESC LIMIT 1";
                using var command = new MySqlCommand(query, connection);
                
                var result = await command.ExecuteScalarAsync();
                string nextId;

                if (result == null)
                {
                    nextId = "CUST0001"; // First customer
                }
                else
                {
                    string lastId = result.ToString()!;
                    if (lastId.StartsWith("CUST") && lastId.Length == 8)
                    {
                        if (int.TryParse(lastId.Substring(4), out int lastNumber))
                        {
                            nextId = $"CUST{(lastNumber + 1):D4}";
                        }
                        else
                        {
                            nextId = "CUST0001";
                        }
                    }
                    else
                    {
                        nextId = "CUST0001";
                    }
                }

                if (_customerIdTextBox != null)
                {
                    _customerIdTextBox.Text = nextId;
                    _customerIdTextBox.IsReadOnly = true; // Make it read-only
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error generating customer ID: {ex.Message}");
                if (_customerIdTextBox != null)
                {
                    _customerIdTextBox.Text = "CUST0001";
                    _customerIdTextBox.IsReadOnly = true;
                }
            }
        }

        private async Task<bool> CheckCustomerExistsAsync(string customerId)
        {
            try
            {
                using var connection = new MySqlConnection(Connection.ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM customers WHERE CustomerID = @customerId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@customerId", customerId);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error checking customer existence: {ex.Message}");
                return false;
            }
        }

        private async Task OnBundleSelectionChanged()
        {
            var selectedBundle = _bundleComboBox?.SelectedItem as BundleDisplayItem;
            if (selectedBundle == null)
            {
                ResetCalculatedValues();
                UpdateNextButtonState();
                return;
            }

            // Use the DailyRate directly from the BundleDisplayItem instead of looking it up again
            _bundleDailyRate = selectedBundle.DailyRate;
            _bundleName = selectedBundle.BundleName;
    
            // Debug output to see what's happening
            System.Diagnostics.Debug.WriteLine($"Bundle selected: {selectedBundle.BundleId}, Rate: {_bundleDailyRate}");
    
            CalculateRentalCosts();
            UpdateNextButtonState();
        }


        private async Task OnDurationChanged()
        {
            var selectedBundle = _bundleComboBox?.SelectedItem as BundleDisplayItem;
            if (selectedBundle == null)
            {
                ResetCalculatedValues();
                UpdateNextButtonState();
                return;
            }

            // Use the DailyRate directly from the BundleDisplayItem
            _bundleDailyRate = selectedBundle.DailyRate;
            _bundleName = selectedBundle.BundleName;
    
            CalculateRentalCosts();
            UpdateNextButtonState();
        }



        private double GetDailyRate(string bundleId)
        {
            return BundleDailyRates.ContainsKey(bundleId) ? BundleDailyRates[bundleId] : 0;
        }

        private async Task LoadBundlesAsync()
        {
            try
            {
                using var connection = new MySqlConnection(Connection.ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT BundleID, BundleName, Description, Price FROM bundles ORDER BY BundleName";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var bundles = new List<BundleDisplayItem>();

                while (await reader.ReadAsync())
                {
                    var bundleId = reader.GetString("BundleID");
                    // Use the Price from database as DailyRate instead of hardcoded values
                    var dailyRate = reader.GetDouble("Price");
            
                    bundles.Add(new BundleDisplayItem
                    {
                        BundleId = bundleId,
                        BundleName = reader.GetString("BundleName"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        Price = dailyRate, // Keep this for compatibility
                        DailyRate = dailyRate // Use database price as daily rate
                    });
                }

                if (_bundleComboBox != null)
                {
                    _bundleComboBox.ItemsSource = bundles;
                    _bundleComboBox.DisplayMemberBinding = new Avalonia.Data.Binding("DisplayText");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading bundles: {ex.Message}");
            }
        }

        private void ResetCalculatedValues()
        {
            _bundleDailyRate = 0;
            _bundleName = "";
            if (_rentalFeeLabel != null) _rentalFeeLabel.Text = "₱0.00";
            if (_depositLabel != null) _depositLabel.Text = "₱0.00";
            if (_totalAmountLabel != null) _totalAmountLabel.Text = "₱0.00";
        }

        private void CalculateRentalCosts()
        {
            var duration = _rentalDurationUpDown?.Value ?? 3;

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Calculating costs - Duration: {duration}, Daily Rate: {_bundleDailyRate}");

            // Ensure minimum 3 days
            if (duration < 3) 
            {
                duration = 3;
                if (_rentalDurationUpDown != null) _rentalDurationUpDown.Value = 3;
            }

            // Calculate the full rental fee
            var fullRentalFee = _bundleDailyRate * (double)duration;
    
            // totalRentalFee is what the customer pays upfront
            var totalRentalFee = 0.0;
    
            if (_payFullRadio?.IsChecked == true)  
            {
                totalRentalFee = fullRentalFee; // Pay full amount upfront
            }
            else if (_payDepositRadio?.IsChecked == true)
            {
                totalRentalFee = fullRentalFee * 0.50; // Pay 50% deposit upfront
            }

            // For display purposes, show what they're paying
            var totalAmount = totalRentalFee;

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Calculated - Full Rental Fee: {fullRentalFee}, Upfront Payment: {totalRentalFee}");

            // Update the UI labels with peso sign formatting
            if (_rentalFeeLabel != null) 
            {
                _rentalFeeLabel.Text = $"₱{fullRentalFee:N2}";
                System.Diagnostics.Debug.WriteLine($"Updated rental fee label to: {_rentalFeeLabel.Text}");
            }
            if (_depositLabel != null) 
            {
                // Show what they're actually paying upfront
                _depositLabel.Text = $"₱{totalRentalFee:N2}";
                System.Diagnostics.Debug.WriteLine($"Updated deposit label to: {_depositLabel.Text}");
            }
            if (_totalAmountLabel != null) 
            {
                _totalAmountLabel.Text = $"₱{totalAmount:N2}";
                System.Diagnostics.Debug.WriteLine($"Updated total label to: {_totalAmountLabel.Text}");
            }
        }


        private async Task<BundleInfo?> GetBundleInformation(string bundleId)
        {
            try
            {
                using var connection = new MySqlConnection(Connection.ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT BundleName, Description, Price FROM bundles WHERE BundleID = @bundleId";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@bundleId", bundleId);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new BundleInfo
                    {
                        Name = reader.GetString("BundleName"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        Price = reader.GetDouble("Price")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error retrieving bundle information: {ex.Message}");
                return null;
            }
        }

        private void UpdateCardDetailsVisibility()
        {
            if (_cardDetailsPanel != null) _cardDetailsPanel.IsVisible = _cardRadio?.IsChecked == true;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePageVisibility();
                UpdateNavigationButtons();
                UpdateHeader();
            }
        }

        private async void NextButton_Click(object? sender, RoutedEventArgs e)
        {
            if (await ValidateCurrentPageAsync())
            {
                if (_currentPage < TotalPages)
                {
                    _currentPage++;

                    if (_currentPage == 3)
                    {
                        await LoadBundlesAsync();
                    }

                    if (_currentPage == 4)
                    {
                        PopulateReviewPage();
                    }

                    UpdatePageVisibility();
                    UpdateNavigationButtons();
                    UpdateHeader();
                }
            }
        }

        private async void FinishButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_termsCheckBox?.IsChecked != true)
            {
                ShowMessage("Please accept the terms and conditions to continue.");
                return;
            }

            try
            {
                await SaveCustomerAndTransactionAsync();
                ShowMessage("Purchase completed successfully!");
                Close();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error completing purchase: {ex.Message}");
            }
        }

        private async Task<bool> ValidateCurrentPageAsync()
        {
            switch (_currentPage)
            {
                case 1:
                    if (string.IsNullOrWhiteSpace(_customerIdTextBox?.Text) ||
                        string.IsNullOrWhiteSpace(_fullNameTextBox?.Text) ||
                        string.IsNullOrWhiteSpace(_emailTextBox?.Text))
                    {
                        ShowMessage("Please fill in all required customer information fields.");
                        return false;
                    }

                    // Check if customer already exists
                    if (await CheckCustomerExistsAsync(_customerIdTextBox!.Text))
                    {
                        ShowMessage("A customer with this ID already exists. Please use a different ID.");
                        return false;
                    }
                    break;

                case 2:
                    if (!(_cashRadio?.IsChecked == true || _cardRadio?.IsChecked == true || 
                          _bankTransferRadio?.IsChecked == true || _digitalWalletRadio?.IsChecked == true))
                    {
                        ShowMessage("Please select a payment method.");
                        return false;
                    }

                    if (!(_payDepositRadio?.IsChecked == true || _payFullRadio?.IsChecked == true))
                    {
                        ShowMessage("Please select whether to pay deposit or full amount.");
                        return false;
                    }

                    if (_cardRadio?.IsChecked == true)
                    {
                        if (string.IsNullOrWhiteSpace(_cardNumberTextBox?.Text) ||
                            string.IsNullOrWhiteSpace(_expiryTextBox?.Text) ||
                            string.IsNullOrWhiteSpace(_cvvTextBox?.Text))
                        {
                            ShowMessage("Please fill in all card details.");
                            return false;
                        }
                    }
                    break;

                case 3:
                    if (_bundleComboBox?.SelectedItem == null ||
                        _rentDatePicker?.SelectedDate == null)
                    {
                        ShowMessage("Please select a bundle and rental date.");
                        return false;
                    }

                    var duration = _rentalDurationUpDown?.Value ?? 0;
                    if (duration < 3)
                    {
                        ShowMessage("Minimum rental duration is 3 days.");
                        return false;
                    }
                    break;
            }

            return true;
        }


        
        private void UpdateNavigationButtons()
        {
            if (_backButton != null) _backButton.IsEnabled = _currentPage > 1;
            if (_nextButton != null) _nextButton.IsVisible = _currentPage < TotalPages;
            if (_finishButton != null) _finishButton.IsVisible = _currentPage == TotalPages;
    
            // Update the Next button state based on current page validation
            UpdateNextButtonState();
        }

        private void UpdatePageVisibility()
        {
            for (int i = 0; i < _pages.Length; i++)
            {
                _pages[i]!.IsVisible = (i + 1) == _currentPage;
            }
    
            // Update button state when page changes
            UpdateNextButtonState();
        }


        private void UpdateHeader()
        {
            string[] headers = {
                "Customer Information",
                "Payment Method", 
                "Rental Details",
                "Review & Confirm"
            };

            if (_headerText != null) _headerText.Text = headers[_currentPage - 1];
            if (_stepIndicator != null) _stepIndicator.Text = $"Step {_currentPage} of {TotalPages}";
        }

        private void PopulateReviewPage()
        {
            _reviewPanel?.Children.Clear();

            var selectedBundle = _bundleComboBox?.SelectedItem as BundleDisplayItem;

            // Customer Info
            AddReviewSection("Customer Information");
            AddReviewItem("Customer ID", _customerIdTextBox?.Text);
            AddReviewItem("Full Name", _fullNameTextBox?.Text);
            AddReviewItem("Email", _emailTextBox?.Text);
            AddReviewItem("Phone", _phoneTextBox?.Text);

            // Payment Method
            AddReviewSection("Payment Information");
            string paymentMethod = GetSelectedPaymentMethod();
            AddReviewItem("Payment Method", paymentMethod);
            AddReviewItem("Payment Amount", GetSelectedPaymentAmount());

            // Rental Details
            AddReviewSection("Rental Details");
            AddReviewItem("Bundle ID", selectedBundle?.BundleId);
            AddReviewItem("Bundle Name", selectedBundle?.BundleName);
            AddReviewItem("Daily Rate", $"₱{selectedBundle?.DailyRate:N2}");
            AddReviewItem("Duration", $"{_rentalDurationUpDown?.Value} days");
            if (_rentDatePicker != null)
                AddReviewItem("Start Date", _rentDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
            AddReviewItem("End Date", CalculateEndDate());
            AddReviewItem("Venue", _venueAddressTextBox?.Text ?? "Not specified"); // Add venue information
            
            // Add notes if provided
            if (!string.IsNullOrWhiteSpace(_notesTextBox?.Text))
            {
                AddReviewItem("Additional Notes", _notesTextBox?.Text);
            }

            var duration = _rentalDurationUpDown?.Value ?? 3;
            var totalRentalFee = _bundleDailyRate * (double)duration;
            var deposit = totalRentalFee * 0.20; // 20% deposit as per DDL

            AddReviewItem("Rental Fee", $"₱{totalRentalFee:N2}");
            AddReviewItem("Security Deposit (50%)", $"₱{deposit:N2}");

            // Total
            AddReviewSection("Total");
            var totalAmount = totalRentalFee;
            AddReviewItem("Total Amount", $"₱{totalAmount:N2}");
        }


        private void AddReviewSection(string title)
        {
            var sectionHeader = new TextBlock
            {
                Text = title,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 15, 0, 10)
            };
            _reviewPanel?.Children.Add(sectionHeader);
        }

        private void AddReviewItem(string label, string? value)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            var labelBlock = new TextBlock
            {
                Text = label + ":",
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            Grid.SetColumn(labelBlock, 0);

            var valueBlock = new TextBlock
            {
                Text = value ?? ""
            };
            Grid.SetColumn(valueBlock, 1);

            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
            _reviewPanel?.Children.Add(grid);
        }

        private string GetSelectedPaymentMethod()
        {
            if (_cashRadio?.IsChecked == true) return "Cash";
            if (_cardRadio?.IsChecked == true) return "Credit/Debit Card";
            if (_bankTransferRadio?.IsChecked == true) return "Bank Transfer";
            if (_digitalWalletRadio?.IsChecked == true) return "Digital Wallet";
            return "Not Selected";
        }

        private int GetSelectedPaymentMethodCode()
        {
            if (_cashRadio?.IsChecked == true) return 0; // Cash
            if (_cardRadio?.IsChecked == true) return 1; // Card
            if (_bankTransferRadio?.IsChecked == true) return 2; // Bank Transfer
            if (_digitalWalletRadio?.IsChecked == true) return 3; // Digital Wallet
            return 0; // Default to Cash
        }

        private string CalculateEndDate()
        {
            if (_rentDatePicker != null && _rentDatePicker.SelectedDate.HasValue)
            {
                if (_rentalDurationUpDown != null)
                {
                    var endDate = _rentDatePicker.SelectedDate.Value.AddDays((double)(_rentalDurationUpDown.Value ?? 3));
                    return endDate.ToString("yyyy-MM-dd");
                }
            }
            return "";
        }

        private async Task SaveCustomerAndTransactionAsync()
{
    using var connection = new MySqlConnection(Connection.ConnectionString);
    await connection.OpenAsync();
    using var transaction = await connection.BeginTransactionAsync();

    try
    {
        // Insert customer with full name stored as FirstName
        var customerQuery = @"
            INSERT INTO customers (CustomerID, FirstName, LastName, PhoneNumber, Email, Notes) 
            VALUES (@customerId, @fullName, '', @phone, @email, '')";

        using var customerCommand = new MySqlCommand(customerQuery, connection, transaction);
        customerCommand.Parameters.AddWithValue("@customerId", _customerIdTextBox?.Text);
        customerCommand.Parameters.AddWithValue("@fullName", _fullNameTextBox?.Text);
        customerCommand.Parameters.AddWithValue("@phone", _phoneTextBox?.Text ?? "");
        customerCommand.Parameters.AddWithValue("@email", _emailTextBox?.Text);

        await customerCommand.ExecuteNonQueryAsync();

        // Calculate rental costs
        var selectedBundle = _bundleComboBox?.SelectedItem as BundleDisplayItem;
        var duration = _rentalDurationUpDown?.Value ?? 3;
        var fullRentalFee = _bundleDailyRate * (double)duration;
        
        // totalRentalFee is the upfront payment amount
        var totalRentalFee = 0.0;
        var isPayingFull = IsPayingFullAmount();
        
        if (isPayingFull)
        {
            totalRentalFee = fullRentalFee; // Pay full amount upfront
        }
        else
        {
            totalRentalFee = fullRentalFee * 0.50; // Pay 50% deposit upfront
        }

        var transactionId = await GenerateTransactionIdAsync(connection, transaction);

        var transactionQuery = @"
            INSERT INTO transactions 
            (TransactionID, Method, Amount, Deposit, RentalFee, RentDate, RentDuration, RentEndDate, BundleID, CustomerID) 
            VALUES (@transactionId, @method, @amount, @deposit, @rentalFee, @rentDate, @duration, @endDate, @bundleId, @customerId)";

        using var transCommand = new MySqlCommand(transactionQuery, connection, transaction);
        transCommand.Parameters.AddWithValue("@transactionId", transactionId);
        transCommand.Parameters.AddWithValue("@method", GetSelectedPaymentMethodCode());
        transCommand.Parameters.AddWithValue("@amount", fullRentalFee); // Upfront payment amount
        transCommand.Parameters.AddWithValue("@deposit", isPayingFull ? 1 : 0); // 1 = full payment, 0 = deposit only
        transCommand.Parameters.AddWithValue("@rentalFee", totalRentalFee); // Total rental fee for the duration
        transCommand.Parameters.AddWithValue("@rentDate", _rentDatePicker?.SelectedDate?.ToString("yyyy-MM-dd"));
        transCommand.Parameters.AddWithValue("@duration", (int)duration);
        transCommand.Parameters.AddWithValue("@endDate", CalculateEndDate());
        transCommand.Parameters.AddWithValue("@bundleId", selectedBundle?.BundleId);
        transCommand.Parameters.AddWithValue("@customerId", _customerIdTextBox?.Text);

        await transCommand.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}


        private async Task<string> GenerateTransactionIdAsync(MySqlConnection connection, MySqlTransaction transaction)
        {
            var query = "SELECT TransactionID FROM transactions ORDER BY TransactionID DESC LIMIT 1";
            using var command = new MySqlCommand(query, connection, transaction);
    
            var result = await command.ExecuteScalarAsync();
    
            if (result == null)
            {
                return "TXN00001"; // First transaction
            }
            else
            {
                string lastId = result.ToString()!;
                if (lastId.StartsWith("TXN") && lastId.Length == 8)
                {
                    if (int.TryParse(lastId.Substring(3), out int lastNumber))
                    {
                        return $"TXN{(lastNumber + 1):D5}";
                    }
                }
                return "TXN00001";
            }
        }

        private async void ShowMessage(string message)
        {
            var messageBox = new Window
            {
                Title = "Information",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 15
            };

            content.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14
            });

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (s, e) => messageBox.Close();
            content.Children.Add(okButton);

            messageBox.Content = content;
            await messageBox.ShowDialog(this);
        }
    }

    public class BundleDisplayItem
    {
        public string BundleId { get; set; } = string.Empty;
        public string BundleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public double DailyRate { get; set; }

        // This property is used for display binding in the ComboBox
        public string DisplayText => $"{BundleName} - ₱{DailyRate:N2}/day";
    }

    

    // Helper class for bundle information
    public class BundleInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Price { get; set; }
    }
}